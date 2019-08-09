using System.Collections.Generic;
using System;
using UnityEngine;


public abstract class AbstractTileControl : ITileListControl
{
    protected TileListBase BaseList;
    public abstract int Count { get; }
    public abstract int XDim { get; }
    public abstract int YDim { get; }
    public abstract int ZDim { get; }
    public abstract void SetTile(HexCoordinates coords, Tile tile);
    public abstract void SetTile(int x, int z, int y, Tile tile);
    public abstract Tile GetTile(HexCoordinates coords);
    public abstract Tile GetTile(int x, int z, int y);
    abstract public Vector3 TileLocation(HexCoordinates coords);

    public void CloneList(AbstractTileControl cloneFrom)
    {
        BaseList = new TileListBase(cloneFrom.BaseList);
    }
    public void AssignTileType(HexCoordinates tileCoords, TileType tileType)
    {
        GetTile(tileCoords).AssignSolidType(tileType);
    }

    /// <summary>
    /// Get the solid type if it exists, or gets the floor type if it exists, or gets null.
    /// </summary>
    /// <param name="coords"></param>
    /// <returns></returns>

    public bool IsTileNull(HexCoordinates coords) //Valid query?
    {
        return GetTile(coords) == null;
    }

    public List<Tile> TileNeighbors(HexCoordinates coords) { return TileNeighbors(coords, false); }
    public List<Tile> TileNeighbors(HexCoordinates coords, bool searchDepth) //I... I think this should work???
    {
        HexCoordinates tileCoords = coords;
        List<HexCoordinates> coordsList = tileCoords.GetCoordsWithinDistance(1);
        coordsList.Remove(coords);
        List<Tile> ret = new List<Tile>();
        for (int i = 0; i < coordsList.Count; i++)
        {
            if (GetTile(coordsList[i]) != null)
            {
                ret.Add(GetTile(coordsList[i]));
            }
        }
        return ret;
    }

    public bool TileBlocked(HexCoordinates value) //Maybe should be a query directly to tile instead 
    {
        return GetTile(value).IsBlocked;
    }

    public bool IsBruteVisable(HexCoordinates location, HexCoordinates hexCoordinates) //Not good at all, actualy.
    {
        List<HexCoordinates> coords = HexCoordinates.cube_linedraw(location, hexCoordinates);

        for (int i = 0; i < coords.Count; i++)
        {
            if (GetSolidType(coords[i]) == null)
            { return false; }
        }

        return true;
    }

    public bool HasSolidFloor(HexCoordinates source) //Worthwhile data query
    {
        if (GetTile(source) == null) { return false; } //tile doesnt exist
        if (GetTile(source).HasSolidFloor) { return true; } //tile has a proper floor (still might be blocked!)
        if (GetTile(source.Below) == null) { return false; } //tile has no floor, tile under it is invalid
        if (GetTile(source.Below).IsSolidBlock) { return true; } //tile has no floor, but solid SolidBlock under it can be a floor
        return false;
    }

    public bool HasAnyFloor(HexCoordinates source) //Worthwhile query
    {
        if (GetTile(source) == null) { return false; } //tile doesnt exist
        if (GetTile(source).floorType != null) { return true; } //tile has a proper floor (still might be blocked!)
        if (GetTile(source.Below) == null) { return false; } //tile has no floor, tile under it is invalid
        if (GetTile(source.Below).solidType != null) { return true; } //tile has no floor, but a SolidBlock under it can be a floor
        return false;
    }
    /*
    public void PlaceTransformsOnTile(HexCoordinates coords, Transform transform)  //Superior to the above due to not using the index
    {
        Vector3 TileCoords = TileLocation(coords);
        MeshRenderer MeshRender = transform.GetComponent<MeshRenderer>();
        float ObjectHeight = 0;
        if (MeshRender)
        {
            ObjectHeight = transform.GetComponent<MeshRenderer>().bounds.extents.y;
        }
        transform.localPosition = new Vector3(TileCoords.x, TileCoords.y + ObjectHeight, TileCoords.z); 
    }
    */

    public void AssignFloorType(HexCoordinates coords, TileType tileType)
    {
        Tile T = GetTile(coords);
        if (T != null) { T.AssignFloorType(tileType); }
    }

    public void AssignSolidType(HexCoordinates coords, TileType tileType)
    {
        Tile T = GetTile(coords);
        if (T != null) { T.AssignSolidType(tileType); }
    }

    abstract public Vector3 TileLocation(Vector3 tileOffsets);
    abstract public bool ValidDirection(HexCoordinates coords, HexDirection direction);

    virtual public TileType GetSolidType(HexCoordinates coords)
    {
        return BaseList.GetSolidType(coords);
    }

    virtual public TileType GetFloorType(HexCoordinates coords)
    {
        return BaseList.GetFloorType(coords);
    }
}