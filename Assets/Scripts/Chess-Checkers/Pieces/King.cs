using System;
using System.Collections.Generic;
using UnityEngine;

public class King : GenericPiece
{
    bool HasMoved = false;

    void Start()
    {
        if (BoardMaterials.GameType == BoardMaterials.CHECKERS_CHESS_GAME)
        {
            int boardPos = ChessBoard.PosToBoardPos(this.transform.position);
            AddMoveTokens($"{this.TeamID}", $"{boardPos}");
            UpdateMoveList();
        }
    }

    public override List<int> GetValidMoves(GenericPiece currentPiece, bool getOnlyJumps = false)
    {
        List<int> validMoves = new List<int>();
        GenericPiece[] board = ChessBoard.Board;

        int boardPos = ChessBoard.PosToBoardPos(RoundVector(currentPiece.transform.position));
        foreach (int offset in new int[] { -9, -8, -7, 1, 9, 8, 7, -1 })
        {
            int newPos = boardPos + offset;

            if (OutOfBounds(newPos) || Overflown(newPos, offset))
                continue;
            
            GenericPiece piece = board[newPos];
            if (piece && ArePiecesOnSameTeam(currentPiece, piece))
                continue;

            validMoves.Add(newPos);
        }
        if (this.HasMoved)
            return validMoves;

        foreach (int offset in new int[] { -2, 2 })
        {
            int newPos = boardPos + offset;

            if (OutOfBounds(newPos) || Overflown(newPos, offset))
                continue;
            
            int x_offset = (offset>0) ? -1 : 1;
            if (board[newPos+x_offset] || board[newPos]) // piece beside king or two spaces from king
                continue;
            
            x_offset = (x_offset==1) ? -1 : 1;
            int position = newPos + x_offset;
            while (!OutOfBounds(position) && !Overflown(position, x_offset))
            {
                GenericPiece piece = board[position];

                if (!piece)
                    position += x_offset;

                else if (piece is not Rook rook || rook.HasMoved)
                    break;

                else
                {
                    validMoves.Add(newPos);
                    break;
                }
            }
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
        if (currentPos%8 == 0 && (offset == -7 || offset == 9 || offset == 1 || offset == 2)) return true;
        if (currentPos%8 == 7 && (offset == -9 || offset == 7 || offset == -1 || offset == -2)) return true;
        if (currentPos%8 == 6 && (offset == -2)) return true;       
        if (currentPos%8 == 1 && (offset == 2)) return true;       
        return false;
    }

    protected override void PostMoveProcess(Vector3 lastPos, Vector3 nextPos)
    {
        int oldPos = ChessBoard.PosToBoardPos(lastPos);
        int newPos = ChessBoard.PosToBoardPos(nextPos);

        if (Math.Abs(oldPos-newPos) != 2)
        {
            if (ChessBoard.Board[newPos] != null)
            {
                AddMoveTokens($"{this.TeamID}", $"{lastPos}", "x", $"{newPos}", $"{ChessBoard.Board[newPos].TeamID}");
                ChessBoard.RemovePiece(newPos);
            }
        }
        else
            Castle(oldPos, newPos);
        
        if (MoveTokens.Count == 0)
            AddMoveTokens($"{this.TeamID}", $"{lastPos}", $"{newPos}");
        UpdateMoveList();

        UpdatePosition(this, nextPos);
        
        this.HasMoved = true;
        ChessBoard_S.ChangeSides(this);
    }

    private void Castle(int oldPos, int newPos)
    {
        int offset = (newPos-oldPos > 0) ? 1 : -1;
        int pos = newPos + offset;
        while (ChessBoard.Board[pos] == null)
            pos += offset;

        Rook rook = ChessBoard.Board[pos].GetComponent<Rook>();
        rook.HasMoved = true;

        AddMoveTokens($"{this.TeamID}", $"{oldPos}", "c", $"{newPos}", $"{rook.TeamID}", $"{pos}");
        
        UpdatePosition(rook, ChessBoard.BoardPosToPos(oldPos+offset));
    }
}
