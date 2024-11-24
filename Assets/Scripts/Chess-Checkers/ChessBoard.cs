using System;
using System.Collections.Generic;
using QFSW.QC;
using TMPro;
using Unity.Netcode;
using UnityEngine;

using NV_Bool = Unity.Netcode.NetworkVariable<bool>;
using NV_String64B = Unity.Netcode.NetworkVariable<Unity.Collections.FixedString64Bytes>;

public class ChessBoard : NetworkBehaviour
{
    [SerializeField] private BoardMaterials Board_SO;
    [SerializeField] private VictoryScreen VictoryScreen_S;

    private static Vector3 DEAD_PIECE = new Vector3(-100f, -100f, -100f);
    
    public static GenericPiece[] Board = new GenericPiece[64];

    public event Action<bool> OnChangedTurn;
    public event Action<bool> OnGameOver;

    // public static NV_String64B Board_Net = new NV_String64B("", NVRP.Everyone, NVWP.Server);

    void Start()
    {
        GenerateBoardTiles();
        // StartGame();
    }

    // public override void OnNetworkSpawn()
    // {
    //     if (!IsServer) return;
        
    //     Board_Net.Value = "cccccccccccc00000000CCCCCCCCCCCC";

    //     Board_Net.OnValueChanged += (FixedString64Bytes previousVal, FixedString64Bytes newVal) => {
    //         print("Old Value ::: "+ previousVal);
    //         print("New Value ::: "+ newVal);
    //     };
    // }

    public void StartGame()
    {
        OnChangedTurn?.Invoke(BoardMaterials.IsP1Turn);

        switch (BoardMaterials.GameType)
        {
            case BoardMaterials.CHECKERS_GAME: GeneratePieces(BoardMaterials.CheckersSetup); break;
            case BoardMaterials.CHESS_GAME: GeneratePieces(BoardMaterials.ChessSetup); break;
        }

        SetValidMovesForPieces();
    }

    public void ResetGame()
    {
        VictoryScreen_S.gameObject.SetActive(false);

        StartGame();
    }

    private void GenerateBoardTiles()
    {
        Instantiate(Board_SO.BoardEdgePrefab);
        for (int z=0; z<8; ++z)
            for (int x=0; x<8; ++x)
                Instantiate(Board_SO.TitlePrefab, new Vector3(x,0,z), Quaternion.identity);
    }

    private void GeneratePieces(string layout)
    {
        float y = GenericPiece.Y;
        int x = 0;
        int z = 7;
        foreach (char c in layout)
        {
            GenericPiece piece = null;
            switch (c)
            {
                case 'c': case 'C': piece = Instantiate(Board_SO.CheckerPrefab, new Vector3(x,y,z), Quaternion.identity); break;
                case 'd': case 'D': piece = Instantiate(Board_SO.DukePrefab, new Vector3(x,y,z), Quaternion.identity); break;
                case 'p': case 'P': piece = Instantiate(Board_SO.PawnPrefab, new Vector3(x,y,z), Quaternion.identity); break;
                case 'q': case 'Q': piece = Instantiate(Board_SO.QueenPrefab, new Vector3(x,y,z), Quaternion.identity); break;
                case 'r': case 'R': piece = Instantiate(Board_SO.RookPrefab, new Vector3(x,y,z), Quaternion.identity); break;
                case 'n': case 'N': piece = Instantiate(Board_SO.KnightPrefab, new Vector3(x,y,z), Quaternion.identity); break;
                case 'b': case 'B': piece = Instantiate(Board_SO.BishopPrefab, new Vector3(x,y,z), Quaternion.identity); break;
                case 'k': case 'K': piece = Instantiate(Board_SO.KingPrefab, new Vector3(x,y,z), Quaternion.identity); break;
                
                case '/': --z; x=-1; break;
                default: x += c-'0'-1; break; // Takes number an increment by whitespace in-between
            }
            piece?.InstantiatePieceComponents(forP1: GenericPiece.IsP1Piece(c));

            if (++x >= 8)
                x = 0;
        }
    }

    public void ChangeSides()
    {
        BoardMaterials.IsP1Turn ^= true;
        UpdateBoard();
        SetValidMovesForPieces();
        CheckForGameOver();

        OnChangedTurn?.Invoke(BoardMaterials.IsP1Turn);
    }

    private static void SetValidMovesForPieces()
    {
        List<GenericPiece> allPieces = new List<GenericPiece>();
        List<Checker> allPiecesJump = new List<Checker>();
        foreach (GenericPiece piece in Board)
        {
            if (piece == null)
                continue;
            
            if ((!BoardMaterials.IsP1Turn && GenericPiece.IsP1Piece(piece)) || (BoardMaterials.IsP1Turn && GenericPiece.IsP2Piece(piece)))
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
        return;

        void CheckForJumpingMovesAndAdd(GenericPiece piece)
        {
            if (piece is not Checker checker) return;

            List<int> jumpingMoves = new List<int>();

            int currentPos = PosToBoardPos(piece.PreviousPosition);
            foreach (int move in piece.ValidMoves)
                if (Math.Abs(currentPos - move) > 9)
                    jumpingMoves.Add(move);
            
            if (jumpingMoves.Count > 0)
            {
                allPiecesJump.Add(checker);
                piece.SetValidMoves(jumpingMoves);
            }
        }
        void ExcludeNonJumpMoves()
        {
            foreach (GenericPiece piece in allPieces)
                if (piece is not Checker checker || !HaveJumpingMove(checker))
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
        bool p1HasMove = false;
        bool p2HasMove = false;

        foreach (GenericPiece piece in Board)
        {
            if (!piece)
                continue;

            if (!p1HasMove && GenericPiece.IsP1Piece(piece) && piece.ValidMoves.Count > 0)
                p1HasMove = true;
            else if (!p2HasMove && GenericPiece.IsP2Piece(piece) && piece.ValidMoves.Count > 0)
                p2HasMove = true;
        }

        if (!p1HasMove && !p2HasMove)
        {
            OnGameOver(BoardMaterials.IsP1Turn);
            VictoryScreen_S.gameObject.SetActive(true);
            VictoryScreen_S.GetComponentInChildren<TextMeshProUGUI>().text = BoardMaterials.IsP1Turn ? "Player 2 Wins!" : "Player 1 Wins!";
            RemoveAllPieces();
        }
    }

    public void SendMoveToServer(Vector3 oldPos, Vector3 newPos)
    {
        if (BoardMaterials.IsLocalGame) return;
        
        ulong senderID = NetworkManager.Singleton.LocalClientId;
        SendMove_ServerRPC(senderID, oldPos, newPos);
        // SetP1Val_ServerRpc();
    }

    public void UpdateBoard()
    {
        for (int i=0; i<Board.Length; ++i)
            Board[i] = null;
        foreach (GameObject piece in GameObject.FindGameObjectsWithTag("Piece"))
            if (piece.transform.position != DEAD_PIECE)
                Board[PosToBoardPos(piece.transform.position)] = piece.GetComponent<GenericPiece>();
    }

    public void DisableEnPassantForEachPawn()
    {
        foreach (GenericPiece piece in Board)
        {
            if (piece is Pawn pawn)
            {
                pawn.CanEnPassantLeft = false;
                pawn.CanEnPassantRight = false;
            }
        }
    } 

    private void RemoveAllPieces()
    {
        foreach (GenericPiece piece in Board)
            if (piece)
                RemovePiece(PosToBoardPos(piece.transform.position));
    }

    public void RemovePiece(int index)
    {
        Board[index].transform.position = DEAD_PIECE;
        Destroy(Board[index].gameObject);
        Board[index] = null;
    }
    public void CreatePiece(GenericPiece prefab, Vector3 position)
    {
        GenericPiece piece = Instantiate(prefab, position, Quaternion.identity);
        piece.InstantiatePieceComponents(forP1: BoardMaterials.IsP1Turn);
    }

    public void ClearAllPiecesValidMoves()
    {
        foreach(GenericPiece piece in Board)
            piece?.ClearValidMoves();
    }
    public static int PosToBoardPos(Vector3 pos)
    {
        return (int) (((7-pos.z)*8)+pos.x);
    }
    public static Vector3 BoardPosToPos(int pos)
    {
        int x = pos%8;
        int z = 7-(pos/8);
        return new Vector3(x, GenericPiece.Y, z);
    }



    // private FixedString64Bytes GetGameState()
    // {
    //     char[] AlexNotation = new char[32];
    //     Array.Fill(AlexNotation, '0');

    //     GameObject[] allPieces = GameObject.FindGameObjectsWithTag("Piece");
    //     foreach (GameObject piece in allPieces)
    //     {
    //         Checker checker = piece.GetComponent<Checker>();
    //         if (checker.transform.position == DEAD_PIECE)
    //             continue;

    //         int index = PosToBoardPos(checker.transform.position)/2;
    //         AlexNotation[index] = checker.TeamID;
    //     }
    //     return new FixedString64Bytes(new string(AlexNotation));
    // }





    // [ServerRpc(RequireOwnership = false)]
    // private void SetP1Val_ServerRpc()
    // {
    //     IsP1Turn_Net.Value ^= true;
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetP1Val_ServerRpc(bool p1Turn)
    // {
    //     IsP1Turn_Net.Value = p1Turn;
    // }

    [ServerRpc(RequireOwnership = false)]
    public void SendMove_ServerRPC(ulong senderID, Vector3 oldPos, Vector3 newPos)
    {
        UpdatePos_ClientRpc(senderID, oldPos, newPos);
    }

    [ClientRpc]
    public void UpdatePos_ClientRpc(ulong callerID, Vector3 oldPos, Vector3 newPos)
    {
        if (callerID == NetworkManager.Singleton.LocalClientId)
            return;

        GameObject[] allPieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (GameObject piece in allPieces)
            if (piece.transform.position == oldPos)
                piece.GetComponent<GenericPiece>().ProcessTurnLocally(oldPos, newPos);
    }



    [Command]
    public void PrintBoard()
    {
        string line = "";
        int x = 0;
        foreach (GenericPiece piece in Board)
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
        foreach (GenericPiece piece in Board)
        {
            if (!piece)
                continue;

            print($"{PosToBoardPos(piece.transform.position)}: {piece.ValidMoves.Count}");
        }
    }
    [Command]
    public void PrintData()
    {
        print($"Player 1 Turn: {BoardMaterials.IsP1Turn}");
        print("ID: " + NetworkManager.Singleton.LocalClientId);
        print("IsLocalGame: " + BoardMaterials.IsLocalGame);
        // print($"Player 1 Turn: {IsP1Turn_Net.Value}");
    }
    [Command]
    public void ChangeP1Turn()
    {
        BoardMaterials.IsP1Turn ^= true;
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
