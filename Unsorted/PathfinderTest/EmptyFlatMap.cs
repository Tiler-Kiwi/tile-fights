using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Creates an empty flat map. For testing purposes, presumably.
/// </summary>
public class EmptyFlatMap : MonoBehaviour
{
    public TileListBase TileList;
    public TileType FloorTile;
    public int width;
    public int height;
    public int depth;

    public void Start()
    {
        GenerateMap();
        TileListChunkCoordinator Coordinator = FindObjectOfType<TileListChunkCoordinator>();
        Coordinator.AssignList(TileList);
    }
    public void GenerateMap()
    { //used for pathfinding prototype
        //if (TileList == null)
        { TileList = new TileListBase(width, height, depth); }
        TileList2DControl ReturnedTileArray = new TileList2DControl(TileList);
        System.Random rng = new System.Random((int)DateTime.Now.Ticks);
        double MapCenterX = width / 2; //same off by one error
        double MapCenterY = height / 2;
        double TileCount = width * height;
        double TileCountSqr = Math.Sqrt(TileCount);
        float radius = width / 2;
        if (width > height)
        {
            radius = height / 2; //radius ought not to extend off the map so the small of the two is used here
        }
        for (int i = 0; i < TileCount; i++)
        {
            int xindex = i % width;
            int yindex = i / width;
            if (Mathf.Sqrt(Mathf.Pow(((float)xindex - (float)MapCenterX), 2) + Mathf.Pow(((float)yindex - (float)MapCenterY), 2)) <= radius)
            {
                ReturnedTileArray.AssignFloorType(HexCoordinates.FromOffsetCoordinates(xindex, yindex, 0), FloorTile);
            }
            else
            {
                ReturnedTileArray.AssignFloorType(HexCoordinates.FromOffsetCoordinates(xindex, yindex, 0), null);
            }
        } //map will be ~ 78.5% grass tiles, and a circle
    }
}
