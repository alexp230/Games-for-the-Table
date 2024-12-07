using System.Collections.Generic;
using UnityEngine;

public class Checker : GenericPiece
{
    private bool CanElevate(Checker piece)
    {
        if (BoardMaterials.GameType != BoardMaterials.CHECKERS_CHESS_GAME)
            return false;
        if (ChessBoard_S.TurnCount == ChessBoard.KING_SPAWN)
            return false;
        
        return BoardMaterials.IsP1Turn == IsP1Piece(piece);
    }

    void OnMouseOver()
    {
        if (!CanElevate(this))
            return;

        if (Input.GetKeyDown(KeyCode.Q)) ElevatePiece(Board_SO.QueenPrefab);
        else if (Input.GetKeyDown(KeyCode.R)) ElevatePiece(Board_SO.RookPrefab);
        else if (Input.GetKeyDown(KeyCode.B)) ElevatePiece(Board_SO.BishopPrefab);
        else if (Input.GetKeyDown(KeyCode.K)) ElevatePiece(Board_SO.KnightPrefab);
        else if (Input.GetKeyDown(KeyCode.N)) ElevatePiece(Board_SO.KnightPrefab);
        else if (Input.GetKeyDown(KeyCode.P)) ElevatePiece(Board_SO.PawnPrefab);
    }

    public override List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps)
    {
        GenericPiece[] board = ChessBoard.Board;
        int[] offsets = (this is Duke) ? new int[]{-9,-7,7,9} : IsP1Piece(currentPiece) ? new int[]{-9, -7} : new int[]{7, 9};
        int boardPos = ChessBoard.PosToBoardPos(RoundVector(currentPiece.transform.position));

        List<int> allMoves = new List<int>();
        foreach (int offset in offsets)
        {
            int newPos = boardPos+offset;
            if (OutOfBounds(newPos) || Overflown(newPos, offset))
                continue;

            GenericPiece newPiece = board[newPos];
            if (!newPiece)
            {
                if (!getOnlyJumps)
                    allMoves.Add(newPos);
                continue;
            }
            
            if (ArePiecesOnSameTeam(newPiece, currentPiece))
                continue;
            
            newPos += offset;
            if (OutOfBounds(newPos) || Overflown(newPos, offset))
                continue;

            if (board[newPos] == null)
                allMoves.Add(newPos);
        }
        return allMoves;
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

        if (OnPromotionRow(newPos) && this is not Duke)
            PromotePiece(lastPos, nextPos);
        else
            UpdatePosition(this, nextPos);

        if (Mathf.Abs(newPos - oldPos) > 9) // if jumped piece
        {
            ChessBoard.RemovePiece((newPos+oldPos)/2);

            List<int> newValidMoves = GetValidMoves(this, getOnlyJumps: true); // Checks for jumping moves
            if (newValidMoves.Count > 0) // if piece has jumpMove and made a jump
            {
                ChessBoard_S.UpdateBoard();
                ChessBoard_S.ClearAllPiecesValidMoves();
                this.ValidMoves = newValidMoves;
            }
            else
                ChessBoard_S.ChangeSides(this);
        }
        else
            ChessBoard_S.ChangeSides(this);

        bool OnPromotionRow(int pos){return (pos < 8 && BoardMaterials.IsP1Turn) || (pos > 55 && !BoardMaterials.IsP1Turn);}
    }

    private void PromotePiece(Vector3 oldPos, Vector3 newPos)
    {
        ChessBoard.RemovePiece(ChessBoard.PosToBoardPos(oldPos));
        ChessBoard.CreatePiece(Board_SO.DukePrefab, newPos);
    }

    private void ElevatePiece(GenericPiece prefab)
    {
        Vector3 pos = this.transform.position;
        ChessBoard.RemovePiece(ChessBoard.PosToBoardPos(pos));
        ChessBoard.CreatePiece(prefab, pos);

        ChessBoard_S.ChangeSides(this);
    }

}
