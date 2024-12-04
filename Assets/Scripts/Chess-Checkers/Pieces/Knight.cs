using System.Collections.Generic;
using UnityEngine;

public class Knight : GenericPiece
{
    public override List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps = false)
    {
        List<int> validMoves = new List<int>();
        GenericPiece[] board = ChessBoard.Board;
        int[] offsets = new int[] { -15, -6, 10, 17, 15, 6, -10, -17 };

        int boardPos = ChessBoard.PosToBoardPos(RoundVector(currentPiece.transform.position));
        foreach (int offset in offsets)
        {
            int newPos = boardPos + offset;

            if (OutOfBounds(newPos) || Overflown(newPos, offset))
                continue;
            
            GenericPiece piece = board[newPos];
            if (piece && ArePiecesOnSameTeam(currentPiece, piece))
                continue;

            validMoves.Add(newPos);
        }

        return validMoves;
    }

    public override bool Overflown(int currentPos, int offset)
    {
        if (currentPos%8 == 0 && (offset == -15 || offset == -6 || offset == 10 || offset == 17)) return true;
        if (currentPos%8 == 1 && (offset == -6 || offset == 10)) return true;
        if (currentPos%8 == 7 && (offset == -17 || offset == -10 || offset == 6 || offset == 15)) return true;
        if (currentPos%8 == 6 && (offset == -10 || offset == 6)) return true;
        return false;
    }

    protected override void PostMoveProcess(Vector3 lastPos, Vector3 nextPos)
    {
        int newPos = ChessBoard.PosToBoardPos(nextPos);
        if (ChessBoard.Board[newPos] != null)
            ChessBoard_S.RemovePiece(newPos);
        
        UpdatePosition(this, nextPos);
        ChessBoard_S.ChangeSides(this);
    }
}