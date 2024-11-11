using System.Collections.Generic;

public class Checker : GenericPiece
{
    public override void InstantiatePieceComponents(bool forP1)
    {
        this._MeshRenderer.material = forP1 ? Board_SO.Piece_p1Color : Board_SO.Piece_p2Color;
        this.TeamID = forP1 ? Board_SO.P1_PIECE : Board_SO.P2_PIECE;
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

}
