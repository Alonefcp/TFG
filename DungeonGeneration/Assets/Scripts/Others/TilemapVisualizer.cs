using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tile floorTile;
    [SerializeField] private Tile wallTile;
    [SerializeField] private Tile pathTile;


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

    public void PaintWallTiles(IEnumerable<Vector2Int> positions)
    {
        PaintTiles(positions, wallTilemap, wallTile);
    }

    public void PaintPathTiles(IEnumerable<Vector2Int> positions)
    {
        PaintTiles(positions, floorTilemap, pathTile);
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
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, floorTile);
    }

    public void PaintSingleFloorTile(Tile tile,Vector2Int position)
    {
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, tile);
    }

    public void PaintSingleFloorTileWithColor(Vector2Int position, Color color)
    {
        floorTile.color = color;
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, floorTile);
    }

    public void PaintSingleWallTile(Vector2Int position)
    {
        Vector3Int tileMapPosition = wallTilemap.WorldToCell((Vector3Int)position);
        wallTilemap.SetTile(tileMapPosition, wallTile);
    }

    public void PaintSingleWallTile(Tile tile, Vector2Int position)
    {
        Vector3Int tileMapPosition = wallTilemap.WorldToCell((Vector3Int)position);
        wallTilemap.SetTile(tileMapPosition, tile);
    }


    public void PaintSinglePathTile(Vector2Int position)
    {
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, pathTile);
    }

    public void EraseSingleFloorTile(Vector2Int position)
    {
        PaintSingleTile(position,floorTilemap,null);
    }

    public void EraseSingleWallTile(Vector2Int position)
    {
        PaintSingleTile(position, wallTilemap, null);
    }

    /// <summary>
    /// Eliminates all tiles of the tilemap
    /// </summary>
    public void ClearTilemaps()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        floorTile.color = Color.white;
    }

    /// <summary>
    /// Eliminates from the tilemap all cells without a tile sourronded by cells with tiles
    /// </summary>
    /// <param name="positions">Positions which become walkable cells</param>
    public void EliminateSingleSpaces(out HashSet<Vector2Int> positions)
    {
        positions = new HashSet<Vector2Int>();

        foreach (var position in floorTilemap.cellBounds.allPositionsWithin)
        {
            Vector2Int pos = (Vector2Int)position;

            bool hasTile = floorTilemap.HasTile((Vector3Int)pos);
            bool hasUpperTile = floorTilemap.HasTile((Vector3Int)(pos + new Vector2Int(0, 1)));
            bool hasBottomTile = floorTilemap.HasTile((Vector3Int)(pos + new Vector2Int(0, -1)));
            bool hasRightTile = floorTilemap.HasTile((Vector3Int)(pos + new Vector2Int(1, 0)));
            bool hasLeftTile = floorTilemap.HasTile((Vector3Int)(pos + new Vector2Int(-1, 0)));

            if (!hasTile && hasUpperTile && hasBottomTile && hasRightTile && hasLeftTile)
            {
                Vector3Int tileMapPosition = floorTilemap.WorldToCell(position);
                floorTilemap.SetTile(tileMapPosition, floorTile);
                positions.Add(pos);
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