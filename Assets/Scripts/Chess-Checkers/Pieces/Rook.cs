using System.Collections.Generic;
using UnityEngine;

public class Rook : GenericPiece
{
    public bool HasMoved = false;

    public override List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps = false)
    {
        List<int> validMoves = new List<int>();
        GenericPiece[] board = ChessBoard.Board;
        int[] offsets = new int[] { -8, 1, 8, -1 };

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

    public override bool Overflown(int currentPos, int offset)
    {
        if (currentPos%8 == 0 && offset == 1) return true;
        if (currentPos%8 == 7 && offset == -1) return true;
        
        return false;
    }

    protected override void PostMoveProcess(Vector3 lastPos, Vector3 nextPos)
    {
        int oldPos = ChessBoard.PosToBoardPos(lastPos);
        int newPos = ChessBoard.PosToBoardPos(nextPos);
        if (ChessBoard.Board[newPos] != null)
        {
            AddMoveTokens($"{this.TeamID}", $"{oldPos}", "x", $"{newPos}");
            ChessBoard.RemovePiece(newPos);
        }
        
        if (MoveTokens.Count == 0)
            AddMoveTokens($"{this.TeamID}", $"{oldPos}", $"{newPos}");
        UpdateMoveList();
        
        UpdatePosition(this, nextPos);
        // SetAICastleRights(oldPos);
        this.HasMoved = true;
        ChessBoard_S.ChangeSides(this);
    }

    // private void SetAICastleRights(int pos)
    // {
    //     if (IsP1Piece(this))
    //         StockfishAI_S.CanCastle1[(pos==56) ? 2 : 1] = false;
    //     else
    //         StockfishAI_S.CanCastle2[(pos==0) ? 2 : 1] = false;
    // }
}