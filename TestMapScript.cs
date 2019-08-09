using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMapScript : MonoBehaviour {

    TileListBase BaseList;
    public TileListChunkCoordinator ChunkMaker;
    public OldMapGen Generate;
    public EntityMapping TheGameBoard;

	// Use this for initialization
	void Start ()
    {
        BaseList = new TileListBase(50, 50, 10);
        TileList2DControl BaseList2D = new TileList2DControl(BaseList);
        //OldMapGen Generate = new OldMapGen();
        Generate.GenerateRandomMap(BaseList.XDim, BaseList.ZDim, BaseList.YDim, BaseList2D);

        ChunkMaker.AssignList(BaseList);
        TheGameBoard = Instantiate<EntityMapping>(TheGameBoard);
        //TheGameBoard.GiveTileMap(BaseList);
	}
}
