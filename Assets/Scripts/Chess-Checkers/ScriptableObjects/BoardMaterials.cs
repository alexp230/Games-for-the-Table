using UnityEngine;

[CreateAssetMenu(fileName = "BoardMaterials", menuName = "Scriptable Objects/BoardMaterials")]
public class BoardMaterials : ScriptableObject
{
    public GameObject BoardEdgePrefab;
    public Material Tile_LightColor;
    public Material Tile_DarkColor;
    public Material Tile_HighLight;

    public Material Piece_p1Color;
    public Material Piece_p2Color;
    public Material SpecialPieceColor;

    public AudioClip ChessSFX_1;
    public AudioClip ChessSFX_2;
    public AudioClip ChessSFX_3;

    public Tile TitlePrefab;

    public Checker CheckerPrefab;
    public Duke DukePrefab;

    public Pawn PawnPrefab;
    public Knight KnightPrefab;
    public Bishop BishopPrefab;
    public Rook RookPrefab;
    public Queen QueenPrefab;
    public King KingPrefab;

    public GenericPiece GetPrefab(char c)
    {
        switch(c)
        {
            case 'c': return CheckerPrefab;
            case 'd': return DukePrefab;
            case 'p': return PawnPrefab;
            case 'b': return BishopPrefab;
            case 'h': return KnightPrefab;
            case 'r': return RookPrefab;
            case 'q': return QueenPrefab;
            case 'k': return KingPrefab;
            default: return null;
        }
    }

    public const string ChessSetup = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public const string CheckersSetup = "1c1c1c1c/c1c1c1c1/1c1c1c1c/8/8/C1C1C1C1/1C1C1C1C/C1C1C1C1";

    public const string Apical = "8/1CCCCCC1/2CCCC2/3CC3";
    public const string Canal = "8/CC4CC/CC4CC/CC4CC";
    public const string Default = "8/C1C1C1C1/1C1C1C1C/C1C1C1C1";
    public const string Offset = "8/1C1C1C1C/C1C1C1C1/1C1C1C1C";
    public const string Pyramid = "8/3CC3/2CCCC2/1CCCCCC1";
    public const string Wedges = "8/1C1CC1C1/C1C2C1C/C1C2C1C";
    public const string Xs = "8/C1C2C1C/1C1CC1C1/C1C2C1C";
    public static string P1_Formation = "";
    public static string P2_Formation = "";

    public const char P1_PIECE = 'C';
    public const char P2_PIECE = 'c';
    public const char P1_DUKE = 'D';
    public const char P2_DUKE = 'd';

    public const char P1_PAWN = 'P';
    public const char P2_PAWN = 'p';
    public const char P1_BISHOP = 'B';
    public const char P2_BISHOP = 'b';
    public const char P1_KNIGHT = 'H';
    public const char P2_KNIGHT = 'h';
    public const char P1_ROOK = 'R';
    public const char P2_ROOK = 'r';
    public const char P1_QUEEN = 'Q';
    public const char P2_QUEEN = 'q';
    public const char P1_KING = 'K';
    public const char P2_KING = 'k';

    public const int CHECKERS_GAME = 0;
    public const int CHESS_GAME = 1;
    public const int CHECKERS_CHESS_GAME = 2;
    public static int GameType = 2;

    public static bool IsPaused = true;
    public static bool IsLocalGame = true;
    public static bool ShowValidMoves = true;
    public static bool ForceJump = true;
    public static bool RotateBoardOnMove = false;
    public static bool IsP1Turn = true;

    public AudioClip GetMoveSoundEffect()
    {
        int AllSFXs = 4;
        int randomIndex = Random.Range(1, AllSFXs+1);
        switch (randomIndex)
        {
            case 1: return ChessSFX_1;
            case 2: return ChessSFX_2;
            case 3: return ChessSFX_3;
            default: return ChessSFX_1; 
        }
    }
}
