using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

public enum VertDirection
{
    UP,DOWN
}

public enum RotationDirection // god at this point i could have pitch and yaw ugh
{
    CLOCKWISE, COUNTERCLOCKWISE
}

public enum HexDirection3D
{
    NE, E, SE, SW, W, NW, UP, DOWN
}
/*
public struct HexDirection3D
{
    HexDirection? _HexDir;
    VertDirection? _VertDir;

    public HexDirection? HexDir { get { return _HexDir; } }
    public VertDirection? VertDir { get { return _VertDir; } }

    public HexDirection3D(HexDirection dir, VertDirection vertDir)
    {
        _HexDir = dir;
        _VertDir = vertDir;
    }
    public HexDirection3D(HexDirection dir)
    {
        _HexDir = dir;
        _VertDir = null;
    }
    public HexDirection3D(VertDirection vertDir)
    {
        _HexDir = null;
        _VertDir = vertDir;
    }
}

public static class VertDirectionExtensions
{
    public static VertDirection Opposite(this VertDirection direction)
    {
        if(direction == VertDirection.DOWN) { return VertDirection.UP; }
        return VertDirection.DOWN;
    }
}
*/
public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        if ((int)direction < 3) //magic number-ish. only six directions so its okay i guess
        {
            return (direction + 3); // 0->3, 1->4, 2->5
        }
        return (direction - 3); //etc
    }

    public static HexDirection Previous(this HexDirection direction) //counterclockwise
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1); //does what youd expect
    }

    public static HexDirection Next(this HexDirection direction) //clockwise
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1); //has specific check for the final index
    }

    public static Vector3 ToRotation(this HexDirection direction)
    {
        return new Vector3(0, 30 + (int)direction * 60, 0);
    }
}

public static class HexDirection3DExtends
{
    public static HexDirection3D Opposite(this HexDirection3D direction)
    {
        if ((int)direction < 3) //still works, ish
        {
            return (direction + 3); // 0->3, 1->4, 2->5
        }
        if ((int)direction < 6)
        {
            return (direction - 3); //etc
        }
        if (direction == HexDirection3D.UP)
        {
            return HexDirection3D.DOWN;
        }
        return HexDirection3D.UP;
    }

    public static HexDirection3D Previous(this HexDirection3D direction) //counterclockwise
    {
        if ((int)direction < 6)
        {
            return direction == HexDirection3D.NE ? HexDirection3D.NW : (direction - 1); //does what youd expect
        }

        if (direction == HexDirection3D.UP) { return HexDirection3D.DOWN; }
        { return HexDirection3D.UP; }
    }

    public static HexDirection3D Next(this HexDirection3D direction) //clockwise
    {
        if ((int)direction < 6)
        {
            return direction == HexDirection3D.NW ? HexDirection3D.NE : (direction + 1); //has specific check for the final index
        }
        if (direction == HexDirection3D.UP) { return HexDirection3D.DOWN; }
        { return HexDirection3D.UP; }
    }

    public static Vector3 ToRotation(this HexDirection3D direction)
    {
        if ((int)direction < 6)
        {
            return new Vector3(0, 30 + (int)direction * 60, 0);
        }
        return new Vector3(0,0,0);
    }
}