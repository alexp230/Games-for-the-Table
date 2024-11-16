using System;
using System.Collections.Generic;
using QFSW.QC;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

using NV_Bool = Unity.Netcode.NetworkVariable<bool>;
using NV_String64B = Unity.Netcode.NetworkVariable<Unity.Collections.FixedString64Bytes>;

public class ChessBoard : NetworkBehaviour
{
    [SerializeField] private BoardMaterials Board_SO;

    private static Vector3 DEAD_PIECE = new Vector3(-100f, -100f, -100f);
    
    private int P1_PieceCount = 12;
    private int P2_PieceCount = 12;
    
    public static bool IsLocalGame = false;
    public static GenericPiece[] Board = new GenericPiece[64];
    public static bool IsP1Turn = true;

    public static NV_Bool IsP1Turn_Net = new NV_Bool(true, NVRP.Everyone, NVWP.Server);
    public static NV_String64B Board_Net = new NV_String64B("", NVRP.Everyone, NVWP.Server);

    private void Start()
    {
        GenerateBoardTiles();
        // StartGame();  
    }

    public override void OnNetworkSpawn()
    {
        Board_Net.Value = "cccccccccccc00000000CCCCCCCCCCCC";

        Board_Net.OnValueChanged += (FixedString64Bytes previousVal, FixedString64Bytes newVal) => {
            print("Old Value ::: "+ previousVal);
            print("New Value ::: "+ newVal);
        };
    }

    public void StartGame()
    {
        GeneratePieces(Board_SO.ChessSetup);
        // GeneratePieces(Board_SO.CheckersSetup);

        SetValidMovesForPieces();
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
        SetP1Val_ServerRpc();
        ChangeSides_ServerRPC();
    }

    private static void SetValidMovesForPieces()
    {
        List<GenericPiece> allPieces = new List<GenericPiece>();
        List<Checker> allPiecesJump = new List<Checker>();
        foreach (GenericPiece piece in Board)
        {
            if (piece == null)
                continue;
            
            if ((!IsP1Turn && GenericPiece.IsP1Piece(piece)) || (IsP1Turn && GenericPiece.IsP2Piece(piece)))
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
        if (P1_PieceCount <= 0)
            print("Player 2 WINS!");
        else if (P2_PieceCount <= 0)
            print("Player 1 WINS!");

        foreach (GenericPiece piece in Board)
        {
            if (!piece)
                continue;

            if (piece.ValidMoves.Count > 0)
                return;
        }
        print(IsP1Turn ? "Player 2 WINS!" : "Player 1 WINS!");
    }

    public void SendMoveToServer(Vector3 currentPos, Vector3 newPos)
    {
        ulong senderID = NetworkManager.Singleton.LocalClientId;
        SendMove_ServerRPC(senderID, currentPos, newPos);
    }

    public void UpdateBoard(Vector3 previousPos, Vector3 currentPos)
    {
        int oldPos = PosToBoardPos(previousPos);
        int newPos = PosToBoardPos(currentPos);

        DisableEnPassantForEachPawn();

        GenericPiece piece = Board[oldPos];
        if (piece is Checker)
        {
            if (OnPromotionRow(newPos) && piece is not Duke)
                PromotePiece();
            else
                Board[newPos] = piece;

            if (Mathf.Abs(newPos - oldPos) > 9) // if jumped piece
                RemoveCapturedPiece((newPos+oldPos)/2);
        }
        else if (piece is Pawn pawn)
        {
            SetPotentialEnPassant(pawn);

            if (Board[newPos] != null)
                RemoveCapturedPiece(newPos);
            else if (Mathf.Abs(newPos-oldPos) == 9 || (Mathf.Abs(newPos-oldPos) == 7)) // enpassant move
            {
                if (GenericPiece.IsP1Piece(pawn))
                    RemoveCapturedPiece(newPos+8);
                else
                    RemoveCapturedPiece(newPos-8);
            }
            Board[newPos] = piece;

        }
        else
        {
            if (Board[newPos] != null)
                RemoveCapturedPiece(newPos);
            Board[newPos] = piece; 
        }

        Board[oldPos] = null;
        return;

        void PromotePiece(){
            Destroy(Board[oldPos].gameObject);
            Vector3 newTransform = BoardPosToPos(newPos);

            Checker newPiece = Instantiate(Board_SO.DukePrefab, newTransform, Quaternion.identity);
            newPiece.InstantiatePieceComponents(IsP1Turn);
            Board[newPos] = newPiece;
        }
        void RemoveCapturedPiece(int index){
            Board[index].transform.position = DEAD_PIECE;
            Destroy(Board[index].gameObject);
            DecrementPlayerCount(index);
            Board[index] = null;
        }
        bool OnPromotionRow(int pos){return (pos < 8 && IsP1Turn) || (pos > 55 && !IsP1Turn);}
        void DisableEnPassantForEachPawn()
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
        void SetPotentialEnPassant(Pawn currentPiece)
        {
            if (Math.Abs(newPos-oldPos) != 16)
                return;
            
            if (!OnLeftEdge() && Board[newPos-1] is Pawn pawnL && !GenericPiece.ArePiecesOnSameTeam(currentPiece, pawnL))
                pawnL.CanEnPassantRight = true;
            else if (!OnRightEdge() && Board[newPos+1] is Pawn pawnR && !GenericPiece.ArePiecesOnSameTeam(currentPiece, pawnR))
                pawnR.CanEnPassantLeft= true;

            bool OnLeftEdge() {return newPos%8 == 0;}
            bool OnRightEdge() {return newPos%8 == 7;}
        }
    }

    public void ClearAllPiecesValidMoves()
    {
        foreach(GenericPiece piece in Board)
            piece?.ClearValidMoves();
    }

    private void DecrementPlayerCount(int index)
    {
        if (GenericPiece.IsP1Piece(Board[index]))
            --P1_PieceCount;
        else
            --P2_PieceCount;
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
            AlexNotation[index] = checker.TeamID;
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
        return new Vector3(x, GenericPiece.Y, z);
    }



    [ServerRpc(RequireOwnership = false)]
    private void ChangeSides_ServerRPC()
    {
        ChangeSides_ClientRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetP1Val_ServerRpc()
    {
        IsP1Turn_Net.Value ^= true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendMove_ServerRPC(ulong senderID, Vector3 currentPos, Vector3 newPos)
    {
        UpdatePos_ClientRpc(senderID, currentPos, newPos);
    }


    [ClientRpc]
    private void ChangeSides_ClientRPC()
    {
        IsP1Turn ^= true;
        SetValidMovesForPieces();
        CheckForGameOver();
    }

    [ClientRpc]
    public void UpdatePos_ClientRpc(ulong callerID, Vector3 currentPos, Vector3 newPos)
    {
        if (callerID == NetworkManager.Singleton.LocalClientId)
            return;

        GameObject[] allPieces = GameObject.FindGameObjectsWithTag("Piece");

        foreach (GameObject piece in allPieces)
        {
            if (piece.transform.position == currentPos)
            {
                UpdateBoard(piece.transform.position, newPos);

                piece.transform.position = newPos;
                piece.GetComponent<GenericPiece>().UpdatePreviousPos(newPos);
            }
        }
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
        print($"P1_Count: {P1_PieceCount}");
        print($"P2_Count: {P2_PieceCount}");
        print($"Player 1 Turn: {IsP1Turn}");
        print($"Player 1 Turn: {IsP1Turn_Net.Value}");
    }
    [Command]
    public void ChangeP1Turn()
    {
        IsP1Turn ^= true;
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
