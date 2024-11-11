using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public abstract class GenericPiece : MonoBehaviour
{
    [SerializeField] protected BoardMaterials Board_SO;
    [SerializeField] protected MeshRenderer _MeshRenderer;
    private static ChessBoard ChessBoard_S;
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
            case Duke: this.TeamID = forP1 ? Board_SO.P1_DUKE : Board_SO.P2_DUKE; break;
            case Checker: this.TeamID = forP1 ? Board_SO.P1_PIECE : Board_SO.P2_PIECE; break;
            case Pawn: this.TeamID = forP1 ? Board_SO.P1_PAWN : Board_SO.P2_PAWN; break;
            case Knight: this.TeamID = forP1 ? Board_SO.P1_KNIGHT : Board_SO.P2_KNIGHT; break;
            case Bishop: this.TeamID = forP1 ? Board_SO.P1_BISHOP : Board_SO.P2_BISHOP; break;
            case Rook: this.TeamID = forP1 ? Board_SO.P1_ROOK : Board_SO.P2_ROOK; break;
            case Queen: this.TeamID = forP1 ? Board_SO.P1_QUEEN : Board_SO.P2_QUEEN; break;
            case King: this.TeamID = forP1 ? Board_SO.P1_KING : Board_SO.P2_KING; break;
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

        if (!ChessBoard.IsLocalGame && ((!p1Turn && playerID == 0) || (p1Turn && playerID == 1)))
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
        Tile.HighlightTiles(this.ValidMoves);
    }

    private void OnMouseDrag()
    {
        if (!CanMove())
            return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPos = new Vector3(mousePos.x, mousePos.y, CameraDistanceZ);
        Vector3 newWorldPos = MainCamera.ScreenToWorldPoint(screenPos);
        
        this.transform.position = newWorldPos;
    }

    private void OnMouseUp()
    {
        if (!CanMove())
            return;
            
        Vector3 currentPos = RoundVector(this.transform.position);
        this.transform.position = this.PreviousPosition;

        foreach (int validMove in ValidMoves)
        {
            Vector3 validPos = ChessBoard.BoardPosToPos(validMove);
            if (currentPos == validPos)
            {
                ChessBoard_S.SendMoveToServer(this.PreviousPosition, validPos);
                ProcessTurnLocally(this, validPos);
                break;
            }
        }
        
        Tile.DeHighLightTiles();
    }

    private void ProcessTurnLocally(GenericPiece currentPiece, Vector3 newPos)
    {
        ChessBoard_S.UpdateBoard(currentPiece.PreviousPosition, newPos);
        UpdatePosition(currentPiece, newPos);
        DoSecondJumpOrChangeSides(currentPiece, newPos);

        currentPiece.PreviousPosition = newPos;
    }
    private void UpdatePosition(GenericPiece currentPiece, Vector3 validPos)
    {
        currentPiece.transform.position = validPos;
    }
    private void DoSecondJumpOrChangeSides(GenericPiece currentPiece, Vector3 validPos)
    {
        if (currentPiece is not Checker)
        {
            ChessBoard_S.ChangeSides();
            return;
        }

        List<int> newValidMoves = GetValidMoves(this, getOnlyJumps: true); // Check for second jump
        if (newValidMoves.Count > 0 && Math.Abs(currentPiece.PreviousPosition.x - validPos.x) == 2) // if piece has move and made a jump
        {
            ChessBoard_S.ClearAllPiecesValidMoves();
            currentPiece.ValidMoves = newValidMoves;
        }
        else
            ChessBoard_S.ChangeSides();
    }


    public abstract List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps = false);


    public void UpdatePreviousPos(Vector3 position)
    {
        this.PreviousPosition = position;
    }
    public void SetValidMoves()
    {
        this.ValidMoves = GetValidMoves(this, getOnlyJumps: false);
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

    protected abstract bool Overflown(int currentPos, int offset);

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
