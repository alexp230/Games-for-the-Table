using UnityEngine;

[CreateAssetMenu(fileName = "BoardMaterials", menuName = "Scriptable Objects/BoardMaterials")]
public class BoardMaterials : ScriptableObject
{
    public GameObject BoardEdgePrefab;
    public Material Tile_LightColor;
    public Material Tile_DarkColor;

    public Material Piece_p1Color;
    public Material Piece_p2Color;

    public Tile TitlePrefab;
    public Checker CheckerPrefab;
    public Duke DukePrefab;
    public Bishop BishopPrefab;

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

    
}
