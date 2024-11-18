using UnityEngine;

[CreateAssetMenu(fileName = "BoardMaterials", menuName = "Scriptable Objects/BoardMaterials")]
public class BoardMaterials : ScriptableObject
{
    public GameObject BoardEdgePrefab;
    public Material Tile_LightColor;
    public Material Tile_DarkColor;

    public Material Piece_p1Color;
    public Material Piece_p2Color;
    public Material SpecialPieceColor;

    public Tile TitlePrefab;

    public Checker CheckerPrefab;
    public Duke DukePrefab;

    public Pawn PawnPrefab;
    public Knight KnightPrefab;
    public Bishop BishopPrefab;
    public Rook RookPrefab;
    public Queen QueenPrefab;
    public King KingPrefab;

    public const string ChessSetup = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public const string CheckersSetup = "1c1c1c1c/c1c1c1c1/1c1c1c1c/8/8/C1C1C1C1/1C1C1C1C/C1C1C1C1";

    public char P1_PIECE = 'C';
    public char P2_PIECE = 'c';
    public char P1_DUKE = 'D';
    public char P2_DUKE = 'd';

    public char P1_PAWN = 'P';
    public char P2_PAWN = 'p';
    public char P1_BISHOP = 'B';
    public char P2_BISHOP = 'b';
    public char P1_KNIGHT = 'H';
    public char P2_KNIGHT = 'h';
    public char P1_ROOK = 'R';
    public char P2_ROOK = 'r';
    public char P1_QUEEN = 'Q';
    public char P2_QUEEN = 'q';
    public char P1_KING = 'K';
    public char P2_KING = 'k';

    public const int CHECKERS_GAME = 0;
    public const int CHESS_GAME = 1;
    public const int COMBINATION_GAME = 2;
    public static int GameType = 0;

    public static bool IsLocalGame = true;
    
}
