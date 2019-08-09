using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TileListBase))]
public class TileList3DControl : AbstractTileControl
{
    //TileListBase BaseList;
    public TileList3DControl(TileListBase baseList)
    {
        BaseList = baseList;
    }
    override public int Count
    {
        get
        {
            return BaseList.Count;
        }
    }

    override public int XDim
    {
        get
        {
            return BaseList.XDim;
        }
    }

    override public int YDim
    {
        get
        {
            return BaseList.YDim;
        }
    }

    override public int ZDim
    {
        get
        {
            return BaseList.ZDim;
        }
    }
    override public Tile GetTile(HexCoordinates coords)
    {
        return BaseList.GetTile(coords);
    }

    override public Tile GetTile(int x, int z, int y)
    {
        return BaseList.GetTile(new Vector3(x, y, z));
    }

    private HexCoordinates IndexToCoords(int index)
    {
        return BaseList.IndexToCoords(index);
    }

    private int? OffsetsToIndex(Vector3 offsets)
    {
        return BaseList.OffsetsToIndex(offsets);
    }

    override public void SetTile(HexCoordinates coords, Tile tile)
    {
        BaseList.SetTile(coords, tile);
    }

    override public void SetTile(int x, int z, int y, Tile tile)
    {
        BaseList.SetTile(new Vector3(x, y, z), tile);
    }

    override public Vector3 TileLocation(HexCoordinates coords)
    {
        return TileLocation(BaseList.CoordsToOffsets(coords));
    }

    override public Vector3 TileLocation(Vector3 tileOffsets)
    {
        float offset = HexMetrics.innerRadius * ((int)tileOffsets.z & 1);
        Vector3 ret = new Vector3(tileOffsets.x * HexMetrics.innerRadius * 2f + offset, tileOffsets.y * HexMetrics.elevationStep, -1 * tileOffsets.z * HexMetrics.outerRadius * 1.5f);
        return ret;
    }

    override public bool ValidDirection(HexCoordinates coords, HexDirection direction) //Valid query
    {
        return BaseList.CoordsToIndex(coords.DirectionTransform((HexDirection3D)(direction))).HasValue;
    }
}
