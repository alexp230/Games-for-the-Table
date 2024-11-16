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

    public bool CheckIfKingIsInCheck(GenericPiece[] board, int kingPos)
    {
        foreach (int offset in IsP1Piece(this) ? new int[] { -9, -7 } : new int[] { 9, 7 })
        {
            int newPos = kingPos + offset;
            if (OutOfBounds(newPos))
                continue;

            GenericPiece piece = board[newPos];
            if (piece && piece is Pawn pawn && !pawn.Overflown(kingPos, -offset) && !ArePiecesOnSameTeam(this, pawn))
                return true;
        }

        foreach (int offset in new int[] { -15, -6, 10, 17, 15, 6, -10, -17 })
        {
            int newPos = kingPos + offset;
            if (OutOfBounds(newPos))
                continue;

            GenericPiece piece = board[newPos];
            if (piece && piece is Knight knight && !knight.Overflown(kingPos, -offset) && !ArePiecesOnSameTeam(this, knight))
                return true;
        }

        bool diagonal = true;
        foreach (int offset in new int[] { -9, -8, -7, 1, 9, 8, 7, -1 })
        {
            int newPos = kingPos + offset;
            while (!OutOfBounds(newPos) && !Overflown(newPos, offset))
            {
                GenericPiece piece = board[newPos];
                if (!piece)
                    newPos += offset;
                
                else if (ArePiecesOnSameTeam(this, piece))
                    break;

                else if (piece is Queen || (piece is Bishop && diagonal) || piece is Rook && !diagonal)
                    return true;

                else
                    break;                    
            }
            diagonal ^= true;
        }
        return false;
    }

    public override bool Overflown(int currentPos, int offset)
    {
        if (currentPos%8 == 0 && (offset == -7 || offset == 9 || offset == 1)) return true;
        if (currentPos%8 == 7 && (offset == -9 || offset == 7 || offset == -1)) return true;
        
        return false;
    }

    protected override void PostMoveProcess(GenericPiece currentPiece, Vector3 validPos)
    {
        ChessBoard_S.ChangeSides();
    }
}
