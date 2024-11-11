using System.Collections.Generic;

public class Queen : GenericPiece
{
    public override List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps = false)
    {
        List<int> validMoves = new List<int>();
        GenericPiece[] board = ChessBoard.Board;
        int[] offsets = new int[] { -7, 9, 7, -9, -8, 1, 8, -1 };

        int boardPos = ChessBoard.PosToBoardPos(RoundVector(currentPiece.transform.position));
        foreach (int offset in offsets)
        {
            int newPos = boardPos + offset;

            while (!OutOfBounds(newPos) && !Overflown(newPos, offset))
            {
                GenericPiece piece = board[newPos];

                if (!piece)
                    validMoves.Add(newPos);
                else
                {
                    if (!ArePiecesOnSameTeam(currentPiece, piece))
                        validMoves.Add(newPos);
                    break;
                }

                newPos += offset;
            }
        }

        return validMoves;
    }

    protected override bool Overflown(int currentPos, int offset)
    {
        if (currentPos%8 == 0 && (offset == -7 || offset == 9 || offset == 1)) return true;
        if (currentPos%8 == 7 && (offset == -9 || offset == 7 || offset == -1)) return true;
        
        return false;
    }
}