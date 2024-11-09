using UnityEngine;

[CreateAssetMenu(fileName = "BoardMaterials", menuName = "Scriptable Objects/BoardMaterials")]
public class BoardMaterials : ScriptableObject
{
    public Material Tile_LightColor;
    public Material Tile_DarkColor;

    public Material Piece_p1Color;
    public Material Piece_p2Color;

    public Tile TitlePrefab;
    public Checker CheckerPrefab;
    public Duke DukePrefab;
    
}
