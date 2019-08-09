using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//catlike coding stuff, actually used.

public static class HexMetrics
{

    public const float outerRadius = 1.5f; //how much space a tile occupies, more or less. radius to corner of hex
    public const float innerRadius = outerRadius * 0.866025404f; // radius when drawn to middle of hexagon side. pythago stuff
    // public const float solidFactor = 0.75f;
    public const float solidFactor = .95f; //used in the CC tutorial for blending. used here to designate how large the gap is between tiles.
    public const float blendFactor = 1f - solidFactor; //the "gap" or space meant to be blended.

    public const float elevationStep = 2f; // height gap between each tile

    public const float floorFactor = .2f * elevationStep; //how tall a floor ought to be on its lonesome
    public const float wallFactor = 1 - floorFactor; // height of a wall when the floor is a different substance? idk


    static Vector3[] corners =
    {

        new Vector3(0f,0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f*outerRadius),
        new Vector3(innerRadius,0f,-.5f*outerRadius),
        new Vector3(0f,0f,-outerRadius),
        new Vector3(-innerRadius,0f,-.5f*outerRadius),
        new Vector3(-innerRadius,0f,0.5f*outerRadius),
        new Vector3(0f,0f,outerRadius)
        
            //defines six corners that constitute a hexagon. pointy bit facing up. commented out code is for other hexagon orientation
            // their indexes coorelate with the directions enum.
            // so if you change it or change this everything breaks 

        /*new Vector3(outerRadius,0f,0f),
        new Vector3(.5f*outerRadius,0f,-innerRadius),
        new Vector3(-.5f*outerRadius,0f,-innerRadius),
        new Vector3(-outerRadius,0f,0f),
        new Vector3(-.5f*outerRadius,0f,innerRadius),
        new Vector3(.5f*outerRadius,0f,innerRadius),
        new Vector3(outerRadius,0f,0f)*/
    };

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction]; //corner as in absolute total distance. not modified by solid factor.
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }

    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return corners[(int)direction + 1] * solidFactor;
    }

    public static Vector3 GetBridge(HexDirection direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor; 
    }

    public static Vector3 TileLocation(Vector3 tileOffsets)
    {
        float offset = HexMetrics.innerRadius * ((int)tileOffsets.z & 1);
        Vector3 ret = new Vector3(
            tileOffsets.x * HexMetrics.innerRadius * 2f + offset,
            tileOffsets.y * HexMetrics.elevationStep,
            -1 * tileOffsets.z * HexMetrics.outerRadius * 1.5f);
        return ret;
    }
}