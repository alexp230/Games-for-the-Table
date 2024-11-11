using System.Collections.Generic;

public class Bishop : GenericPiece
{
    public override void InstantiatePieceComponents(bool forP1)
    {
        this._MeshRenderer.material = forP1 ? Board_SO.Piece_p1Color : Board_SO.Piece_p2Color;
        this.TeamID = forP1 ? Board_SO.P1_BISHOP : Board_SO.P2_BISHOP;
    }
    
    public override List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps = false)
    {
        List<int> validMoves = new List<int>();
        GenericPiece[] board = ChessBoard.Board;
        int[] offsets = new int[] { -7, 9, 7, -9 };

        int boardPos = ChessBoard.PosToBoardPos(RoundVector(currentPiece.transform.position));
        foreach (int offset in offsets)
        {
            int newPos = boardPos + offset;

            while (!OutOfBounds(newPos) && !Overflown(newPos, offset))
            {
                GenericPiece piece = board[newPos];

                print(piece);
                if (!piece)
                    validMoves.Add(newPos);
                else
                {
                    print($"{currentPiece.TeamID} | {piece.TeamID}");
                    print($"{newPos} {ArePiecesOnSameTeam(currentPiece, piece)}");
                    if (!ArePiecesOnSameTeam(currentPiece, piece))
                        validMoves.Add(newPos);
                    break;
                }

                newPos += offset;
            }
        }

        return validMoves;
    }
}
