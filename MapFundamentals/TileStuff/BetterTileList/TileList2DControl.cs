using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Treats a TileList as a flat map. Assumes highest non-null tile is the only tile on any XZ location, any adjustments brute force things to keep it that way. For use with a map generator.
/// </summary>
[RequireComponent(typeof(TileListBase))]
public class TileList2DControl : AbstractTileControl
{

    //protected TileListBase BaseList;

    public TileList2DControl(TileListBase baseList)
    {
        BaseList = baseList;
    }

    public TileList2DControl(TileList2DControl list2DControl)
    {
        BaseList = new TileListBase(list2DControl.BaseList);
    }

    override public int Count { get { return BaseList.XDim * BaseList.ZDim; } }
    public int Capacity { get { return Count; } }

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

    private Tile GetTrueTile(HexCoordinates coords) //Gets the actual tile from this coord, use with caution.
    {
        return BaseList.GetTile(coords);
    }

    override public Tile GetTile(HexCoordinates coords)
    {
        Vector3 offsets = BaseList.CoordsToOffsets(coords);
        if (offsets.x >= XDim || offsets.x < 0 || offsets.z >= ZDim || offsets.z < 0) { return null; }
        return BaseList.GetTile(new Vector3(offsets.x, TileHeight((int)offsets.x, (int)offsets.z), offsets.z));
    }
    /// <summary>
    /// Returns Y coords of highest tile at this location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public int TileHeight(int x, int z)
    {
        if (x >= XDim || x < 0 || z >= ZDim || z < 0) { throw new Exception("Invalid location!"); }
        for (int i = BaseList.YDim - 1; i >= 0; i--)
        {
            HexCoordinates CheckCoords = HexCoordinates.FromOffsetCoordinates(new Vector3(x, i, z));
            Tile Check = BaseList.GetTile(CheckCoords);
            //if (BaseList.GetFloorType(CheckCoords) != null || BaseList.GetSolidType(CheckCoords) != null)
            if(Check.floorType!=null || Check.solidType != null)
            {
                return i;
            }
        }
        return 0;
    }

    private int TileHeight(HexCoordinates coords)
    {
        Vector3 offsets = HexCoordinates.ToOffsetCoordinates(coords);
        return TileHeight((int)offsets.x, (int)offsets.z);
    }

    /// <summary>
    /// Gets highest tile at this offset location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public Tile GetTile(int x, int z) //via offsets!
    {
        int num = TileHeight(x, z);
        return BaseList.GetTile(new Vector3(x, num, z));
    }

    override public Tile GetTile(int x, int z, int NOTUSED)
    {
        return GetTile(x, z);
    }
    /// <summary>
    /// returns the actual hexcoords of the tile at the index
    /// </summary>
    private HexCoordinates IndexToCoords(int index)
    {
        if (index >= this.Count || index < 0) { throw new ArgumentOutOfRangeException(this.Count.ToString() + " is the max size, " + index.ToString()); }
        HexCoordinates ret = BaseList.IndexToCoords(index);
        int layer = TileHeight(ret);
        ret = new HexCoordinates(ret.X, ret.Z, layer);
        return ret;
    }

    override public void SetTile(HexCoordinates coords, Tile tile)
    {
        Tile holdme = tile; //prevent overwriting a tile when swapping
        Vector3 TargetOffsets = BaseList.CoordsToOffsets(coords);
        int TargetHeight = TileHeight((int)TargetOffsets.x, (int)TargetOffsets.z);
        if (TargetHeight > coords.Depth) //tile(s) exist at a higher level than where this tile is being set
        {
            SetTileHeight(coords, coords.Depth);
        }
        BaseList.SetTile(coords, holdme);
    }

    public void SetTile(int x, int z, Tile tile) // via offsets!
    {
        int? index = BaseList.OffsetsToIndex(new UnityEngine.Vector3(x, TileHeight(x, z), z));
        if (index.HasValue)
        {
            SetTile(IndexToCoords(index.Value), tile);
        }
    }

    override public void SetTile(int x, int z, int tileHeight, Tile tile) //Set a tile, then set its height to this level.
    {
        SetTile(HexCoordinates.FromOffsetCoordinates(x,z,tileHeight), tile);
    }

    public void SetTile(int index, Tile tile)
    {
        if (index >= Count) { throw new ArgumentOutOfRangeException(); }
        HexCoordinates TrueCoords = IndexToCoords(index);
        SetTile(TrueCoords, tile);
    }

    protected Tile GetTile(int index)
    {
        if (index >= Count || index < 0) { throw new ArgumentOutOfRangeException(String.Format("Index:{0}, Length:{1}", index, Count)); }
        int? TrueIndex;
        HexCoordinates trueCoords = IndexToCoords(index);
        TrueIndex = BaseList.CoordsToIndex(trueCoords);
        if (!TrueIndex.HasValue)
        {
            string errorstring = String.Format("Index:{0}, TrueIndex:null, TileHeight:{4}, trueCoords:{2}, TrueListCount:{3} Offsets:{5}", index, TrueIndex, trueCoords.ToString(), BaseList.Count, "n/a", BaseList.CoordsToOffsets(trueCoords));
            throw new Exception(errorstring);//I dont know if this ever comes up. 
        }
        Tile ret = BaseList.GetTile(BaseList.IndexToCoords(TrueIndex.Value)); //converting it to this then converting right back... oi.
        if (ret == null) //Also never comes up, but STILL...
        {
            string errorstring = String.Format("Index:{0}, TrueIndex:{1}, TileHeight:{4}, trueCoords:{2}, TrueListCount:{3}", index, TrueIndex, trueCoords.ToString(), BaseList.Count, "n/a");
            throw new Exception(errorstring);
        }
        return ret;
    }
    public void SetTileHeight(HexCoordinates coords, int newHeight)
    {
        Vector3 offsets = HexCoordinates.ToOffsetCoordinates(coords);
        SetTileHeight((int)offsets.x, (int)offsets.z, newHeight);
    }

    public void SetTileHeight(int x, int z, int newHeight)
    {
        //Place the highest tile at the x,z offset at the stated height, and ensure it remains the highest tile at said location
        //This operation will null out all tiles between old and new if new height is less than the old height

        int oldHeight = TileHeight(x, z); //prior height of highest tile at XZ
        if (oldHeight == newHeight) { return; } //if height is not actually being changed, stop

        int? DesiredIndex = BaseList.OffsetsToIndex(new Vector3(x, newHeight, z)); //index new tile will be placed at in BaseList
        if (!DesiredIndex.HasValue) { return; }  //if outside the correct range, do nothing

        Tile OldTile = GetTile(x, z); //Gets the highest tile at XZ
        HexCoordinates OldCoords = HexCoordinates.FromOffsetCoordinates(x, z, oldHeight); //coords of tile
        HexCoordinates OldCoordsAbove = OldCoords.Above;
        Tile OldTileAbove = GetTrueTile(OldCoordsAbove); //Since things could be "on top" of the old tile, it should be moved as well
        HexCoordinates NewCoords = HexCoordinates.FromOffsetCoordinates(x, z, newHeight);
        HexCoordinates NewCoordsAbove = NewCoords.Above;
        if (oldHeight < newHeight)
        {
            BaseList.SetTile(NewCoordsAbove, OldTileAbove);
            BaseList.SetTile(OldCoordsAbove, null);
            BaseList.SetTile(NewCoords, OldTile);
            BaseList.SetTile(OldCoords, null);
        }
        else
        {
            BaseList.SetTile(NewCoords, OldTile);
            BaseList.SetTile(NewCoordsAbove, OldTileAbove);
            BaseList.SetTile(OldCoordsAbove, null);
            while (true)
            {
                NewCoordsAbove = NewCoordsAbove.Above;
                if (BaseList.CoordsToIndex(NewCoordsAbove).HasValue)
                {
                    BaseList.SetTile(NewCoordsAbove, null);
                }
                else { break; }
            }
        }
        // OldTile = GetTile(DesiredIndex); //should be the "moved" tile
        //  OldTileAbove = GetTrueTile(NewAbove);
        /*
            if (tile.HasActor) { PlaceTransformsOnTile(index, tile.Actor.transform); }
            if (tile.Features != null && tile.Features.Count>0) { for (int i = 0; i < tile.Features.Count; i++) { tile.Features[i].Location = Coords; _TrueList.PlaceTransformsOnTile(_TrueList.CoordsToIndex(Coords).Value, tile.Features[i].transform); } }
            if (abovetile.HasActor) { PlaceTransformsOnTile(_TrueList.CoordsToIndex(NewAbove).Value, abovetile.Actor.transform); }
            if (abovetile.Features != null && abovetile.Features.Count > 0) { for (int i = 0; i < abovetile.Features.Count; i++) { abovetile.Features[i].Location = NewAbove; _TrueList.PlaceTransformsOnTile(_TrueList.CoordsToIndex(NewAbove).Value, abovetile.Features[i].transform); } }
            */
    }

    public void SwapTiles(ref Tile a, ref Tile b)
    {
        Tile temp = a;
        a = b;
        b = temp;
    }

    override public Vector3 TileLocation(HexCoordinates coords)
    {
        //Debug.Log(TileHeight[tileIndex] + " " + _TrueList.TileLocation(IndexToCoords(tileIndex)).y + " " + IndexToCoords(tileIndex).Depth); //why is this always 0 0 0??
        return BaseList.TileLocation(coords);
    }

    internal void AddMapBottom(TileType bottomType)
    {
        List<HexCoordinates> GrowDown = new List<HexCoordinates>();
        for (int i = 0; i < this.Count; i++)
        {
            if (GetTile(i).floorType != null || GetTile(i).solidType != null)
            {
                GrowDown.Add(IndexToCoords(i));
            }
        }
        for (int i = 0; i < GrowDown.Count; i++)
        {
            HexCoordinates Check = GrowDown[i];
            TileType what = bottomType;
            while (true)
            {
                Check = Check.Below;
                Tile checkTile = BaseList.GetTile(Check);
                if (checkTile != null)
                {
                    checkTile.solidType = what;
                }
                else { break; }
            }
        }
    }

    override public Vector3 TileLocation(Vector3 tileOffsets)
    {
        float offset = HexMetrics.innerRadius * ((int)tileOffsets.z & 1);
        Vector3 ret = new Vector3(tileOffsets.x * HexMetrics.innerRadius * 2f + offset, tileOffsets.y * HexMetrics.elevationStep, -1 * tileOffsets.z * HexMetrics.outerRadius * 1.5f);
        return ret;
    }

    override public bool ValidDirection(HexCoordinates coords, HexDirection direction)
    {
        return BaseList.CoordsToIndex(coords.DirectionTransform((HexDirection3D)(direction))).HasValue;
    }

    override public TileType GetSolidType(HexCoordinates coords)
    {
        if (BaseList.GetTile(coords) == null) { return null; }
        Vector3 offsets = HexCoordinates.ToOffsetCoordinates(coords);
        int height = TileHeight((int)offsets.x, (int)offsets.z);
        coords = HexCoordinates.FromOffsetCoordinates((int)offsets.x, (int)offsets.z, height);
        return BaseList.GetSolidType(coords);
    }

    public override TileType GetFloorType(HexCoordinates coords)
    {
        if (BaseList.GetTile(coords) == null) { return null; }
        Vector3 offsets = HexCoordinates.ToOffsetCoordinates(coords);
        int height = TileHeight((int)offsets.x, (int)offsets.z);
        coords = HexCoordinates.FromOffsetCoordinates((int)offsets.x, (int)offsets.z, height);
        return BaseList.GetFloorType(coords);
    }
}
