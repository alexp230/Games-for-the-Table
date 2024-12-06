using System;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : GenericPiece
{
    private bool MadeFirstMove = false;
    public bool CanEnPassantLeft = false;
    public bool CanEnPassantRight = false;

    void OnMouseOver()
    {
        if (OnPromotionRow())
        {
            if (Input.GetKeyDown(KeyCode.Q)) PromotePiece(Board_SO.QueenPrefab);
            else if (Input.GetKeyDown(KeyCode.R)) PromotePiece(Board_SO.RookPrefab);
            else if (Input.GetKeyDown(KeyCode.B)) PromotePiece(Board_SO.BishopPrefab);
            else if (Input.GetKeyDown(KeyCode.K)) PromotePiece(Board_SO.KnightPrefab);
            else if (Input.GetKeyDown(KeyCode.N)) PromotePiece(Board_SO.KnightPrefab);
            else if (Input.GetKeyDown(KeyCode.D)) PromotePiece(Board_SO.DukePrefab);
        }    
    }

    public override List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps = false)
    {
        List<int> validMoves = new List<int>();

        GenericPiece[] board = ChessBoard.Board;
        int yOffset = IsP1Piece(currentPiece) ? -1 : 1;

        int boardPos = ChessBoard.PosToBoardPos(RoundVector(currentPiece.transform.position));

        int newPos = boardPos + 8 * yOffset; // One space ahead
        if (!OutOfBounds(newPos) && !board[newPos])
        { 
            validMoves.Add(newPos);

            if (!MadeFirstMove)
            {
                newPos = boardPos + (16*yOffset); // Two spaces ahead
                if (!OutOfBounds(newPos) && !board[newPos]) 
                    validMoves.Add(newPos);
            }
        }

        foreach (int offset in new int[]{ 7*yOffset, 9*yOffset }) // Captures left and right
        {
            newPos = boardPos + offset;
            if (!OutOfBounds(newPos) && !Overflown(newPos, offset) && board[newPos] && !ArePiecesOnSameTeam(currentPiece, board[newPos]))
                validMoves.Add(newPos);
        }

        if (this.CanEnPassantRight)
            validMoves.Add( IsP1Piece(this) ? boardPos-7 : boardPos+9 );
        else if (this.CanEnPassantLeft)
            validMoves.Add( IsP1Piece(this) ? boardPos-9 : boardPos+7 );

        return validMoves;
    }

    public override bool Overflown(int currentPos, int offset)
    {
        if (currentPos%8 == 0 && (offset == -7 || offset == 9)) return true;
        if (currentPos%8 == 7 && (offset == -9 || offset == 7)) return true;
        return false;
    }

    protected override void PostMoveProcess(Vector3 lastPos, Vector3 nextPos)
    {
        int oldPos = ChessBoard.PosToBoardPos(lastPos);
        int newPos = ChessBoard.PosToBoardPos(nextPos);

        SetPotentialEnPassant(this);

        if (ChessBoard.Board[newPos] != null)
            ChessBoard.RemovePiece(newPos);
        else if (Mathf.Abs(newPos-oldPos) == 9 || (Mathf.Abs(newPos-oldPos) == 7)) // enpassant move
        {
            if (IsP1Piece(this))
                ChessBoard.RemovePiece(newPos+8);
            else
                ChessBoard.RemovePiece(newPos-8);
            
            if (ChessBoard.DidThisPlayerMove())
                SteamAchievements.UnlockAchievement("NEW_ACHIEVEMENT_1_8");
        }

        if (OnPromotionRow(newPos))
        {
            this._MeshRenderer.material = Board_SO.SpecialPieceColor;
            UpdatePosition(this, nextPos);
            this.MadeFirstMove = true;

            ChessBoard_S.UpdateBoard();
            ChessBoard_S.ClearAllPiecesValidMoves();
            return;
        }

        UpdatePosition(this, nextPos);
        this.MadeFirstMove = true;
        ChessBoard_S.ChangeSides(this);

        void SetPotentialEnPassant(Pawn currentPiece)
        {
            if (Math.Abs(newPos-oldPos) != 16)
                return;
            
            if (!OnLeftEdge() && ChessBoard.Board[newPos-1] is Pawn pawnL && !ArePiecesOnSameTeam(currentPiece, pawnL))
                pawnL.CanEnPassantRight = true;
            else if (!OnRightEdge() && ChessBoard.Board[newPos+1] is Pawn pawnR && !ArePiecesOnSameTeam(currentPiece, pawnR))
                pawnR.CanEnPassantLeft= true;

            bool OnLeftEdge() {return newPos%8 == 0;}
            bool OnRightEdge() {return newPos%8 == 7;}
        }
    }

    private void PromotePiece(GenericPiece prefab)
    {
        Vector3 currentPos = this.transform.position;
        ChessBoard.RemovePiece(ChessBoard.PosToBoardPos(currentPos));
        ChessBoard.CreatePiece(prefab, currentPos);
        ChessBoard_S.ChangeSides(this);
    }

    private bool OnPromotionRow(int pos){return (pos < 8 && BoardMaterials.IsP1Turn) || (pos > 55 && !BoardMaterials.IsP1Turn);}
    private bool OnPromotionRow(){return (this.PreviousPosition.z == 7 && BoardMaterials.IsP1Turn) || (this.PreviousPosition.z == 0 && !BoardMaterials.IsP1Turn);}

}