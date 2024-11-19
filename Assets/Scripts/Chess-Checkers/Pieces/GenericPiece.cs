using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public abstract class GenericPiece : MonoBehaviour
{
    [SerializeField] protected BoardMaterials Board_SO;
    [SerializeField] protected MeshRenderer _MeshRenderer;
    protected static ChessBoard ChessBoard_S;
    public const float Y = 0.15f;

    private static Camera MainCamera;
    private static float CameraDistanceZ;

    [SerializeField] public Vector3 PreviousPosition;
    [SerializeField] public char TeamID;
    [SerializeField] public List<int> ValidMoves = new List<int>();

    private void Awake()
    {
        this.PreviousPosition = this.transform.position;

        int index = ChessBoard.PosToBoardPos(this.PreviousPosition);
        ChessBoard.Board[index] = this;
    }

    public void InstantiatePieceComponents(bool forP1)
    {
        switch(this)
        {
            case Duke: this.TeamID = forP1 ? BoardMaterials.P1_DUKE : BoardMaterials.P2_DUKE; break;
            case Checker: this.TeamID = forP1 ? BoardMaterials.P1_PIECE : BoardMaterials.P2_PIECE; break;
            case Pawn: this.TeamID = forP1 ? BoardMaterials.P1_PAWN : BoardMaterials.P2_PAWN; break;
            case Knight: this.TeamID = forP1 ? BoardMaterials.P1_KNIGHT : BoardMaterials.P2_KNIGHT; break;
            case Bishop: this.TeamID = forP1 ? BoardMaterials.P1_BISHOP : BoardMaterials.P2_BISHOP; break;
            case Rook: this.TeamID = forP1 ? BoardMaterials.P1_ROOK : BoardMaterials.P2_ROOK; break;
            case Queen: this.TeamID = forP1 ? BoardMaterials.P1_QUEEN : BoardMaterials.P2_QUEEN; break;
            case King: this.TeamID = forP1 ? BoardMaterials.P1_KING : BoardMaterials.P2_KING; break;
        }

        this._MeshRenderer.material = forP1 ? Board_SO.Piece_p1Color : Board_SO.Piece_p2Color;
    }



    private void Start()
    {
        MainCamera = Camera.main;
        CameraDistanceZ = MainCamera.WorldToScreenPoint(this.transform.position).z;
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
    }


    protected bool CanMove()
    {
        bool p1Turn = ChessBoard.IsP1Turn_Net.Value;
        ulong playerID = NetworkManager.Singleton.LocalClientId;

        if (!BoardMaterials.IsLocalGame && ((!p1Turn && playerID == 0) || (p1Turn && playerID == 1)))
            return false;
        if (this.ValidMoves.Count == 0)
            return false;

        return true;
    }

    private void OnMouseDown()
    {
        if (!CanMove())
            return;
        
        this.PreviousPosition = RoundVector(this.transform.position);
        Tile.HighlightTiles(this);
    }

    private void OnMouseDrag()
    {
        if (!CanMove())
            return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPos = new Vector3(mousePos.x, mousePos.y, CameraDistanceZ);
        Vector3 newWorldPos = MainCamera.ScreenToWorldPoint(screenPos);
        newWorldPos.y = 0.30f;
        
        this.transform.position = newWorldPos;
    }

    private void OnMouseUp()
    {
        if (!CanMove())
            return;
            
        Vector3 currentPos = RoundVector(this.transform.position);
        Vector3 lastPos = this.PreviousPosition;
        this.transform.position = this.PreviousPosition;

        foreach (int validMove in this.ValidMoves)
        {
            Vector3 validPos = ChessBoard.BoardPosToPos(validMove);
            if (currentPos == validPos)
            {
                ChessBoard_S.SendMoveToServer(lastPos, validPos);
                ProcessTurnLocally(lastPos, validPos);
                break;
            }
        }
        
        Tile.DeHighLightTiles();
    }

    protected abstract void PostMoveProcess(Vector3 lastPos, Vector3 nextPos);

    public void ProcessTurnLocally(Vector3 oldPos, Vector3 newPos)
    {
        ChessBoard_S.DisableEnPassantForEachPawn();
        PostMoveProcess(oldPos, newPos);
    }
    protected void UpdatePosition(GenericPiece currentPiece, Vector3 validPos)
    {
        currentPiece.transform.position = validPos;
        currentPiece.PreviousPosition = validPos;
    }

    public abstract List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps = false);

    private bool MoveKeepsKingSafe(int oldPos, int newPos)
    {
        GenericPiece[] newBoard = new GenericPiece[ChessBoard.Board.Length];
        int i = -1;
        foreach (GenericPiece piece in ChessBoard.Board)
            newBoard[++i] = piece;

        newBoard[newPos] = newBoard[oldPos];
        newBoard[oldPos] = null;

        return !KingInCheck(newBoard);

    }

    bool KingInCheck(GenericPiece[] board)
    {
        int i=0;
        foreach (GenericPiece piece in board)
        {
            if (piece is King kingPiece && (IsP1Piece(kingPiece) == ChessBoard.IsP1Turn))
                return kingPiece.CheckIfKingIsInCheck(board, i);
            ++i;
        }
            
        return false;
    }

    public void UpdatePreviousPos(Vector3 position)
    {
        this.PreviousPosition = position;
    }
    public void SetValidMoves()
    {
        List<int> validMoves = new List<int>();

        int currentPos = ChessBoard.PosToBoardPos(this.transform.position);
        List<int> allMoves = GetValidMoves(this, getOnlyJumps: false);
        foreach (int newMove in allMoves)
            if (MoveKeepsKingSafe(currentPos, newMove))
                validMoves.Add(newMove);
        
        this.ValidMoves = validMoves;

    }
    public void SetValidMoves(List<int> moves)
    {
        this.ValidMoves = moves;
    }
    public void ClearValidMoves()
    {
        this.ValidMoves.Clear();
    }


    protected bool OutOfBounds(int currentPos){return currentPos < 0 || currentPos > 63;}      

    // 00 01 02 03 04 05 06 07
    // 08 09 10 11 12 13 14 15
    // 16 17 18 19 20 21 22 23
    // 24 25 26 27 28 29 30 31
    // 32 33 34 35 36 37 38 39
    // 40 41 42 43 44 45 46 47
    // 48 49 50 51 52 53 54 55
    // 56 57 58 59 60 61 62 63

    // i.e Given the tile 7, when trying to access its right tile, it will get 16 which is
    // not a valid tile for a checker on 7 to go to, though being within bounds of board
    public abstract bool Overflown(int currentPos, int offset);


    public static bool IsP1Piece(GenericPiece piece)
    {
        return char.IsUpper(piece.TeamID);
    }
    public static bool IsP2Piece(GenericPiece piece)
    {
        return char.IsLower(piece.TeamID);
    }
    public static bool IsP1Piece(char teamID)
    {
        return char.IsUpper(teamID);
    }
    public static bool IsP2Piece(char teamID)
    {
        return char.IsLower(teamID);
    }
    public static bool ArePiecesOnSameTeam(GenericPiece piece1, GenericPiece piece2)
    {
        return IsP1Piece(piece1) != IsP2Piece(piece2);
    }

    protected static Vector3 RoundVector(Vector3 vector)
    {
        return new Vector3(Mathf.RoundToInt(vector.x), Y, Mathf.RoundToInt(vector.z));
    }

}
