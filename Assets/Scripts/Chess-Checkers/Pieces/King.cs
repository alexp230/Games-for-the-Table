using System.Collections.Generic;
using UnityEngine;

public class King : GenericPiece
{
    public override List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps = false)
    {
        List<int> validMoves = new List<int>();
        GenericPiece[] board = ChessBoard.Board;
        int[] offsets = new int[] { -8, -7, 1, 9, 8, 7, -1, -9 };

        int boardPos = ChessBoard.PosToBoardPos(RoundVector(currentPiece.transform.position));
        foreach (int offset in offsets)
        {
            int newPos = boardPos + offset;

            if (OutOfBounds(newPos))
                continue;
            
            GenericPiece piece = board[newPos];
            if (piece && ArePiecesOnSameTeam(currentPiece, piece))
                continue;

            validMoves.Add(newPos);
        }

        return validMoves;
    }

    protected override bool Overflown(int currentPos, int offset)
    {
        throw new System.NotImplementedException();
    }

    protected override void PostMoveProcess(GenericPiece currentPiece, Vector3 validPos)
    {
        ChessBoard_S.ChangeSides();
    }
}
