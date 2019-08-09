using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(MeshRenderer))]

public class TileTypeChanged : EventArgs //for use in signaling need to update rendering information
{
    public Tile ChangedTile;
}

public class TileActorChanged : EventArgs
{
    public Tile ModifiedTile;
}

/*
 * unit of what makes up a map
 * null tile is offmap
 * null tiletype is empty space
 */

    [Serializable]
public class Tile
{
    [SerializeField]
    private TileType _solidType;
    [SerializeField]
    private TileType _floorType;

    private Color OverrideColor;
    private bool _UseOverrideColor = false;
    public bool UseOverrideColor
    {
        get { return _UseOverrideColor; }
        set
        {
            if (value != _UseOverrideColor) //dont fire event if its not actually changing the colors
            {
                _UseOverrideColor = value;
                TileTypeChanged e = new TileTypeChanged();
                e.ChangedTile = this;
                OnTileChange(e);
            }
        }
    }

    public Color? MySolidColor
    {
        get
        { if (UseOverrideColor)
            { return OverrideColor; }
        if(solidType == null) { return null; }
            return solidType.TileTypeColor;
        }
        set
        {
            if(!value.HasValue)
            {
                UseOverrideColor = false;
            }
            OverrideColor = value.Value;
            if(UseOverrideColor != false) { _UseOverrideColor = false; } //to make sure event fires
            UseOverrideColor = true;
        }
    }

    public Color? MyFloorColor
    {
        get
        {
            if (UseOverrideColor)
            { return OverrideColor; }
            if(floorType == null) { return null; }
            return floorType.TileTypeColor;
        }
        set
        {
            if (!value.HasValue)
            {
                UseOverrideColor = false;
            }
            OverrideColor = value.Value;
            if (UseOverrideColor != false) { _UseOverrideColor = false; } //makes sure event fires
            UseOverrideColor = true;
        }
    }


    public EventHandler<TileTypeChanged> TileTypeChangedEvent;

    public Tile()
    {
        _solidType = null;
        _floorType = null;
    }

    public Tile(TileType solidType, TileType floorType)
    {
        _solidType = solidType;
        _floorType = floorType;
    }

    public TileType solidType
    { get { return _solidType; }
        set
        {
            AssignSolidType(value);
        }
    }

    public TileType floorType
    {
        get
        { if(!_floorType)
            {
                return solidType;
            }
            return _floorType;
        }
        set { AssignFloorType(value); }
    }

    public bool IsBlocked //square is occupied by something
    {
        get
        {
            if (solidType != null){ return true; }
            return false;
        }

        set { }
    }

    public bool BlocksVisibility
    {
        get
        {
            if (solidType != null) { return true; }
            return false;
        }
    }

    public bool HasSolidFloor {
        get {
            if (!floorType) { return false; }
            return floorType.Solid;
        } set { } }
    public bool IsSolidBlock {
        get {
            if (!solidType) { return false; }
            return solidType.Solid;
        } set { } }

    protected void OnTileChange(TileTypeChanged e)
    {
        EventHandler<TileTypeChanged> handler = TileTypeChangedEvent;
        if(handler != null)
        {
            handler(this, e);
        }
    }

    public void AssignSolidType(TileType tt)
    {
        _solidType = tt;
        TileTypeChanged tileChange = new TileTypeChanged();
        tileChange.ChangedTile = this;
        OnTileChange(tileChange);
    }

    public void AssignFloorType(TileType tt)
    {
        _floorType = tt;
        TileTypeChanged tileChange = new TileTypeChanged();
        tileChange.ChangedTile = this;
        OnTileChange(tileChange);
    }

    /// <summary>
    /// Removes all features from the tile that are instances of the prefab provided. Returns a list of removed instances.
    /// </summary>
    /// <param name="featurePrefab"></param>
    /// <returns></returns>
}
