using System;
using System.Collections.Generic;
using QFSW.QC;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ChessBoard : NetworkBehaviour
{
    [SerializeField] private BoardMaterials Board_SO;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private GameObject GameInfoNoteBook;

    private static Vector3 DEAD_PIECE = new Vector3(-100f, -100f, -100f);
    
    public static GenericPiece[] Board = new GenericPiece[64];

    public UnityEvent<bool> OnChangedTurn;
    public UnityEvent OnGameOver;
    public UnityEvent<string> OnWinnerAnnounced;

    void Start()
    {
        GenerateBoardTiles();
        
        // StartGame();
    }

    public void StartGame()
    {
        BoardMaterials.IsP1Turn = true;
        OnChangedTurn?.Invoke(BoardMaterials.IsP1Turn);

        switch (BoardMaterials.GameType)
        {
            case BoardMaterials.CHECKERS_GAME: GeneratePieces(BoardMaterials.CheckersSetup); break;
            case BoardMaterials.CHESS_GAME: GeneratePieces(BoardMaterials.ChessSetup); break;
            case BoardMaterials.CHECKERS_CHESS_GAME: GeneratePieces(BoardMaterials.Xs_P2 + '/' + BoardMaterials.Xs_P1); break;
        }

        SetValidMovesForPieces();

        FixCameraAndPiecesRotation();
    }

    // Canvas/OptionsScreen/Options(BackButton) OC
    public void FixCameraAndPiecesRotation()
    {
        SetObjectsTransform(forP1: PlayerData.PlayerID == 0);
    }
    public void SetCameraAndPiecesRotation()
    {
        if (BoardMaterials.IsLocalGame && BoardMaterials.RotateBoardOnMove)
            SetObjectsTransform(forP1: BoardMaterials.IsP1Turn);
    }

    private void SetObjectsTransform(bool forP1)
    {
        Vector3 GameInfoPosition = forP1 ? new Vector3(-6.25f, 1.5f, -4f) : new Vector3(6.25f, 1.5f, 4f);
        Quaternion GameInfoRotation = forP1 ? Quaternion.Euler(0, 340f, 0) : Quaternion.Euler(0, 160f, 0);
        GameInfoNoteBook.transform.SetLocalPositionAndRotation(GameInfoPosition, GameInfoRotation);
        
        Vector3 cameraPosition = forP1 ? new Vector3(3.5f, 10f, 2f) : new Vector3(3.5f, 10f, 5f);
        Quaternion cameraEulerAngle = forP1 ? Quaternion.Euler(85f, 0f, 0f) : Quaternion.Euler(85f, 180f, 0f);
        if (MainCamera != null)
            MainCamera.transform.SetLocalPositionAndRotation(cameraPosition, cameraEulerAngle);

        Vector3 pieceEulerAngle = forP1 ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 180f, 0f);
        foreach (GenericPiece piece in Board)
            if (piece)
                piece.transform.eulerAngles = pieceEulerAngle;
    }

    public void ResetGame()
    {
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

    public void ChangeSides(GenericPiece currentPiece)
    {
        BoardMaterials.IsP1Turn ^= true;
        UpdateBoard();
        SetValidMovesForPieces();
        CheckForGameOver(currentPiece);
        SetCameraAndPiecesRotation();

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

        if (allPiecesJump.Count > 0 && BoardMaterials.ForceJump && BoardMaterials.GameType == BoardMaterials.CHECKERS_GAME)
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

    private void CheckForGameOver(GenericPiece currentPiece)
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
            CallCheckMateAchievement(currentPiece);
            OnGameOver?.Invoke();
            SetWinnerName(PlayerData.PlayerID, PlayerData.PlayerName);
            RemoveAllPieces();
        }

        void CallCheckMateAchievement(GenericPiece currentPiece)
        {
            if (DidThisPlayerMove() || (BoardMaterials.GameType != BoardMaterials.CHESS_GAME))
                return;
                
            switch (currentPiece)
            {
                case Pawn: SteamAchievements.UnlockAchievement("NEW_ACHIEVEMENT_1_1"); break;
                case Bishop: SteamAchievements.UnlockAchievement("NEW_ACHIEVEMENT_1_2"); break;
                case Rook: SteamAchievements.UnlockAchievement("NEW_ACHIEVEMENT_1_3"); break;
                case Knight: SteamAchievements.UnlockAchievement("NEW_ACHIEVEMENT_1_4"); break;
                case Queen: SteamAchievements.UnlockAchievement("NEW_ACHIEVEMENT_1_5"); break;
            }
        }
    }
    private void SetWinnerName(int playerID, string playerName)
    {
        if (BoardMaterials.IsLocalGame)
            OnWinnerAnnounced?.Invoke(BoardMaterials.IsP1Turn == (playerID == 0) ? $"{playerName} Loses!" : $"{playerName} Wins!");
        else
            SetWinnerName_ServerRPC(PlayerData.PlayerID, PlayerData.PlayerName);
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

    public static bool DidThisPlayerMove()
    {
        return BoardMaterials.IsP1Turn == (PlayerData.PlayerID == 0);
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

    [ServerRpc(RequireOwnership = false)]
    public void SetWinnerName_ServerRPC(int callerID, string playerName)
    {
        bool isP1Turn = BoardMaterials.IsP1Turn;
        if ((isP1Turn && callerID == 1) || (!isP1Turn && callerID == 0))
            SetWinnerName_ClientRPC(playerName);
    }

    [ClientRpc]
    public void SetWinnerName_ClientRPC(string playerName)
    {
        print(playerName);
        OnWinnerAnnounced?.Invoke($"{playerName} Wins!");
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
        print("NetworkID: " + NetworkManager.Singleton?.LocalClientId);
        print("IsLocalGame: " + BoardMaterials.IsLocalGame);
        print("PlayerID: " + PlayerData.PlayerID);
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
