using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private ChessBoard ChessBoard_S;
    [SerializeField] private BoardMaterials Board_SO;
    [SerializeField] private MeshRenderer _MeshRenderer;
    [SerializeField] private GameObject HighLight;

    private static Dictionary<int, Tile> TilePositions = new Dictionary<int, Tile>();
    private static List<int> HighLightedPositions = new List<int>();

    void Awake()
    {
        Vector3 pos = this.transform.position;
        this._MeshRenderer.material = GetTileMaterial(pos);
        this.name = $"Tile({pos.x},{pos.z})";

        TilePositions.Add(ChessBoard.PosToBoardPos(pos), this);
    }

    void Start()
    {
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
    }

    void OnDisable()
    {
        TilePositions.Remove(ChessBoard.PosToBoardPos(this.transform.position));
    }

    private bool CanSpawnKing()
    {
        if (BoardMaterials.IsPaused)
            return false;
        if (!BoardMaterials.IsLocalGame && (BoardMaterials.IsP1Turn != (PlayerData.PlayerID == 0)))
            return false;
            
        return this._MeshRenderer.material.color==Board_SO.Tile_HighLight.color; 
    }

    void OnMouseDown()
    {
        if (CanSpawnKing())
        {
            DeHighlightKingRow();
            SpawnKing();
        }
    }
    public static void DeHighlightKingRow()
    {
        int[] kingsRow = BoardMaterials.IsP1Turn ? new int[] {56,57,58,59,60,61,62,63} : new int[] {0,1,2,3,4,5,6,7};
        foreach (int pos in kingsRow)
        {
            Tile tile = TilePositions[pos];
            tile._MeshRenderer.material = tile.GetTileMaterial(tile.transform.position);
        }
    }
    private void SpawnKing()
    {
        Vector3 position = new Vector3(this.transform.position.x, GenericPiece.Y, this.transform.position.z);
        ChessBoard.CreatePiece(Board_SO.KingPrefab, position);
        ChessBoard_S.SendMoveToServer(new Vector3[1] {position}, 'k');
        ChessBoard_S.ChangeSides(null);
    }

    private Material GetTileMaterial(Vector3 position)
    {
        return (position.x%2 == position.z%2) ? Board_SO.Tile_DarkColor : Board_SO.Tile_LightColor;
    }

    public static void HighlightTiles(GenericPiece currentPiece)
    {            
        List<int> positions = currentPiece.ValidMoves;
        GenericPiece[] board = ChessBoard.Board;
        foreach (int pos in positions)
        {
            GameObject highlight = TilePositions[pos].HighLight;
            
            highlight.transform.localScale = CaptureTile(pos) ? new Vector3(1, 0.1f, 1) : new Vector3(0.25f, 0.1f, 0.25f);

            highlight.SetActive(true);
            HighLightedPositions.Add(pos);
        }

        bool CaptureTile(int position)
        {
            if (currentPiece is Pawn pawn)
            {
                if (pawn.CanEnPassantLeft)
                    return GenericPiece.IsP1Piece(pawn) ? board[position-9] : board[position+7];
                else if (pawn.CanEnPassantRight)
                    return GenericPiece.IsP1Piece(pawn) ? board[position-7] : board[position+9];
            }
            return board[position];
        }   
    }

    public static void DeHighLightTiles()
    {
        foreach (int pos in HighLightedPositions)
            TilePositions[pos].HighLight.SetActive(false);
        HighLightedPositions.Clear();
    }

    public static void SetValidKingSpawnTiles(List<int> validTiles, Material highlightMaterial)
    {
        foreach (int pos in validTiles)
            TilePositions[pos]._MeshRenderer.material = highlightMaterial;
    }
}
