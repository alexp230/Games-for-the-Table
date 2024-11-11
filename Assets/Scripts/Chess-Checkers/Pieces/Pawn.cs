using System.Collections.Generic;

public class Pawn : GenericPiece
{
    private bool MadeFirstMove = false;

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

        return validMoves;
    }

    protected override bool Overflown(int currentPos, int offset)
    {
        if (currentPos%8 == 0 && (offset == -7 || offset == 9)) return true;
        if (currentPos%8 == 7 && (offset == -9 || offset == 7)) return true;
        
        return false;
    }
}