using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using QFSW.QC;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ChessBoard : NetworkBehaviour
{
    [SerializeField] private BoardMaterials Prefabs;
    private NetworkGameManager NetworkGameManager_S;

    private static Vector3 DEAD_PIECE = new Vector3(-100f, -100f, -100f);
    
    public const int P1 = 1;
    public const int P2 = 0;
    public const int P1_PIECE = 1;
    public const int P2_PIECE = 2;
    public const int P1_DUKE = 3;
    public const int P2_DUKE = 4;

    private int P1_PieceCount = 12;
    private int P2_PieceCount = 12;
    public static bool IsP1Turn { get; private set; }
    
    public static Checker[] Board = new Checker[64];
    // public NetworkVariable<Checker[]> Board_Net = new NetworkVariable<Checker[]>(new Checker[64], NVRP.Everyone, NVWP.Server);

    public NetworkVariable<bool> IsP1Turn_Net = new NetworkVariable<bool>(true, NVRP.Everyone, NVWP.Server);

    // public override void OnNetworkSpawn()
    // {
    //     base.OnNetworkSpawn();

    //     IsP1Turn_Net.OnValueChanged += (bool previousVal, bool newVal) => {
    //         print(OwnerClientId + " | " + IsP1Turn_Net.Value + " " + newVal);
    //         IsP1Turn = IsP1Turn_Net.Value;
    //     };
    // }

    private void Start()
    {
        GenerateBoardTiles();

        NetworkGameManager_S = GameObject.Find("NetworkGameManager").GetComponent<NetworkGameManager>();

        // StartGame();  
    }
    

    public void StartGame()
    {
        GeneratePieces();

        IsP1Turn = true;

        SetValidMovesForPieces();
    }


    private void GenerateBoardTiles()
    {
        for (int z=0; z<8; ++z)
            for (int x=0; x<8; ++x)
                Instantiate(Prefabs.TitlePrefab, new Vector3(x,0,z), Quaternion.identity);
    }

    private void GeneratePieces()
    {
        float y = Checker.Y;
        PlacePieces(forP1: false, 5, 8);
        PlacePieces(forP1: true, 0, 3);

        void PlacePieces(bool forP1, int zMin, int zMax){
            IsP1Turn = forP1;
            for (int z=zMin; z<zMax; ++z)
                for (int x=z%2; x<8; x+=2)
                    Instantiate(Prefabs.CheckerPrefab, new Vector3(x,y,z), Quaternion.identity);
        }
        
        
    }

    public void ChangeSides()
    {
        IsP1Turn ^= true;
        SetValidMovesForPieces();
        CheckForGameOver();
    }

    private static void SetValidMovesForPieces()
    {
        List<Checker> allPieces = new List<Checker>();
        List<Checker> allPiecesJump = new List<Checker>();
        foreach (Checker piece in Board)
        {
            if (piece == null)
                continue;
            
            if ((!IsP1Turn && piece.TeamID%2 == P1) || (IsP1Turn && piece.TeamID%2 == P2)) // If it is other player's piece
                piece.ClearValidMoves();
            else
            {
                piece.SetValidMoves();
                CheckForJumpingMovesAndAdd(piece);
                
                allPieces.Add(piece);
            }
        }

        if (allPiecesJump.Count > 0)
            ExcludeNonJumpMoves();


        void CheckForJumpingMovesAndAdd(Checker piece)
        {
            List<int> jumpingMoves = new List<int>();
            int currentPos = PosToBoardPos(piece.PreviousPosition);
            foreach (int move in piece.ValidMoves)
                if (Math.Abs(currentPos - move) > 9)
                    jumpingMoves.Add(move);
            
            if (jumpingMoves.Count > 0)
            {
                allPiecesJump.Add(piece);
                piece.SetValidMoves(jumpingMoves);
            }
        }
        void ExcludeNonJumpMoves()
        {
            foreach (Checker piece in allPieces)
                if (!HaveJumpingMove(piece))
                    piece.ClearValidMoves();                
        }
        bool HaveJumpingMove(Checker currentPiece)
        {
            foreach (Checker piece in allPiecesJump)
                if (currentPiece == piece)
                    return true;
            return false;
        }
    }

    private void CheckForGameOver()
    {
        if (P1_PieceCount <= 0)
            print("Player 2 WINS!");
        else if (P2_PieceCount <= 0)
            print("Player 1 WINS!");

        foreach (Checker piece in Board)
        {
            if (!piece)
                continue;

            if (piece.ValidMoves.Count > 0)
                return;
        }
            

        print(IsP1Turn ? "Player 2 WINS!" : "Player 1 WINS!");
    }

    public void UpdateBoard(Vector3 previousPos, Vector3 currentPos)
    {
        int oldPos = PosToBoardPos(previousPos);
        int newPos = PosToBoardPos(currentPos);

        if (OnPromotionRow(newPos) && !PieceIsDuke(Board[oldPos]))
            PromotePiece();
        else
            Board[newPos] = Board[oldPos];
        Board[oldPos] = null;

        if (Mathf.Abs(newPos - oldPos) > 9)
            RemoveCapturedPiece((newPos+oldPos)/2);

        return;

        void PromotePiece(){
            Destroy(Board[oldPos].gameObject);
            Vector3 newTransform = BoardPosToPos(newPos);
            Board[newPos] = Instantiate(Prefabs.DukePrefab, newTransform, Quaternion.identity);

        }
        void RemoveCapturedPiece(int index){
            Board[index].transform.position = DEAD_PIECE;
            Destroy(Board[index].gameObject);
            DecrementPlayerCount(index);
            Board[index] = null;
        }
        bool OnPromotionRow(int pos){return (pos < 8 && IsP1Turn) || (pos > 55 && !IsP1Turn);}
        bool PieceIsDuke(Checker piece){return piece.TeamID == P1_DUKE || piece.TeamID == P2_DUKE;}
            
    }

    public void ClearAllPiecesValidMoves()
    {
        foreach(Checker piece in Board)
            piece?.ClearValidMoves();
    }

    private void DecrementPlayerCount(int index)
    {
        if (Board[index].TeamID%2 == P1_PIECE)
            --P1_PieceCount;
        else
            --P2_PieceCount;
    }



    [Command]
    public void ValidateGame()
    {
        FixedString64Bytes notation = GetGameState();
        ValidateGame_ServerRpc(notation);
    }
    [ServerRpc]
    private void ValidateGame_ServerRpc(FixedString64Bytes notation)
    {
        bool boardValidity = NetworkGameManager_S.ValidateBoard(notation);

        if (!boardValidity)
            print("CHEATER!");
    }

    public void SetGameState()
    {
        FixedString64Bytes gameState = GetGameState();
        SetGameState_ServerRpc(gameState);
    }
    [ServerRpc]
    private void SetGameState_ServerRpc(FixedString64Bytes notation)
    {
        NetworkGameManager_S.SetBoardState(notation);
    }

    private FixedString64Bytes GetGameState()
    {
        char[] AlexNotation = new char[32];
        Array.Fill(AlexNotation, '0');

        GameObject[] allPieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (GameObject piece in allPieces)
        {
            Checker checker = piece.GetComponent<Checker>();
            if (checker.transform.position == DEAD_PIECE)
                continue;

            int index = PosToBoardPos(checker.transform.position)/2;
            AlexNotation[index] = (char)(checker.TeamID + '0');
        }
        return new FixedString64Bytes(new string(AlexNotation));
    }





    public static int PosToBoardPos(Vector3 pos)
    {
        return (int) (((7-pos.z)*8)+pos.x);
    }
    public static Vector3 BoardPosToPos(int pos)
    {
        int x = pos%8;
        int z = 7-(pos/8);
        return new Vector3(x, Checker.Y, z);
    }


    [ClientRpc]
    public void UpdatePos_ClientRpc(ulong callerID, Vector3 currentPos, Vector3 newPos)
    {
        print($"{OwnerClientId}");
        if (OwnerClientId == callerID)
            return;

        GameObject[] allPieces = GameObject.FindGameObjectsWithTag("Piece");

        foreach (GameObject piece in allPieces)
        {
            if (piece.transform.position == currentPos)
            {
                UpdateBoard(piece.transform.position, newPos);

                piece.transform.position = newPos;
                piece.GetComponent<Checker>().UpdatePreviousPos(newPos);
            }
        }
    }




    [ClientRpc]
    private void SetP1Val_ClientRpc(bool p1Turn)
    {
        IsP1Turn = p1Turn;
        print(IsP1Turn);
    }







    [Command]
    public void PrintBoard()
    {
        string line = "";
        int x = 0;
        foreach (Checker piece in Board)
        {
            string character = (!piece) ? "0 " : $"{piece.TeamID} ";
            line += $"{character} ";
            if (++x > 7)
            {
                line += "\n";
                x = 0;
            }
        }
        print(line);
    }
    [Command]
    public void PrintValidMoves()
    {
        foreach (Checker piece in Board)
        {
            if (!piece)
                continue;

            print($"{PosToBoardPos(piece.transform.position)}: {piece.ValidMoves.Count}");
        }
    }
    [Command]
    public void PrintData()
    {
        print($"P1_Count: {P1_PieceCount}");
        print($"P2_Count: {P2_PieceCount}");
        print($"Player 1 Turn: {IsP1Turn}");
    }
    [Command]
    public void ChangeP1Turn()
    {
        IsP1Turn_Net.Value ^= true;
    }

    public static class NVRP
    {
        public static NetworkVariableReadPermission Owner => NetworkVariableReadPermission.Owner;
        public static NetworkVariableReadPermission Everyone => NetworkVariableReadPermission.Everyone;
    }
    public static class NVWP
    {
        public static NetworkVariableWritePermission Server => NetworkVariableWritePermission.Server;
        public static NetworkVariableWritePermission Owner => NetworkVariableWritePermission.Owner;
    }
}
