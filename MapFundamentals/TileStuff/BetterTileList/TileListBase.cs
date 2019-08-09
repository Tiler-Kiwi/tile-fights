using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Holds a list of tiles, storing a 3d map into a 1D array, and providing basic access methods. Tiles may only be accessed via HexCoordinates or offsets via Vector3 (width, depth, height).
/// </summary>

    [Serializable]
    public class TileListBase
    {
    [SerializeField]
    private List<Tile> _MapTiles;
    [SerializeField]
    int _XDim; //width of map <- ->
    [SerializeField]
    int _YDim; //depth of map
    [SerializeField]
    int _ZDim; //height of map ^ V

    //Dimensions are in offset coords, Y serving as depth
    /// <summary>
    /// width of map <- ->
    /// </summary>
    public int XDim { get { return _XDim; } }
    /// <summary>
    /// depth of map
    /// </summary>
    public int YDim { get { return _YDim; } }
    /// <summary>
    /// height of map ^ V
    /// </summary>
    public int ZDim { get { return _ZDim; } }
    public int Count { get { return _MapTiles.Count; } }

    //Constructors for non-monobehavior version, if needed later.
    
    public TileListBase(int XDim, int ZDim, int YDim)
    {
        _XDim = XDim;
        _ZDim = ZDim;
        _YDim = YDim;
        _MapTiles = new List<Tile>(XDim * ZDim * YDim);
        for (int i = 0; i < _MapTiles.Capacity; i++)
        {
            _MapTiles.Add(null);
            this.SetTile(i, new Tile());
        }
    }
    public TileListBase(TileListBase tilelist) //deep copy
    {
        _XDim = tilelist.XDim;
        _ZDim = tilelist.ZDim;
        _YDim = tilelist.YDim;
        _MapTiles = new List<Tile>(XDim * ZDim * YDim);
        if (_MapTiles.Capacity == 0) { throw new Exception("Don't make zero capacity tile lists please."); }
        for (int i = 0; i < _MapTiles.Capacity; i++)
        {
            _MapTiles.Add(null);
            if (tilelist.GetTile(i) != null)
            {
                //throw new Exception("Its working");
                this.SetTile(i, new Tile(tilelist.GetTile(i).solidType, tilelist.GetTile(i).floorType));
            }
            else { this.SetTile(i, new Tile()); }
        }
    }
    
    public Tile GetTile(HexCoordinates tileCoords)
    {
        int? index = CoordsToIndex(tileCoords);
        if (index.HasValue) { return this.GetTile(index.Value); }
        return null;
    }

    public void SetTile(HexCoordinates tileCoords, Tile tile)
    {
        int? index = CoordsToIndex(tileCoords);
        if (index.HasValue)
        {
            SetTile(index.Value, tile);
        }
    }

    public TileType GetFloorType(HexCoordinates coords)
    {
        Tile Target = GetTile(coords);
        if(Target == null) { return null; }
        //return Target.floorType;
        
        if(Target.solidType != null) { return Target.solidType; }
        if (Target.floorType != null) { return Target.floorType; }
        Tile BelowTarget = GetTile(coords.Below);
        if(BelowTarget == null) { return null; }
        return BelowTarget.solidType;
        
    }

    internal bool ValidCoords(HexCoordinates coords)
    {
        return (GetTile(coords) != null) ;
    }

    public TileType GetSolidType(HexCoordinates coords)
    {
        Tile Target = GetTile(coords);
        if(Target.solidType != null) { return Target.solidType; }
        return null;
    }

    public Tile GetTile(Vector3 offsets)
    {
        int? index = OffsetsToIndex(offsets);
        if (index.HasValue)
        {
            return GetTile(index.Value);
        }
        return null;
    }
    public void SetTile(Vector3 offsets, Tile tile)
    {
        int? index = OffsetsToIndex(offsets);
        if (index.HasValue)
        {
            SetTile(index.Value, tile);
        }
    }

    public int? CoordsToIndex(HexCoordinates coords)
    {
        return OffsetsToIndex(CoordsToOffsets(coords));
        //return ((coords.Z * _XDim) + (coords.Y + (coords.Z - (coords.Z & 1)) / 2)) + (coords.Depth * _XDim * _ZDim);
    }

    public Vector3 IndexToOffsets(int index)
    {
        if (index < 0 || index > this.Count)
        {
            throw new ArgumentOutOfRangeException("Index provided is out of range");
        }
        int layer = index / (XDim * ZDim); //y
        int tIndex = index - (layer * XDim * ZDim);
        int col = tIndex % XDim; // x
        int row = tIndex / XDim; // z
        return new Vector3(col, layer, row);
    }
    public HexCoordinates OffsetsToCoords(Vector3 offsets)
    {
        return HexCoordinates.FromOffsetCoordinates(offsets);
        /*
        int x = (int)(offsets.x - (offsets.z - ((int)offsets.z & 1)) / 2);
        int z = (int)offsets.z;
        int depth = (int)offsets.y;
        return new HexCoordinates(x, z, depth);
        */
    }
    public Vector3 CoordsToOffsets(HexCoordinates coords)
    {
        return HexCoordinates.ToOffsetCoordinates(coords);
        /*
        int col = coords.X + (coords.Z - (coords.Z & 1)) / 2;
        int row = coords.Z;
        int layer = coords.Depth;
        Vector3 offsets = new Vector3(col, layer, row);
        return offsets;
        */
    }

    /// <summary>
    /// Get an index from offset values
    /// </summary>
    /// <param name="offsets"></param>
    /// <returns>Null if offset is not in range</returns>
    public int? OffsetsToIndex(Vector3 offsets)
    {
        if (offsets.x >= XDim || offsets.x < 0 ||
            offsets.y >= YDim || offsets.y < 0 ||
            offsets.z >= ZDim || offsets.z < 0) // need to make sure everything is actually in range
        {
            return null;
        }
        int index = (int)(offsets.x + offsets.y * XDim * ZDim + offsets.z * XDim);
        if (index >= this.Count || index < 0)
        {
            throw new Exception("this shouldn't happen.");
        }
        return index;
    }
    public HexCoordinates IndexToCoords(int index)
    {
        return OffsetsToCoords(IndexToOffsets(index));
    }

    private Tile GetTile(int index)
    {
        if (index >= this.Count || index < 0)
        {
            throw new Exception(String.Format("Please dont do this. Index {0} is invalid (this.Count is {1})", index, this.Count));
            //Its okay to provide invalid coords, but providing an invald index is not a good thing, probably.
           // return null;
        }
        return _MapTiles[index];
    }
    private void SetTile(int index, Tile tile)
    {
        //tile.name = String.Format("Tile {0}", index);
        if (tile == null)
        { tile = new Tile(); }
        _MapTiles[index] = tile;
    }

    public Vector3 TileLocation(HexCoordinates coords)
    {
        return TileLocation(CoordsToOffsets(coords));
    }

    public Vector3 TileLocation(Vector3 tileOffsets)
    {
        return HexMetrics.TileLocation(tileOffsets);
    }

    public void AssignTileList(List<Tile> newList, int xDim, int yDim, int zDim)
    {
        //Check that dims match with newList size
        if(xDim * yDim * zDim != newList.Count)
        {
            throw new ArgumentException("Dimensions do not match the size of the list");
        }

        this._MapTiles = newList;
        _XDim = xDim;
        _YDim = yDim;
        _ZDim = zDim;
    }

}

