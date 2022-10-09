using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private TileBase floorTile;

    public void PaintFloorTiles(IEnumerable<Vector2Int> positions)
    {
        PaintTiles(positions, floorTilemap, floorTile);
    }

    public bool HasTile(Vector2Int position)
    {
       return floorTilemap.HasTile((Vector3Int)position);
    }

    public void PaintSingleTile(Vector2Int position)
    {
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, floorTile);
    }

    public void ClearTilemap()
    {
        floorTilemap.ClearAllTiles();
    }

    public void EliminateSingleWalls()
    {
        foreach (var position in floorTilemap.cellBounds.allPositionsWithin)
        {
            Vector2Int pos = (Vector2Int)position;

            bool hasTile = floorTilemap.HasTile((Vector3Int)pos);
            bool upperTile = floorTilemap.HasTile((Vector3Int)(pos + new Vector2Int(0, -1)));
            bool bottomTile = floorTilemap.HasTile((Vector3Int)(pos + new Vector2Int(0, 1)));
            bool rightTile = floorTilemap.HasTile((Vector3Int)(pos + new Vector2Int(1, 0)));
            bool leftTile = floorTilemap.HasTile((Vector3Int)(pos + new Vector2Int(-1, 0)));


            if (!hasTile && upperTile && bottomTile && rightTile && leftTile)
            {
                Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
                floorTilemap.SetTile(tileMapPosition, floorTile);
            }
        }
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (Vector2Int pos in positions)
        {
            PaintSingleTile(pos, tilemap, tile);
        }
    }

    private void PaintSingleTile(Vector2Int position, Tilemap tilemap, TileBase tile)
    {
        Vector3Int tileMapPosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tileMapPosition, tile);
    }

}
