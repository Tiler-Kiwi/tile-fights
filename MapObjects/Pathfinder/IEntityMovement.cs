using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// gives rules on how an entity may move
/// </summary>

public interface IEntityMovement 
{
    int MovementRange { get; }
    int ElevationUpMax { get; }
    int ElevationDownMax { get; }
    int FloorMovementCost(TileType tileType);
    int SolidMovementCost(TileType tileType);
    MapEntity MyEntity { get; }

    //HexCoordinates? StepForward(HexCoordinates source, HexDirection dir, ITileListControl sourceMap);
}

/// <summary>
/// Default entity movement for use in general pathfinding tasks
/// </summary>
public class DefaultEnityMovement : IEntityMovement
{

    public int MovementRange
    {
        get
        {
            return int.MaxValue;
        }
    }

    public int ElevationUpMax
    {
        get
        {
            return int.MaxValue;
        }
    }

    public int ElevationDownMax
    {
        get
        {
            return int.MaxValue;
        }
    }

    public int FloorMovementCost(TileType tileType)
    {
        if(tileType == null) { return -1; }
        return 1;
    }

    public int SolidMovementCost(TileType tileType)
    {
        if(tileType == null) { return 0; }
        return -1;
    }

    public MapEntity MyEntity { get { return null; } }
}

/// <summary>
/// for use in tasks where a non monobehaviour movement data thing is preferable
/// </summary>
public class NonMonoMovement : IEntityMovement
{
    private int _MovementRange = int.MaxValue;
    private int _ElevationUpMax = int.MaxValue;
    private int _ElevationDownMax = int.MaxValue;

    public NonMonoMovement(int range, int up, int down)
    {
        _MovementRange = range;
        _ElevationUpMax = up;
        _ElevationDownMax = down;
    }

    public int MovementRange
    {
        get
        {
            return _MovementRange;
        }
    }

    public int ElevationUpMax
    {
        get
        {
            return _ElevationUpMax;
        }
    }

    public int ElevationDownMax
    {
        get
        {
            return _ElevationDownMax;
        }
    }

    public MapEntity MyEntity
    {
        get
        {
            return null;
        }
    }

    public int FloorMovementCost(TileType tileType)
    {
        if (tileType == null) { return -1; }
        return 1;
    }

    public int SolidMovementCost(TileType tileType)
    {
        if (tileType == null) { return 0; }
        return -1;
    }
}