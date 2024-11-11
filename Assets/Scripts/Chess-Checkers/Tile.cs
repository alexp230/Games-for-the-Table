using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private BoardMaterials TileMaterials_SO;
    [SerializeField] private MeshRenderer _MeshRenderer;
    [SerializeField] private GameObject HighLight;

    private static Dictionary<int, Tile> TilePositions = new Dictionary<int, Tile>();
    private static List<int> HighLightedPositions = new List<int>();

    private void Awake()
    {
        Vector3 pos = this.transform.position;
        this._MeshRenderer.material = (pos.x%2 == pos.z%2) ? TileMaterials_SO.Tile_DarkColor : TileMaterials_SO.Tile_LightColor;
        this.name = $"Tile({pos.x},{pos.z})";

        TilePositions.Add(ChessBoard.PosToBoardPos(pos), this);
    }

    public static void HighlightTiles(List<int> positions)
    {
        GenericPiece[] board = ChessBoard.Board;
        foreach (int pos in positions)
        {
            GameObject highlight = TilePositions[pos].HighLight;
            
            highlight.transform.localScale = board[pos] ? new Vector3(1, 0.1f, 1) : new Vector3(0.25f, 0.1f, 0.25f);

            highlight.SetActive(true);
            HighLightedPositions.Add(pos);
        }
    }

    public static void DeHighLightTiles()
    {
        foreach (int pos in HighLightedPositions)
            TilePositions[pos].HighLight.SetActive(false);
        HighLightedPositions.Clear();
    }
}
