using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EntityMovement : MonoBehaviour, IEntityMovement
{
    [SerializeField]
    private int _MovementRange;
    public int MovementRange
    {
        get { return _MovementRange; }
    }

    [SerializeField]
    private int _ElevationUpMax;
    public int ElevationUpMax
    {
        get
        {
            return _ElevationUpMax;
        }
    }
    [SerializeField]
    private int _ElevationDownMax;
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
            return GetComponent<MapEntity>();
        }
    }

    [SerializeField]
    private int PLACEHOLDERVALUE = 1;
    public int FloorMovementCost(TileType tileType)
    {
        return PLACEHOLDERVALUE;
    }

    public int SolidMovementCost(TileType tileType)
    {
        return PLACEHOLDERVALUE;
    }
}
