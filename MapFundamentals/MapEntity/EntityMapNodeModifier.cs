using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(MapEntity))]
    public class EntityMapNodeModifier : MonoBehaviour
    {
    public bool SoloTileObject; //only it can stand in a space
    public bool BlockingObject; //cannot pass thru it
    public bool SolidFloor; // can stand or walk on it
    public HexDirection3DFlags BlockedDirections; //cannot enter/exit space from this direction; functions independently of all the above

    internal bool IsEntityBlocked(MapEntity entity)
    {
        if (!BlockingObject) { return false; }
        EntityMapNodeModifier OtherEnt = entity.GetComponent<EntityMapNodeModifier>();
        if(OtherEnt == null) { return false; }
        return OtherEnt.BlockingObject;
    }
}

[Flags]
public enum HexDirection3DFlags 
{
    UNASSIGNED = 0,
    NE = 1<<HexDirection3D.NE,
    E = 1<< HexDirection3D.E,
    SE = 1<< HexDirection3D.SE,
    SW = 1<< HexDirection3D.SW,
    W = 1<< HexDirection3D.W,
    NW = 1<< HexDirection3D.NW,
    UP = 1<<HexDirection3D.UP,
    DOWN = 1<<HexDirection3D.DOWN
}

public static class HexDirection3DFlagsExtend
{
    public static List<HexDirection3D> GetValues(this HexDirection3DFlags flags)
    {
        List<HexDirection3D> ReturnValues = new List<HexDirection3D>();
        foreach (HexDirection3D value in HexDirection3DFlags.GetValues(typeof(HexDirection3D)))
        {
            if( (flags&value.ConvertToFlag())==value.ConvertToFlag())
            {
                ReturnValues.Add(value);
            }
        }
        return ReturnValues;
    }

    public static bool CompareToNotFlag(this HexDirection3DFlags a, HexDirection3D b)
    {
        return (a & b.ConvertToFlag()) == b.ConvertToFlag();
    }

    public static HexDirection3DFlags ConvertToFlag(this HexDirection3D value)
    {
        return (HexDirection3DFlags)(1 << (int)value);
    }
}
