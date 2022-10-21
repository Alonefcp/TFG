using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tile floorTile;
    [SerializeField] private Tile corridorTile;

    /// <summary>
    /// Returns the cell's radius. We assume a cell is n*n.
    /// </summary>
    /// <returns></returns>
    public float GetCellRadius()
    {
        return floorTilemap.cellSize.x / 2.0f;
    }

    /// <summary>
    /// Paints tiles whose position is stored is a structure
    /// </summary>
    /// <param name="positions"></param>
    public void PaintFloorTiles(IEnumerable<Vector2Int> positions)
    {
        PaintTiles(positions, floorTilemap, floorTile);
    }

    public void PaintCorridorTiles(IEnumerable<Vector2Int> positions)
    {
        PaintTiles(positions, floorTilemap, corridorTile);
    }

    public void EraseTiles(IEnumerable<Vector2Int> positions)
    {
        PaintTiles(positions, floorTilemap, null);
    }

    /// <summary>
    /// Returns true if th tilemap has a tile in a given position
    /// </summary>
    /// <param name="position">Tile position</param>
    /// <returns></returns>
    public bool HasTile(Vector2Int position)
    {
       return floorTilemap.HasTile((Vector3Int)position);
    }

    /// <summary>
    /// Paints a single tile
    /// </summary>
    /// <param name="position">Tile position</param>
    public void PaintSingleFloorTile(Vector2Int position)
    {
        floorTile.color = Color.white;
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, floorTile);
    }

    public void PaintSingleFloorTileWithColor(Vector2Int position, Color color)
    {
        floorTile.color = color;
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, floorTile);
    }

    public void PaintSingleCorridorTile(Vector2Int position)
    {
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, corridorTile);
    }

    /// <summary>
    /// Eliminates all tiles of the tilemap
    /// </summary>
    public void ClearTilemap()
    {
        floorTilemap.ClearAllTiles();
        floorTile.color = Color.white;
    }

    /// <summary>
    /// Eliminates from the tilemap all tiles 
    /// </summary>
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

    /// <summary>
    /// Paints n tiles on a given tilemap
    /// </summary>
    /// <param name="positions">Tiles postions</param>
    /// <param name="tilemap">The tilemap</param>
    /// <param name="tile">The tile we want to paint</param>
    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (Vector2Int pos in positions)
        {
            PaintSingleTile(pos, tilemap, tile);
        }
    }

    /// <summary>
    /// Paints a single tile on a given tilemap
    /// </summary>
    /// <param name="position">Tile position</param>
    /// <param name="tilemap">The tilemap</param>
    /// <param name="tile">The tile we want to paint</param>
    private void PaintSingleTile(Vector2Int position, Tilemap tilemap, TileBase tile)
    {
        Vector3Int tileMapPosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tileMapPosition, tile);
    }
}
