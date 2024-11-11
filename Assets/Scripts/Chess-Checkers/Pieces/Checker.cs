using System.Collections.Generic;

public class Checker : GenericPiece
{

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

    protected override bool Overflown(int currentPos, int offset)
    {
        // 00 01 02 03 04 05 06 07
        // 08 09 10 11 12 13 14 15
        // 16 17 18 19 20 21 22 23
        // 24 25 26 27 28 29 30 31
        // 32 33 34 35 36 37 38 39
        // 40 41 42 43 44 45 46 47
        // 48 49 50 51 52 53 54 55
        // 56 57 58 59 60 61 62 63

        // i.e Given the tile 7, when trying to access its right tile, it will get 16 which is not a valid tile for
        // the piece on 7 to go to

        // if (currentPos%8 == 0 && (offset == -7 || offset == 9))
        //     return true;
        // if (currentPos%8 == 7 && (offset == -9 || offset == 7))
        //     return true;
        // return false;

        int[] rightOverflow = new int[8]{ 0,8,16,24,32,40,48,56 };
        int[] leftOverflow = new int[8]{ 7,15,23,31,39,47,55,63 };

        foreach (int val in rightOverflow)
            if (val == currentPos && (offset == -7 || offset == 9 ))
                return true;
        foreach (int val in leftOverflow)
            if (val == currentPos && (offset == -9 || offset == 7))
                return true;
        return false;
    }

}
