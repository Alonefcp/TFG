using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [Header("Final Tiles")]
    [SerializeField] private Tile floorTile;
    [SerializeField] private Tile wallTop, wallSideRight, wallSideLeft, wallBottom, wallFull,
        wallInnerCornerDownLeft, wallInnerCornerDownRight, wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight,
        wallDiagonalCornerUpLeft, innerWall;
    [Header("Test Tiles")] //DEBUG
    [SerializeField] private Tile pathTestTile;
    [SerializeField] private Tile floorTestTile;
    [SerializeField] private Tile wallTestTile; 


    public void PaintPathTiles(IEnumerable<Vector2Int> positions) //DEBUG
    {
        PaintTiles(positions, floorTilemap, pathTestTile);
    }

    public void PaintSinglePathTile(Vector2Int position)//DEBUG
    {
        PaintSingleTile(position, floorTilemap, pathTestTile);
    }

    public void EraseTile(Vector2Int pos)
    {
        PaintSingleTile(pos, floorTilemap, null);
    }

    /// <summary>
    /// Returns the cell's radius. We assume a cell is n*n
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

    /// <summary>
    /// Paints a corner wall tile
    /// </summary>
    /// <param name="position">Tile position</param>
    /// <param name="type">Corner wall type</param>
    public void PaintSingleCornerWallTile(Vector2Int position, string type="")
    {
        int typeASInt = Convert.ToInt32(type, 2);
        TileBase tile = null;

        if (WallTypesHelper.wallInnerCornerDownLeft.Contains(typeASInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallTypesHelper.wallInnerCornerDownRight.Contains(typeASInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (WallTypesHelper.wallFullEightDirections.Contains(typeASInt))
        {
            tile = wallFull;
        }
        else if (WallTypesHelper.wallBottmEightDirections.Contains(typeASInt))
        {
            tile = wallBottom;
        }

        if (tile != null)
            PaintSingleTile(position, wallTilemap, tile);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position">Tyle position</param>
    /// <param name="type">Wall type</param>
    public void PaintSingleWallTile(Vector2Int position, string type = "")
    {
        int typeAsInt = Convert.ToInt32(type, 2);
        TileBase tile = null;
        if (WallTypesHelper.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }
        else if (WallTypesHelper.wallSideRight.Contains(typeAsInt))
        {
            tile = wallSideRight;
        }
        else if (WallTypesHelper.wallSideLeft.Contains(typeAsInt))
        {
            tile = wallSideLeft;
        }
        else if (WallTypesHelper.wallBottm.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        else if (WallTypesHelper.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }

        if (tile != null)
            PaintSingleTile(position, wallTilemap, tile);
    }

    /// <summary>
    /// Paints a single floor tile
    /// </summary>
    /// <param name="position">Tile position</param>
    public void PaintSingleFloorTile(Vector2Int position)
    {      
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, floorTile);
    }

    /// <summary>
    /// Paints a single floor tile with color
    /// </summary>
    /// <param name="position">Tile position</param>
    /// <param name="color">Tile color</param>
    public void PaintSingleFloorTileWithColor(Vector2Int position, Color color)
    {
        floorTestTile.color = color;
        Vector3Int tileMapPosition = floorTilemap.WorldToCell((Vector3Int)position);
        floorTilemap.SetTile(tileMapPosition, floorTestTile);
    }

    /// <summary>
    /// Paints a inner wall tile
    /// </summary>
    /// <param name="position">Tile position</param>
    public void PaintSingleInnerWallTile(Vector2Int position)
    {      
        PaintSingleTile(position,wallTilemap, innerWall);
    }

    /// <summary>
    /// Paints a tile on the wall tilemap
    /// </summary>
    /// <param name="tile">Wall tile</param>
    /// <param name="position">Tile position</param>
    public void PaintSingleWallTile(Tile tile, Vector2Int position)
    {
        PaintSingleTile(position, wallTilemap, tile);
    }

    /// <summary>
    /// Paints a tile on the floor tilemap
    /// </summary>
    /// <param name="tile">Floor tile</param>
    /// <param name="position">Tile position</param>
    public void PaintSingleFloorTile(Tile tile, Vector2Int position)
    {
        PaintSingleTile(position, floorTilemap, tile);
    }

    /// <summary>
    /// Eliminates all tiles of the tilemap
    /// </summary>
    public void ClearTilemaps()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        floorTestTile.color = Color.white;
    }

    /// <summary>
    /// Eliminates from the tilemap all cells without a tile sourronded by cells with tiles
    /// </summary>
    /// <param name="positions">Positions which become walkable cells</param>
    public void EliminateSingleSpaces(out HashSet<Vector2Int> positions)
    {
        positions = new HashSet<Vector2Int>();

        foreach (Vector3Int position in floorTilemap.cellBounds.allPositionsWithin)
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
