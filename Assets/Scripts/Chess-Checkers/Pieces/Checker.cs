using System.Collections.Generic;
using UnityEngine;

public class Checker : GenericPiece
{
    private bool CanElevate(Checker piece)
    {
        if (BoardMaterials.GameType != BoardMaterials.CHECKERS_CHESS_GAME)
            return false;
        if (piece is Duke)
            return false;
        if (ChessBoard_S.TurnCount == ChessBoard.KING_SPAWN)
            return false;
        if (ForceJumpPresent())
            return false;

        if (BoardMaterials.IsP1Turn && IsP1Piece(piece))
            return true;
        if (!BoardMaterials.IsP1Turn && IsP2Piece(piece))
            return true;
        
        return false;
    }

    void OnMouseOver()
    {
        if (!CanElevate(this))
            return;

        int playerID = BoardMaterials.IsP1Turn ? 0 : 1;

        if (Input.GetKeyDown(KeyCode.Q) && (PlayerData.QueenTokens[playerID] != 0)) 
            ElevatePiece('q', Board_SO.QueenPrefab, ref PlayerData.QueenTokens[playerID]);
        else if (Input.GetKeyDown(KeyCode.R) && (PlayerData.RookTokens[playerID] != 0)) 
            ElevatePiece('r', Board_SO.RookPrefab, ref PlayerData.RookTokens[playerID]);
        else if (Input.GetKeyDown(KeyCode.B) && (PlayerData.BishopTokens[playerID] != 0)) 
            ElevatePiece('b', Board_SO.BishopPrefab, ref PlayerData.BishopTokens[playerID]);
        else if (Input.GetKeyDown(KeyCode.K) && (PlayerData.KnightTokens[playerID] != 0)) 
            ElevatePiece('h', Board_SO.KnightPrefab, ref PlayerData.KnightTokens[playerID]);
        else if (Input.GetKeyDown(KeyCode.N) && (PlayerData.KnightTokens[playerID] != 0)) 
            ElevatePiece('h', Board_SO.KnightPrefab, ref PlayerData.KnightTokens[playerID]);
        else if (Input.GetKeyDown(KeyCode.P) && (PlayerData.PawnTokens[playerID] != 0)) 
            ElevatePiece('p', Board_SO.PawnPrefab, ref PlayerData.PawnTokens[playerID]);
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
            int jumpedPiecePos = (newPos+oldPos)/2;

            AddMoveTokens($"{this.TeamID}", $"{oldPos}", "x", $"{newPos}");
            ChessBoard.RemovePiece(jumpedPiecePos);

            List<int> newValidMoves = GetValidMoves(this, getOnlyJumps: true); // Checks for jumping moves
            if (newValidMoves.Count > 0) // if piece has jumpMove and made a jump
            {
                ChessBoard_S.UpdateBoard();
                ChessBoard_S.ClearAllPiecesValidMoves();
                this.ValidMoves = newValidMoves;

                DehighlightPieces(Board_SO.Piece_p1Color, Board_SO.Piece_p2Color);
                HighlightPiece();
            }
            else
            {
                UpdateMoveList();
                ChessBoard_S.ChangeSides(this);
            }
        }
        else
        {
            AddMoveTokens($"{this.TeamID}", $"{oldPos}", $"{newPos}");
            UpdateMoveList();
            ChessBoard_S.ChangeSides(this);
        }

        bool OnPromotionRow(int pos){return (pos < 8 && BoardMaterials.IsP1Turn) || (pos > 55 && !BoardMaterials.IsP1Turn);}
    }

    private void PromotePiece(Vector3 oldPos, Vector3 newPos)
    {
        ChessBoard.RemovePiece(ChessBoard.PosToBoardPos(oldPos));
        ChessBoard.CreatePiece(Board_SO.DukePrefab, newPos);
    }

    private void ElevatePiece(char c, GenericPiece prefab, ref int chessPieceToken)
    {
        Vector3 pos = this.transform.position;
        int boardPos = ChessBoard.PosToBoardPos(pos);

        ChessBoard_S.SendMoveToServer(new Vector3[1]{pos}, c);

        ChessBoard.RemovePiece(boardPos);
        GenericPiece newPiece = ChessBoard.CreatePiece(prefab, pos);

        AddMoveTokens($"{this.TeamID}", $"{boardPos}", $"{newPiece.TeamID}");
        UpdateMoveList();

        --chessPieceToken;

        ChessBoard_S.ChangeSides(this);
    }

}
