using System.Collections.Generic;
using System;
using UnityEngine;

public interface ITileListControl
{
    int Count { get; }
    int XDim { get; }
    int YDim { get; }
    int ZDim { get; }

    void CloneList(AbstractTileControl cloneFrom);
    void AssignTileType(HexCoordinates coords, TileType tileType);
    void AssignSolidType(HexCoordinates coords, TileType tileType);
    void AssignFloorType(HexCoordinates coords, TileType tileType);
    Tile GetTile(HexCoordinates coords);
    Tile GetTile(int x, int z, int y);
    TileType GetFloorType(HexCoordinates coords);
    TileType GetSolidType(HexCoordinates coords);
    bool IsBruteVisable(HexCoordinates location, HexCoordinates hexCoordinates);
    void SetTile(HexCoordinates coords, Tile tile);
    void SetTile(int x, int z, int y, Tile tile);
    bool TileBlocked(HexCoordinates value);
    bool ValidDirection(HexCoordinates coords, HexDirection direction);
    Vector3 TileLocation(HexCoordinates coords);
    bool HasSolidFloor(HexCoordinates source);
    bool IsTileNull(HexCoordinates coords); //Valid query.
    //HexCoordinates IndexToCoords(int index);
    Vector3 TileLocation(Vector3 tileOffsets);
    List<Tile> TileNeighbors(HexCoordinates coords);
    List<Tile> TileNeighbors(HexCoordinates coords, bool searchDepth); //I... I think this should work???

    // Tile GetTile(HexCoordinates coords);
    //  Tile GetTile(int x, int z, int y);
    //  Tile GetTile(int index); //nobody gets to know tiles directly anymore fuck u
}