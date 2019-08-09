using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple map generator. Creates a tile array, for use in the battle map logic, out of the given height/width values.
/// </summary>

[CreateAssetMenu]
public class OldMapGen : ScriptableObject
{
    [SerializeField]
    TileType Default;
    [SerializeField]
    TileType Water;
    [SerializeField]
    TileType Wall;
    [SerializeField]
    TileType Floor;
    //[SerializeField]
    TileType Void = null;

    [SerializeField]
    MapEntity Goal;
    [SerializeField]
    MapEntity Tree;
    [SerializeField]
    MapEntity Rock;
    [SerializeField]
    MapEntity Bridge;

    [SerializeField]
    TileType Roof;
    [SerializeField]
    MapEntity Ramp;
    [SerializeField]
    MapEntity Ladder;
    [SerializeField]
    MapEntity WallF;
    [SerializeField]
    IEntityMovement Foot;

    public TileList2DControl GenerateRandomMap(int width, int height, int depth, TileList2DControl map)
    {
        TileList2DControl GenerateMap = map;
        System.Random rng = new System.Random((int)DateTime.Now.Ticks);
        double randomnum;
        /*Should be width+1/2. Same with height.*/
        double MapCenterX = width / 2;
        double MapCenterZ = height / 2;
        double TileCount = width * height;
        double TileCountSqr = Math.Sqrt(TileCount);

        for (int z = 0; z < GenerateMap.ZDim; z++) //build the inital map of grass/empty tiles
            for (int x = 0; x < GenerateMap.XDim; x++)
            {
                {
                    //simple distance formula
                    //sqrt( (x1-x2)^2+(y1-y2)^2)
                    double DistToCenter = Math.Sqrt(Math.Pow((x - MapCenterX), 2) + Math.Pow((z - MapCenterZ), 2));

                    //average distance is .38 * sqrt(height*width)
                    //odds are (50-(dist*(100/(sqrt(totaltiles)-1)))) (dont divvy by zero i guess)
                    randomnum = rng.Next(1, (int)TileCountSqr);
                    randomnum -= DistToCenter;
                    if (randomnum >= (TileCountSqr / 2))
                    {
                        GenerateMap.AssignFloorType(HexCoordinates.FromOffsetCoordinates(x, z, 0), Default);
                       
                    }
                    else
                    {
                        GenerateMap.AssignFloorType(HexCoordinates.FromOffsetCoordinates(x, z, 0), null);
                    }
                }
            }

        //4-5 algorithm for map boundries
        //4-5 is "if tile has four A neighbors, turn A. else, if 5 B neighbors, turn B.
        //a copy map is maintained for reference so the prior changes during the same cycle dont affect outcome
        //in this case, modified for use with hex tiles as opposed to pseudo-octogon "squares" (with pathable corners)
        // so now its 2 / 2. Sort of a flavor thing that got fiddled with until it worked, rather than a real engineered decision.
        for (int i = 0; i < 3; i++) //number of times to peform the algo
        {
            TileList2DControl CopyMap = new TileList2DControl(GenerateMap);
            //CopyMap.CreatedTileArray = true; ???

            for (int z = 0; z < GenerateMap.ZDim; z++)
            {
                for (int x = 0; x < GenerateMap.XDim; x++)
                {
                    int dneighbors = 0;
                    int vneighbors = 0;
                    HexCoordinates coords = HexCoordinates.FromOffsetCoordinates(x, z, 0);
                    List<Tile> jneighbors = CopyMap.TileNeighbors(coords, false);
                    for (int k = 0; k < jneighbors.Count; k++)
                    {
                        if (jneighbors[k].floorType == null)
                        {
                            vneighbors++;
                        }
                        else if (jneighbors[k].floorType == Default)
                        {
                            dneighbors++;
                        }
                    }

                    if (dneighbors >= 2)
                    {
                        GenerateMap.AssignFloorType(coords, Default);
                    }
                    else if (vneighbors >= 2)
                    {
                        GenerateMap.AssignFloorType(coords, null);
                    }
                }
            }
        }

        //find all sets of tiles of a type, delete all but the largest of each
        //results in a big land blob with no "void" inside.
        //bool butt = true;

        

        GenerateMap = TrimMapEdges(GenerateMap, null);

        GenerateMap = PurgeIsolatedTiles(GenerateMap, null, Default, 1);

        GenerateMap = PurgeIsolatedTiles(GenerateMap, Default, null, 1);

        RiverGeneration2D(GenerateMap, rng, 1);
        
        //destroy all but largest two sets of land
        GenerateMap = PurgeIsolatedTiles(GenerateMap, Default, null, 2);
        GenerateMap = PurgeIsolatedTiles(GenerateMap, null, Default, 1);

        //add a bridge
        List<List<HexCoordinates>> DefaultSpaces = ListOfHexGroups(GenerateMap, Default);
        if (DefaultSpaces.Count > 1)
        {
            List<HexCoordinates> ZoneARiverBank = new List<HexCoordinates>();
            List<HexCoordinates> ZoneBRiverBank = new List<HexCoordinates>();

            for (int i = 0; i < DefaultSpaces.Count; i++)
            {
                for (int j = 0; j < DefaultSpaces[i].Count; j++)
                {
                    //List<HexCoordinates> CheckForWater = GenerateMap.TileNeighborsIndex(FaultIfNull(GenerateMap.CoordsToIndex(DefaultSpaces[i][j]))); //should be safe.
                    List<HexCoordinates> CheckForWater = DefaultSpaces[i][j].GetCoordsWithinDistance(1);
                    CheckForWater.Remove(DefaultSpaces[i][j]);
                    bool nexttowater = false;
                    for (int k = 0; k < CheckForWater.Count; k++)
                    {

                        if (GenerateMap.GetFloorType(CheckForWater[k]) == Water)
                        {
                            nexttowater = true; //tile is adjacent to at least one water tile
                            break;
                        }
                    }
                    if (nexttowater)
                    {
                        switch (i) //switch based on set of tiles
                        {
                            case 0:
                                ZoneARiverBank.Add(DefaultSpaces[i][j]);
                                break;
                            case 1:
                                ZoneBRiverBank.Add(DefaultSpaces[i][j]);
                                break;
                        }
                    }
                }
            }

            if (ZoneARiverBank.Count > ZoneBRiverBank.Count) //zoneA is smaller
            {
                List<HexCoordinates> temp = ZoneARiverBank;
                ZoneARiverBank = ZoneBRiverBank;
                ZoneBRiverBank = temp;
            }
            if (ZoneARiverBank.Count == 0 || ZoneBRiverBank.Count == 0)
            {
                //I don't know what the fuck this was meant to be doing.
                /*
                int cow = 0;
                List<HexCoordinates> TestNeighbors = new List<HexCoordinates>();
                TileList returnthis = GenerateMap.TrueList;
                TestNeighbors = returnthis.TileNeighborsIndex(cow);
                Debug.Log("Center:" + returnthis.IndexToCoords(cow).ToString());
                Debug.Log("East: " + returnthis.IndexToCoords(cow).DirectionTransform(HexDirection.E, 0).ToString());
                for (int i = 0; i < TestNeighbors.Count; i++)
                {
                    Debug.Log(TestNeighbors[i].ToString() + " " + returnthis.IndexToCoords(TestNeighbors[i]).ToString() + " " + returnthis.IndexToOffsets(TestNeighbors[i]).ToString());
                    returnthis.AssignFloorType(TestNeighbors[i], Wall);
                }
                returnthis.AssignFloorType(cow, Floor);
                return returnthis;
                */
                throw new Exception("There should probably be rivers.");
            }
            HexCoordinates BridgeStart = ZoneARiverBank[rng.Next(0, ZoneARiverBank.Count)]; //pick spot next to river on smaller side
            HexCoordinates BridgeEnd = ZoneBRiverBank[0];
            double bridgedist = HexCoordinates.cube_distance(BridgeStart, BridgeEnd);
            for (int i = 1; i < ZoneBRiverBank.Count; i++)
            {
                if (HexCoordinates.cube_distance(BridgeStart, ZoneBRiverBank[i]) < bridgedist) //get the closest tile from the other bank
                {
                    BridgeEnd = ZoneBRiverBank[i];
                    bridgedist = HexCoordinates.cube_distance(BridgeStart, ZoneBRiverBank[i]);
                }
            }

            List<HexCoordinates> BridgeCoords = HexCoordinates.cube_linedraw(BridgeStart, BridgeEnd); //draw line across water
            for (int i = 1; i < BridgeCoords.Count - 1; i++)
            {
                if (Bridge != null)
                {
                    AddTileFeature(BridgeCoords[i], Bridge);
                }
            }
        }
        GenerateMap = MakeHeightsViaPerlin(GenerateMap, 5, 1, 1, 1);
        randomnum = rng.Next(1, 3);
        
        for (int i = 0; i < randomnum; i++)
        {
            MakeBuildingOnHeightLand(GenerateMap);
        }
        //sprinkle tree clusters
        GenerateMap = ScatterFeature(GenerateMap, 10, 0, 10, 6, 10, Default, Tree, rng);
        //sprinkle rocks
        GenerateMap = ScatterFeature(GenerateMap, 25, 0, 3, 6, 10, Default, Rock, rng);
        List<HexCoordinates> River = this.FindSeperatedRoleGroups(GenerateMap, Water)[0];
        for (int i = 0; i < River.Count; i++)
        {
            GenerateMap.SetTileHeight(River[i], 0);
        }
        List<List<HexCoordinates>> FloodZones = new List<List<HexCoordinates>>();
        FloodZones = SeperateMovementAreas(GenerateMap);
        //then ensure that all areas can be reached via doing node fuckery and some silly feature stuff
        PlaceRampsAndLadders(GenerateMap, FloodZones, Ramp, Ladder);
        List<List<HexCoordinates>> FloorGroups = this.FindSeperatedRoleGroups(GenerateMap, Floor);
        if (FloorGroups.Count > 1)
        {
            List<HexCoordinates> Floors = this.FindSeperatedRoleGroups(GenerateMap, Floor)[0];
            while (true)
            {
                int randomint = rng.Next(0, Floors.Count);
                if (GenerateMap.GetFloorType(Floors[randomint]) == Floor) //add a goal to a floor
                {
                    AddTileFeature(Floors[randomint], Goal);
                    break;
                }
            }
        }
        GenerateMap.AddMapBottom(Default);
        return GenerateMap;
    }

    private void RiverGeneration2D(TileList2DControl tile2D, System.Random rng, int riverCount)
    {
        //make a river

        int Count = riverCount; //number of rivers to generate

        //create a river by picking two random sides (1/2 chance adjacent, 1/2 oppisite side)
        //then path the river down an axis until default is hit
        //then generate additional nodes for the river to path thru and generate a spline 
        //then create the river.
        while (Count > 0)
        {
            List<HexCoordinates> RiverPath = new List<HexCoordinates>();
            bool HitLand = false;
            HexDirection RiverDirection = HexDirection.E;
            HexDirection EndRiverDirection = HexDirection.E;
            int StartRiverX = -1;
            int StartRiverZ = -1;
            int EndRiverX = -1;
            int EndRiverZ = -1;
            int width = tile2D.XDim;
            int height = tile2D.ZDim;
            // 0-Top 1-Right 2-Bottom 3-Left
            int RiverStartSide = rng.Next(0, 4);
            int RiverEndSide = rng.Next(0, 4);
            HexCoordinates RiverHeadLocation;
            HexCoordinates RiverTailLocation;

            if (RiverStartSide == RiverEndSide) //if they end up on the same side, place them oppisite 
            {
                if (RiverStartSide == 1) { RiverStartSide = 3; }
                else if (RiverStartSide == 2) { RiverStartSide = 0; }
                else if (RiverStartSide == 3) { RiverStartSide = 1; }
                else { RiverStartSide = 2; }
            }

            if (RiverStartSide == 0) //top
            {
                StartRiverX = rng.Next(0, width);
                StartRiverZ = 0;
                int direction = rng.Next(0, 1);
                switch (direction)
                {
                    case 0:
                        RiverDirection = HexDirection.SE;
                        break;
                    case 1:
                        RiverDirection = HexDirection.SW;
                        break;
                }
            }
            if (RiverStartSide == 3) //left
            {
                StartRiverX = 0;
                StartRiverZ = rng.Next(0, height);
                int direction = rng.Next(0, 2);
                switch (direction)
                {
                    case 0:
                        RiverDirection = HexDirection.E;
                        break;
                    case 1:
                        RiverDirection = HexDirection.SE;
                        break;
                    case 2:
                        RiverDirection = HexDirection.NE;
                        break;
                }
            }
            if (RiverStartSide == 2) //bottom
            {
                StartRiverX = -rng.Next(0, width);
                StartRiverZ = height - 1;
                int direction = rng.Next(0, 1);
                switch (direction)
                {
                    case 0:
                        RiverDirection = HexDirection.NW;
                        break;
                    case 1:
                        RiverDirection = HexDirection.NE;
                        break;
                }
            }
            if (RiverStartSide == 1) //right
            {
                //extra -1 is a mistake but a largely meaningless one
                StartRiverX = width - 1;
                StartRiverZ = rng.Next(0, height);
                int direction = rng.Next(0, 2);
                switch (direction)
                {
                    case 0:
                        RiverDirection = HexDirection.W;
                        break;
                    case 1:
                        RiverDirection = HexDirection.SW;
                        break;
                    case 2:
                        RiverDirection = HexDirection.NW;
                        break;
                }
            }

            if (RiverEndSide == 0) //top
            {
                EndRiverX = rng.Next(0, width);
                EndRiverZ = 0;
                int direction = rng.Next(0, 1);
                switch (direction)
                {
                    case 0:
                        EndRiverDirection = HexDirection.SW;
                        break;
                    case 1:
                        EndRiverDirection = HexDirection.SE;
                        break;
                }
            }
            if (RiverEndSide == 3) //left
            {
                EndRiverX = 0;
                EndRiverZ = rng.Next(0, height);
                int direction = rng.Next(0, 2);
                switch (direction)
                {
                    case 0:
                        EndRiverDirection = HexDirection.E;
                        break;
                    case 1:
                        EndRiverDirection = HexDirection.SE;
                        break;
                    case 2:
                        EndRiverDirection = HexDirection.SW;
                        break;
                }
            }
            if (RiverEndSide == 2) //bottom
            {
                EndRiverX = rng.Next(0, width);
                EndRiverZ = height - 1;
                int direction = rng.Next(0, 1);
                switch (direction)
                {
                    case 0:
                        EndRiverDirection = HexDirection.NE;
                        break;
                    case 1:
                        EndRiverDirection = HexDirection.NW;
                        break;
                }
            }
            if (RiverEndSide == 1) //right
            {
                EndRiverX = width - 1;
                EndRiverZ = rng.Next(0, height);
                int direction = rng.Next(0, 2);
                switch (direction)
                {
                    case 0:
                        EndRiverDirection = HexDirection.W;
                        break;
                    case 1:
                        EndRiverDirection = HexDirection.SW;
                        break;
                    case 2:
                        EndRiverDirection = HexDirection.NW;
                        break;
                }
            }

            RiverHeadLocation = HexCoordinates.FromOffsetCoordinates(StartRiverX, StartRiverZ, 0);
            RiverTailLocation = HexCoordinates.FromOffsetCoordinates(EndRiverX, EndRiverZ, 0);

            if (tile2D.GetFloorType(RiverHeadLocation) != null)
            {
                HitLand = true;
            }

            while (!HitLand)
            {
                if (!tile2D.ValidDirection(RiverHeadLocation, RiverDirection))
                {
                    break;
                }
                //keep moving in a direction until a not-void tile is found
                RiverHeadLocation = RiverHeadLocation.DirectionTransform((HexDirection3D)(RiverDirection));

                if (tile2D.GetFloorType(RiverHeadLocation) != null)
                {
                    HitLand = true;
                }
            }

            if (HitLand == false)
            {
                bool error = true;
                for (int z = 0; z < tile2D.ZDim; z++)
                {
                    for (int x = 0; x < tile2D.XDim; x++)

                    {
                        if (tile2D.GetFloorType(HexCoordinates.FromOffsetCoordinates(x, z, 0)) != null) { error = false; break; }
                    }
                    if (!error) { break; }
                }
                if (error) { throw new Exception("no land!"); }
                continue; //should be okay for now to let it loop some
                throw new Exception("Failed to find point for start of river");
            }

            HitLand = false;

            if (tile2D.GetFloorType(RiverTailLocation) != null)
            {
                HitLand = true;
            }

            while (!HitLand)
            {
                if (!tile2D.ValidDirection(RiverTailLocation, EndRiverDirection))
                {
                    break;
                }

                RiverTailLocation = RiverTailLocation.DirectionTransform((HexDirection3D)(EndRiverDirection));

                if (tile2D.GetFloorType(RiverTailLocation) != null)
                {
                    HitLand = true;
                }
            }

            if (HitLand == false)
            {
                continue;
                throw new Exception("Failed to find point for end of river");
            }


            //RiverPath = P_MapBuilder.RiverMeander(ReturnedTileArray,RiverStartIndex,RiverEndIndex,RiverDirection);
            int Node1X; //river uses these random land tiles for spline creation
            int Node1Z;
            int Node2X;
            int Node2Z;
            int Node3X;
            int Node3Z;

            while (true) // make sure the three nodes are on land
            {
                Node1X = rng.Next(tile2D.XDim);
                Node1Z = rng.Next(tile2D.ZDim);
                if (tile2D.GetFloorType(HexCoordinates.FromOffsetCoordinates(Node1X, Node1Z, 0)) == Default)
                {
                    break;
                }
            }
            while (true)
            {
                Node2X = rng.Next(tile2D.XDim);
                Node2Z = rng.Next(tile2D.ZDim);
                if (tile2D.GetFloorType(HexCoordinates.FromOffsetCoordinates(Node2X, Node2Z, 0)) == Default)
                {
                    break;
                }
            }
            while (true)
            {
                Node3X = rng.Next(tile2D.XDim);
                Node3Z = rng.Next(tile2D.ZDim);
                if (tile2D.GetFloorType(HexCoordinates.FromOffsetCoordinates(Node3X, Node3Z, 0)) == Default)
                {
                    break;
                }
            }
            List<HexCoordinates> NodeList = new List<HexCoordinates>();
            NodeList.Add(HexCoordinates.FromOffsetCoordinates(Node1X, Node1Z, 0));
            NodeList.Add(HexCoordinates.FromOffsetCoordinates(Node2X, Node2Z, 0));
            NodeList.Add(HexCoordinates.FromOffsetCoordinates(Node3X, Node3Z, 0));

            // RiverStartIndex = 0;
            //RiverEndIndex = ReturnedTileArray.Capacity-1;
            //RiverPath = BresenhamRiver(ReturnedTileArray, RiverStartIndex, RiverEndIndex, RiverDirection);
            RiverPath = SplineRiver(tile2D, NodeList, RiverHeadLocation, RiverTailLocation); //make a river
                                                                                             // RiverPath = P_MapBuilder.RiverMeander(ReturnedTileArray, RiverStartIndex, RiverEndIndex, RiverDirection);

            for (int i = 0; i < RiverPath.Count; i++) //turn river tiles into water
            {
                tile2D.AssignFloorType(RiverPath[i], Water);
            }
            Count--;

            //Debug!
            tile2D.AssignFloorType(RiverHeadLocation, Wall);
            tile2D.AssignFloorType(RiverTailLocation, Roof);
        }
    }

    public void MapBuildTwo(int width, int height, int depth)
    { //used for pathfinding prototype
        width = 25;
        height = 25;
        TileList2DControl ReturnedTileArray = new TileList2DControl(new TileListBase(width, height, depth));
        //ReturnedTileArray.CreateTileArray(width, height); maybe not needed idk...
        System.Random rng = new System.Random((int)DateTime.Now.Ticks);
        double randomnum;
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
                ReturnedTileArray.AssignFloorType(HexCoordinates.FromOffsetCoordinates(xindex, yindex, 0), Default);
            }
            else
            {
                ReturnedTileArray.AssignFloorType(HexCoordinates.FromOffsetCoordinates(xindex, yindex, 0), null);
            }
        } //map will be ~ 78.5% grass tiles, and a circle

        //use perlin noise to generate some terrain heights
        ReturnedTileArray = MakeHeightsViaPerlin(ReturnedTileArray, 3, 1, 1.5f, 0.5f);



        //create building
        //copy pasted from above


        //Ramps And Ladders

        //get all areas that can be pathed to with standard movement (foot)
        List<List<HexCoordinates>> FloodZones = new List<List<HexCoordinates>>();
        FloodZones = SeperateMovementAreas(ReturnedTileArray);
        //then ensure that all areas can be reached via doing node fuckery and some silly feature stuff
        PlaceRampsAndLadders(ReturnedTileArray, FloodZones, Ramp, Ladder);

        MakeBuildingOnHeightLand(ReturnedTileArray);

        /*
        Tile CenterTile = ReturnedTileArray.MyTiles[height / 2 * width + width / 2];
        CenterTile.FeatureAdd(Ladder);
        for(int i=0; i<ReturnedTileArray.MyTiles.Length; i++)
        {
            CenterTile.FeatureList[0].Gluedtiles.Add(ReturnedTileArray.MyTiles[i]);
        }
        */
        //ReturnedTileArray.PutTilesInCorrectSpots();
        //return ReturnedTileArray;
    }

    //function for creating sets of tiles based on same-role flood fill sort of thing 
    private List<List<HexCoordinates>> ListOfHexGroups(TileList2DControl tilearray, TileType role)
    {
        List<List<HexCoordinates>> GroupIndexes = FindSeperatedRoleGroups(tilearray, role);
        List<List<HexCoordinates>> ReturnValue = new List<List<HexCoordinates>>();
        for (int i = 0; i < GroupIndexes.Count; i++)
        {
            List<HexCoordinates> HexSet = new List<HexCoordinates>();
            for (int k = 0; k < GroupIndexes[i].Count; k++)
            {
                HexSet.Add((GroupIndexes[i][k]));
            }
            ReturnValue.Add(HexSet);
        }
        return ReturnValue;
    }

    //distributes features at random on a tilearray, using some parameters to control scatter behaviour.
    private TileList2DControl ScatterFeature(TileList2DControl tilearray, int clustertightness, int clustercountmin, int clustercountmax, int featurecount,
                                       int clustermaxdist, TileType role, MapEntity feature, System.Random rng)
    {
        if(feature == null) { return tilearray; }
        TileList2DControl NewTileArray = tilearray;
        int clustercount = rng.Next(clustercountmin, clustercountmax + 1);
        int featurechance = clustertightness;
        while (clustercount > 0)
        {
            List<HexCoordinates> ValidSpawn = FindClearSpaces(NewTileArray, role, 2);
            int randomint = rng.Next(0, ValidSpawn.Count);
            HexCoordinates featuresource = ValidSpawn[randomint];
            AddTileFeature(featuresource, feature);
            int Count = featurecount - 1;
            int Ring = 0;
            while (Count > 0)
            {
                Ring++;
                if (Ring > clustermaxdist) { break; }
                List<HexCoordinates> possiblefeature = GetRing(featuresource, Ring);
                for (int i = 0; i < possiblefeature.Count; i++)
                {
                    HexCoordinates index = (possiblefeature[i]);
                    if (rng.Next(1, 101) < featurechance && NewTileArray.GetFloorType(index) == role)
                    {
                        AddTileFeature(index, feature);
                        Count--;
                        if (Count == 0) { break; }
                    }

                }
            }
            clustercount--;
        }

        return NewTileArray;
    }

    //gets all tiles that are radius distance away from center
    private List<HexCoordinates> GetRing(HexCoordinates center, int radius)
    {
        List<HexCoordinates> HexList = new List<HexCoordinates>();
        HexCoordinates coord = center;

        for (int i = 0; i < radius; i++)
        {
            coord = coord.DownLeft;
        }
        HexDirection dir = HexDirection.E;
        int count = 0;
        int steps = radius;
        while (count < 6)
        {
            for (int i = 0; i < steps; i++)
            {
                HexList.Add(coord);
                coord = HexCoordinates.StepForward(coord, dir);
            }
            dir = TurnCounterClockwise(dir);
            count++;
        }

        return HexList;
    }

    //find all tiles in a tilearray that are surrounded by their own role, up to rings distance away.
    private List<HexCoordinates> FindClearSpaces(ITileListControl tilearray, TileType role, int rings)
    {
        List<HexCoordinates> ReturnValue = new List<HexCoordinates>();
        if (rings < 0)
        {
            return ReturnValue; //hurrr
        }
        for (int z = 0; z < tilearray.ZDim; z++)
        {
            for (int x = 0; x < tilearray.XDim; x++)
            {
                if (tilearray.GetFloorType(HexCoordinates.FromOffsetCoordinates(x,z,0)) == role)
                {
                    if (rings < 1)
                    {
                        ReturnValue.Add(HexCoordinates.FromOffsetCoordinates(x, z, 0));
                        continue;
                    }
                    List<HexCoordinates> Neighbors = GetNeighborhoodTiles(tilearray, HexCoordinates.FromOffsetCoordinates(x,z,0), rings);
                    bool valid = true;
                    for (int j = 0; j < Neighbors.Count; j++)
                    {
                        if (tilearray.GetFloorType(Neighbors[j]) != role)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (valid)
                    {
                        ReturnValue.Add((HexCoordinates.FromOffsetCoordinates(x, z, 0)));
                    }
                }
            }
        }

        return ReturnValue;
    }


        //generate noise using perlin noise algorithm
        public TileList2DControl MakeHeightsViaPerlin(TileList2DControl map, float frequency, int octaves, float lacunarity, float persistence)
        {
            System.Random PerlinRandomNum = new System.Random((int)System.DateTime.Now.Ticks);
            float RandomX = (float)PerlinRandomNum.Next(map.XDim);
            float RandomY = (float)PerlinRandomNum.Next(map.ZDim);
            /*
            float MapWidthSqr = 1;// map._Width * map._Width;
            float MapHeightSqr = 1;// map._Height * map._Height;
            NoiseMethod method = Noise.methods[(int)NoiseMethodType.Perlin][1];
            float StepX = 1f / map._Width;
            float StepY = 1f / map._Height;
            Debug.Log(RandomX + ", " + RandomY);
            float MinSample = float.MaxValue;
            float MaxSample = 0;
            float AvSample = 0;
            float SampleCount = 0;
            */
            /*
            for (int x = 0; x < map._Width; x++)
            {
                RandomX++;
                if(RandomX>map._Width)
                {
                    RandomX = 0;
                }
                for (int y = 0; y < map._Height; y++)
                {
                    RandomY++;
                    if (RandomY > map._Height)
                    {
                        RandomY = 0;
                    }
                    Vector3 point = new Vector3((RandomX*StepX), (RandomY*StepY), 0);
                    float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
                   // sample = Mathf.Pow(sample, 5.0f);
                    float splicydicy = (.5f + 0.5f * sample) * 100;
                    map.MyTiles[x + y * map._Width].TileHeight = (byte)(splicydicy);
                    sample = .5f + 0.5f * sample;
                    //if(sample < MinSample){ MinSample = sample; }
                    //if (sample > MaxSample) { MaxSample = sample; }
                    SampleCount++;
                    AvSample += sample;
                }
            }
            */
            Vector3 point00 = new Vector3(-0.5f, -0.5f);
            Vector3 point10 = new Vector3(0.5f, -0.5f);
            Vector3 point01 = new Vector3(-0.5f, 0.5f);
            Vector3 point11 = new Vector3(0.5f, 0.5f);

            NoiseMethod method = Noise.methods[(int)NoiseMethodType.Perlin][2];
            float stepSizeX = 1f / map.XDim;
            float stepSizeY = 1f / map.ZDim;
            for (int y = 0; y < map.ZDim; y++)
            {
                RandomY++;
                if (RandomY > map.ZDim)
                {
                    RandomY = 0;
                }
                Vector3 point0 = Vector3.Lerp(point00, point01, (RandomY + 0.5f) * stepSizeY);
                Vector3 point1 = Vector3.Lerp(point10, point11, (RandomY + 0.5f) * stepSizeY);
                for (int x = 0; x < map.XDim; x++)
                {
                    RandomX++;
                    if (RandomX > map.XDim)
                    {
                        RandomX = 0;
                    }
                    Vector3 point = Vector3.Lerp(point0, point1, (RandomX + 0.5f) * stepSizeX);
                    float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
                    sample = sample * 0.5f + 0.5f;
                    sample = sample * 10;
                    if (sample <= 5.5) { sample = 1; }
                    else if (sample <= 6.5) { sample = 2; }
                    else { sample = 3; }
                    //Debug.Log(sample + ", " + (byte)sample);
                    //map.IndexToCoords(x + y * map.XDim).TileHeight = (int)sample; //(x, y, coloring.Evaluate(sample));
                    map.SetTileHeight(x,y, (int)sample-1);
                }
            }
            /*
        AvSample = AvSample / SampleCount;
            Debug.Log(map._Width + ", " + map._Height);
            Debug.Log(MinSample);
            Debug.Log(MaxSample);
            Debug.Log(SampleCount);
            Debug.Log(AvSample);
            */
            return map;
        }

        //
        private List<HexCoordinates> GetNeighborhoodTiles(ITileListControl tilearray, HexCoordinates point, int rings)
        {
        return point.GetCoordsWithinDistance(rings);
        }

        private static TileType GetTileTypeWithRole(List<TileType> tiletypelist, TileType role)
        {
            for (int i = 0; i < tiletypelist.Count; i++)
            {
                if (tiletypelist[i] == role)
                {
                    return tiletypelist[i];
                }
            }
            throw new Exception(); // Role requested does not exist!!!
        }

    /// <summary>
    /// "Purges" a tile array, starting with the smallest set of connecting tiles.
    /// </summary>
    private TileList2DControl PurgeIsolatedTiles(TileList2DControl tilearray, TileType toPurge, TileType changeInto, int maxremaininggroups)
    {
        TileList2DControl PurgedTileArray = tilearray;
        List<List<HexCoordinates>> ConnectedSets = FindSeperatedRoleGroups(tilearray, toPurge); //get all seperate sets of tiles that are to be purged
        if(ConnectedSets.Count <= maxremaininggroups) { return PurgedTileArray; }
        int Count = maxremaininggroups;
        while (Count>0) //Select the largest groups to be spared.
        {
            List<HexCoordinates> LongestList = ConnectedSets[0]; 
            for (int i = 0; i < ConnectedSets.Count; i++)
            {
                if (ConnectedSets[i].Count > LongestList.Count)
                {
                    LongestList = ConnectedSets[i];
                }
            }

            ConnectedSets.Remove(LongestList);
            Count--;
        }

        for (int i = 0; i < ConnectedSets.Count; i++)
        {
            for (int j = 0; j < ConnectedSets[i].Count; j++)
            {

                PurgedTileArray.AssignFloorType(ConnectedSets[i][j],changeInto);
            }
        }

        return PurgedTileArray;
    }

    private List<List<HexCoordinates>> FindSeperatedRoleGroups(TileList2DControl tilearray, TileType role)
    {
        List<List<HexCoordinates>> ReturnValue = new List<List<HexCoordinates>>();
        List<HexCoordinates> ToCheck = new List<HexCoordinates>();
        for (int z = 0; z < tilearray.ZDim; z++)
        {
            for (int x = 0; x < tilearray.XDim; x++)
            { ToCheck.Add(HexCoordinates.FromOffsetCoordinates(x,z,0)); } //must ensure every tile is examined
        }
        while (ToCheck.Count > 0)
        {
            if (tilearray.GetFloorType(ToCheck[0]) != role) //dont examine a tile thats the wrong type
            {
                ToCheck.RemoveAt(0);
                continue;
            }
            List<HexCoordinates> ConnectedSet = new List<HexCoordinates>(); //set of tiles that are connected directly to each other
            ConnectedSet.Add(ToCheck[0]); //add check itself; even a single tile is a "group" if alone
            List<HexCoordinates> Neighbors = ToCheck[0].GetCoordsWithinDistance(1); //get neighbors of check
            Neighbors.Remove(ToCheck[0]);
            ToCheck.RemoveAt(0); // we dont need this in ToCheck anymore.
            while (Neighbors.Count > 0) //need to continue iterating until theres no neighbors left to check
            {
                HexCoordinates CheckTile = Neighbors[0]; //consult first neighbor
                if (tilearray.IsTileNull(Neighbors[0])) { Neighbors.RemoveAt(0); continue; }
                if (!ToCheck.Contains(Neighbors[0])) //this tile has been checked previously.
                {
                    Neighbors.RemoveAt(0);
                    continue;
                }
                ToCheck.Remove(Neighbors[0]); // Only two outcomes possible of this neighboring tile; either its the correct role and gets added, or its not the correct role and it never needs cheking.
                if (tilearray.GetFloorType(CheckTile) == role) //neighbor still needs checked, and is of the correct role
                {
                    ConnectedSet.Add(CheckTile); //check tile is added to the group
                    List<HexCoordinates> CheckNeighbors = CheckTile.GetCoordsWithinDistance(1); //get neighbors of the added tile
                    CheckNeighbors.Remove(CheckTile);
                    CheckNeighbors.Remove(Neighbors[0]); //no sense adding the thing that added you...
                    for (int k = 0; k < CheckNeighbors.Count; k++)
                    {
                        Neighbors.Add(CheckNeighbors[k]);
                    }

                }
                Neighbors.RemoveAt(0); //whatever the outcome, neighbor has been checked
            }
            ReturnValue.Add(ConnectedSet);
        }
        return ReturnValue;
    }

    private bool IsRiverBroken(ITileListControl tilearray, List<HexCoordinates> coords)
    {
        //bool brokenriver = false;
        for (int i = 0; i < coords.Count; i++)
        {
            //if(!brokenriver)
            //{
            if (tilearray.GetFloorType(coords[i]) == null && tilearray.GetSolidType(coords[i])==null)
            {
                //brokenriver=true;
                //substart = coords[i-1];
                return true;
            }
            //}
            //else
            //{
            //	if(tilearray.MyTiles[tilearray.CoordsToIndex(coords[i])].tileType != TileType.null)
            //	{
            //		subend = coords[i];
            //		sanity = true;
            //		break;
            //	}
            //}
        }

        return false;
    }
    private void MakeBuildingOnHeightLand(TileList2DControl tileArray)
    {
        TileList2DControl tilearray = tileArray as TileList2DControl;
        if(tilearray == null) { return; }
        System.Random rng = new System.Random();
        List<HexCoordinates> ValidSpawn = FindClearSpaces(tilearray, Default, 2);
        List<HexCoordinates> ValiderSpawn = new List<HexCoordinates>();
        List<HexCoordinates> ValidishSpawn = new List<HexCoordinates>();
        for (int i = 0; i < ValidSpawn.Count; i++)
        {
            if (ValidSpawn[i].Depth == 2)
            {
                ValiderSpawn.Add(ValidSpawn[i]);
            }
            if (ValidSpawn[i].Depth > 0)
            {
                ValidishSpawn.Add(ValidSpawn[i]);
            }
        }
        if (ValidishSpawn.Count > 0)
        {
            ValidSpawn = ValidishSpawn;
        }
        if (ValiderSpawn.Count>0)
        {
            ValidSpawn = ValiderSpawn;
        }
        if (ValidSpawn.Count == 0)
        {
            //return;
            throw new Exception("Can't spawn the fucking building");
        }

        int BuildingSource = rng.Next(0, ValidSpawn.Count);
        tilearray.AssignFloorType(ValidSpawn[BuildingSource], Floor);

        List<HexCoordinates> PossibleBuildingTiles = GetNeighborhoodTiles(tilearray, ValidSpawn[BuildingSource], 1);
        PossibleBuildingTiles.Remove(ValidSpawn[BuildingSource]);
        List<HexCoordinates> BuildingTiles = new List<HexCoordinates>();
        BuildingTiles.Add(ValidSpawn[BuildingSource]);
        int BuildingSize = 1;
        while (BuildingSize < 6)
        {
            int Nomination = rng.Next(0, PossibleBuildingTiles.Count);
            List<HexCoordinates> NomNeighbors = PossibleBuildingTiles[Nomination].GetCoordsWithinDistance(1, false, false) ;
            NomNeighbors.Remove(PossibleBuildingTiles[Nomination]);
            bool valid = false;
            for (int i = 0; i < NomNeighbors.Count; i++)
            {
                if (tilearray.GetFloorType(NomNeighbors[i]) == Floor)
                {
                    valid = true;
                    break;
                }
            }

            if (valid)
            {
                tilearray.AssignFloorType(PossibleBuildingTiles[Nomination], Floor);
                tilearray.SetTileHeight(PossibleBuildingTiles[Nomination], 2);
                BuildingTiles.Add(PossibleBuildingTiles[Nomination]);
                PossibleBuildingTiles.RemoveAt(Nomination);
                BuildingSize++;
            }
        }
        List<HexCoordinates> FloorNeighbors = new List<HexCoordinates>();
        for (int i = 0; i < BuildingTiles.Count; i++)
        {
            //FloorNeighbors.AddRange(tilearray.TileNeighborsIndex(BuildingTiles[i])); // ??? Forgot what this is doing.
            List<HexCoordinates> MaybeNeighbors = BuildingTiles[i].GetCoordsWithinDistance(1,false,false);
            MaybeNeighbors.Remove(BuildingTiles[i]);
            for(int k=0;k<MaybeNeighbors.Count;k++)
            {
                if (!tilearray.IsTileNull(MaybeNeighbors[i]))
                {
                    FloorNeighbors.Add(MaybeNeighbors[k]);
                }
            }
        }

        List<HexCoordinates> Walls = new List<HexCoordinates>();
        for (int i = 0; i < FloorNeighbors.Count; i++)
        {
            if (tilearray.GetFloorType(FloorNeighbors[i]) != Floor && tilearray.GetFloorType(FloorNeighbors[i]) != Wall)
            {
                tilearray.AssignFloorType(FloorNeighbors[i], Floor); //walls are now just floors with a feature that gets added later
                tilearray.SetTileHeight(FloorNeighbors[i], 2);
                Walls.Add(FloorNeighbors[i]);
            }
        }

        /*New method*/
        //creates an array of tiles and kind of hangs onto it for a bit
        //P_TileShape FloorShape = P_TileShape.DeepCopyARegion(ReturnedTileArray, FloorNeighbors[0].MyTileIndex); actually this sucks dont fuckin do it
        //FloorShape.ChangeMyTiles(Roof, true, 5);

        List<HexCoordinates> RoofCoords = new List<HexCoordinates>();
        for (int i = 0; i < FloorNeighbors.Count; i++)
        {
            HexCoordinates FloorCoords = FloorNeighbors[i];
            FloorCoords = FloorCoords.Above;
            RoofCoords.Add(FloorCoords);
        }

        for (int i = 0; i < Walls.Count; i++)
        {
            tilearray.AssignFloorType(Walls[i], Wall); //wait i guess we are making the walls walls now
        }
        int randomint;
        randomint = rng.Next(0, Walls.Count);
        int doorway = randomint;
        tilearray.AssignFloorType(Walls[randomint], Floor); //except this one since its a door
        tilearray.SetTileHeight(Walls[randomint], 2);
        //Time to create the roof I guess!

        for (int i = 0; i < RoofCoords.Count; i++)
        {
            tilearray.AssignFloorType(RoofCoords[i], Roof);
        }

        /*ladder to place to access roof with*/
        bool ladderplaced = true;
        while (ladderplaced == false)
        {
            randomint = rng.Next(0, Walls.Count);
            List<HexCoordinates> Neigh = Walls[randomint].GetCoordsWithinDistance(1);
            Neigh.Remove(Walls[randomint]);
            for (int i = 0; i < Neigh.Count; i++) //neighbors
            {
                if (tilearray.GetFloorType(Neigh[i]) == Default) //neighbor to wallhaving tile is grass
                {
                    for (int k = 0; k < RoofCoords.Count; k++)
                    {
                        if (RoofCoords[k].Below == Walls[randomint])
                        {
                            HexCoordinates RoofTileCoords = RoofCoords[k];
                            AddTileFeature(Neigh[i], Ladder); //put a ladder on the grass
                            GlueTiles(Neigh[i], RoofTileCoords); //allow pathing between ladders

                           AddTileFeature(RoofTileCoords, Ladder); ///add ladder on roof "on top of" the wall tile
                            GlueTiles(RoofTileCoords, Neigh[i]);
                            HideFirstTileFeature(RoofTileCoords); //dont need "extra" ladder graphic on top the roof
                            ladderplaced = true;
                            break;
                            /*
                            interestingly, this is a sort of two way portal between the tile array here, and an
                            entirely seperate tile array. while they get merged further on,
                            it does make me wonder if maintaining multiple arrays of tiles
                            would work out at all. as long as the logic for the individual actors
                            is not tracked by the array and the battlemap keeps track of it, it should be alright.
                            didnt get to test the idea, however.
                            */
                        }
                    }
                }
                if (ladderplaced) { break; }
            }
        }

        for (int i = 0; i < Walls.Count; i++)
        {
            HexCoordinates WallTileCoords = (Walls[i]);
            if (tilearray.GetFloorType(WallTileCoords) != Wall) { continue; } //accounting for the wall that is now a floor
            tilearray.AssignFloorType(WallTileCoords, Floor); //change it back to floors again
            AddTileFeature(WallTileCoords, WallF); //add a wall feature
        }
        for (int i = 0; i < Walls.Count; i++)
        {
            if (i == doorway) { continue; } //accounting for the doorway, again. sheesh.
            MapEntity WallFeature = GetFirstTileFeature(Walls[i]) as MapEntity;
            if (WallFeature == null)
            {
                //return;
                //throw new System.Exception("asdj;lasdff"); //mostly just sanity checking. for the program or myself, idk.
            }
            //FeatureWallScript WallScript = WallFeature.GetComponent<FeatureWallScript>();
            List<HexCoordinates> WallNeighbors = Walls[i].GetCoordsWithinDistance(1);
            WallNeighbors.Remove(Walls[i]);
            for(int k=0;k<WallNeighbors.Count;k++)
            {
                if(tilearray.GetTile(WallNeighbors[k]).floorType != tilearray.GetTile((Walls[i])).floorType)
                {
                    HexDirection dir = (Walls[i]).GetFacing(WallNeighbors[k]);
                    //WallScript.BlockDir(dir);
                }

            }
            //WallScript.GenerateGraphic(); //create the wall graphic
        }
    }

    private MapEntity GetFirstTileFeature(HexCoordinates hexCoordinates)
    {
        return null;
    }

    private void HideFirstTileFeature(HexCoordinates roofTileCoords)
    {
        throw new NotImplementedException();
    }

    private void GlueTiles(HexCoordinates hexCoordinates, HexCoordinates roofTileCoords)
    {
        throw new NotImplementedException();
    }

    private List<HexCoordinates> RiverMeander(ITileListControl tilearray, HexCoordinates riverindex, HexCoordinates rivergoal, HexDirection direction)
    {
        List<HexCoordinates> ReturnTiles = new List<HexCoordinates>();
        //List<HexCubeCoords> coords = PathfindRiver(tilearray, tilearray.IndexToCoords(riverindex), tilearray.IndexToCoords(rivergoal));
        List<HexCoordinates> coords = DumbRiverDraw(tilearray, riverindex, rivergoal);

        for (int i = 0; i < coords.Count; i++)
        {
            ReturnTiles.Add(coords[i]);
            List<HexCoordinates> Drench = coords[i].GetCoordsWithinDistance(1, false, false);
            Drench.Remove(coords[i]);
            for (int w = 0; w < Drench.Count; w++)
            {
                if (tilearray.GetFloorType(Drench[w]) != null && tilearray.GetFloorType(Drench[w]) != Water && !coords.Contains(Drench[w]))
                {
                    ReturnTiles.Add(Drench[w]);
                    break;
                }
            }
        }
        return ReturnTiles;
    }

        private List<HexCoordinates> BresenhamRiver(TileList2DControl tilearray, HexCoordinates riverindex, HexCoordinates rivergoal, HexDirection direction)
        {
            List<HexCoordinates> BresLine = HexCoordinates.cube_linedraw(riverindex,rivergoal);
            //List<int> BresLine = BresenhamLine(0,0, tilearray.Width-1, tilearray.MyTiles.Length/tilearray.Width-1, tilearray.Width);
            List<HexCoordinates> ReturnTiles = new List<HexCoordinates>();
            for (int i = 0; i < BresLine.Count; i++)
            {
                if (tilearray.IsTileNull(BresLine[i])) //may not work?
                {
                    continue;
                }

                //if (tilearray.GetTile(BresLine[i]).tileType == null) { continue; } now redundent
                ReturnTiles.Add(BresLine[i]);
            }

            return ReturnTiles;
        }

        private List<HexCoordinates> SplineRiver(TileList2DControl tilearray, List<HexCoordinates> indexnodes, HexCoordinates startCoords, HexCoordinates endCoords)
        {
            List<OffsetNode> Path = new List<OffsetNode>();
            OffsetNode EndNode = new OffsetNode(endCoords);
            Path.Add(new OffsetNode(startCoords));
            List<OffsetNode> Nodes = new List<OffsetNode>();
            for (int i = 0; i < indexnodes.Count; i++)
            {
                Nodes.Add(new OffsetNode(indexnodes[i]));
            }
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].GiveNodes(Nodes);
                Debug.Log(i + " given nodes list. Nodes.Count is " + Nodes.Count);
            }
            Path[0].GiveNodes(Nodes);
            while (Path.Count < indexnodes.Count + 2)
            {
                Debug.Log("Length of Path: " + Path.Count);

                if (Path.Count + 1 == indexnodes.Count + 2)
                {
                    Debug.Log(Path.Count + 1 + " == " + (indexnodes.Count + 2));
                    Path.Add(EndNode);
                    Debug.Log("Path May Be Complete");
                    for (int i = 0; i < Path.Count - 3; i++)
                    {
                        if (OffsetNode.DetectIntersection(Path[i], Path[i + 1], Path[Path.Count - 2], Path[Path.Count - 1]))
                        {
                            Debug.Log("Intersection detected between segements (" + Path[i]._X + "," + Path[i]._Z + ")(" + Path[i + 1]._X + "," + Path[i + 1]._Z + ") and ("
                                + Path[Path.Count - 2]._X + ", " + Path[Path.Count - 2]._Z + ")(" + Path[Path.Count - 1]._X + ", " + Path[Path.Count - 1]._Z + ")");
                            Nodes.Add(Path[Path.Count - 1]);
                            Path.RemoveAt(Path.Count - 1);
                        }
                    }
                    Debug.Log("Path Is Complete.");
                    break;
                }
                if (Path.Count == 0)
                {
                    throw new Exception("Awww fuck this shit mang I can't deal with this");
                }

                if (!Path[Path.Count - 1].CanIPath(Nodes))
                {
                    Debug.Log("Last node in path has no valid nodes to path towards");
                    Path[Path.Count - 1].GiveNodes(Nodes);
                    Nodes.Add(Path[Path.Count - 1]);
                    Path.RemoveAt(Path.Count - 1);
                    continue;
                }
                Debug.Log("Getting closest valid node from last node in path");
                Path.Add(Path[Path.Count - 1].PopClosestNode(Nodes));
                Debug.Log("Removing new last node from Nodes");
                Nodes.Remove(Path[Path.Count - 1]);
                for (int i = 0; i < Path.Count - 3; i++)
                {
                    if (OffsetNode.DetectIntersection(Path[i], Path[i + 1], Path[Path.Count - 2], Path[Path.Count - 1]))
                    {
                        Debug.Log("Intersection detected between segements (" + Path[i]._X + "," + Path[i]._Z + ")(" + Path[i + 1]._X + "," + Path[i + 1]._Z + ") and ("
                            + Path[Path.Count - 2]._X + ", " + Path[Path.Count - 2]._Z + ")(" + Path[Path.Count - 1]._X + ", " + Path[Path.Count - 1]._Z + ")");
                        Nodes.Add(Path[Path.Count - 1]);
                        Path.RemoveAt(Path.Count - 1);
                    }
                }
            }

            //placeholder until splines are sorted

            List<HexCoordinates> ReturnTiles = new List<HexCoordinates>();
            List<HexCoordinates> TileNodes = new List<HexCoordinates>();

            for (int i = 0; i < Path.Count; i++)
            {
                HexCoordinates HexNode = HexCoordinates.FromOffsetCoordinates(Path[i]._X,Path[i]._Z,0);
                TileNodes.Add(HexNode);
            }

            TileNodes = HexCoordinates.cube_crude_brezspline_draw(TileNodes);

            for (int i = 0; i < TileNodes.Count; i++)
            {
                ReturnTiles.Add(TileNodes[i]);
            }

            return ReturnTiles;
        }

        private HexDirection TurnClockwise(HexDirection facing)
        {
            return facing.Next();
        }

        private HexDirection TurnCounterClockwise(HexDirection facing)
        {
            return facing.Previous();
        }

        private static List<int> BresenhamLine(int x, int y, int x2, int y2, int width)
        {
            List<int> ReturnMe = new List<int>();
            int XDiff = x2 - x;
            int YDiff = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (XDiff < 0) dx1 = -1; else if (XDiff > 0) dx1 = 1;
            if (YDiff < 0) dy1 = -1; else if (YDiff > 0) dy1 = 1;
            if (XDiff < 0) dx2 = -1; else if (XDiff > 0) dx2 = 1;
            int longest = Math.Abs(XDiff);
            int shortest = Math.Abs(YDiff);
            if (!(longest > shortest))
            {
                longest = Math.Abs(YDiff);
                shortest = Math.Abs(XDiff);
                if (YDiff < 0) dy2 = -1; else if (YDiff > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                ReturnMe.Add(x + y * width);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
            return ReturnMe;
        }

        private TileList2DControl TrimMapEdges(TileList2DControl tilearray, TileType type)
        {
        TileList2DControl ReturnMe = tilearray;
            for (int z = 0; z < ReturnMe.ZDim; z++) 
            {
                for (int x = 0; x < ReturnMe.XDim; x++)
            {
                    if (z == 0 || z == (ReturnMe.Count-1) / tilearray.XDim )
                    {
                        tilearray.AssignFloorType(HexCoordinates.FromOffsetCoordinates(x,z,0),type);
                        continue;
                    }
                    if (x == 0 || x == tilearray.XDim - 1)
                    {
                        tilearray.AssignFloorType(HexCoordinates.FromOffsetCoordinates(x, z, 0), type);
                    }
                }
            }
            return ReturnMe;
        }

        private List<HexCoordinates> DumbRiverDraw(ITileListControl tilearray, HexCoordinates riverCoords, HexCoordinates goalCoords)
        {
            List<HexCoordinates> coords = HexCoordinates.cube_linedraw(riverCoords, goalCoords);
            if (!IsRiverBroken(tilearray, coords))
            {
                return coords;
            }

            HexCoordinates RiverStartHex = riverCoords;
            HexCoordinates RiverEndHex = goalCoords;

            HexCoordinates NewPoint = ClosestSharedVisable(tilearray, RiverStartHex, RiverEndHex);

            if (NewPoint.X == -1)
            {
                return PathfindRiver(tilearray, RiverStartHex, RiverEndHex);
            }

            HexCoordinates startnplerp = HexCoordinates.cube_lerp(RiverStartHex, NewPoint, .75);
            HexCoordinates endnplerp = HexCoordinates.cube_lerp(NewPoint, RiverEndHex, .25);

            coords = HexCoordinates.cube_linedraw(RiverStartHex, startnplerp);
            coords.AddRange(DumbRiverDraw(tilearray,startnplerp, endnplerp));
            coords.AddRange(HexCoordinates.cube_linedraw(endnplerp, RiverEndHex));

            return coords;
        }

    private List<HexCoordinates> StupidRiverDraw(ITileListControl tilearray, HexCoordinates riverCoords, HexCoordinates riverGoal)
    {
        List<HexCoordinates> coords = HexCoordinates.cube_linedraw(riverCoords, riverGoal);
        //bool brokenriver = false;
        HexCoordinates substart = new HexCoordinates(-1, -1, -1);
        HexCoordinates subend = new HexCoordinates(-1, -1, -1);

        if (!IsRiverBroken(tilearray, coords))
        {
            //Console.WriteLine(riverindex + " " + rivergoal + "UNBROKEN RIVER DRAWN");
            return coords;
        }

        for (int i = 0; i < coords.Count; i++)
        {
            if (tilearray.GetFloorType(coords[i]) == null)
            {
                substart = coords[i - 1];
                break;
            }
        }

        for (int i = coords.Count - 1; i > 0; i--)
        {
            if (tilearray.GetFloorType(coords[i]) == null)
            {
                subend = coords[i - 1];
                break;
            }
        }

        List<HexCoordinates> ReturnValue = new List<HexCoordinates>();
        HexCoordinates midpoint = new HexCoordinates((substart.X + subend.X) / 2, (substart.Y + subend.Y) / 2, (substart.Z + subend.Z) / 2);
        Vector3 midpointOffsets = HexCoordinates.ToOffsetCoordinates(midpoint);
        int midpointX = (int)midpointOffsets.x;
        int midpointZ = (int)midpointOffsets.z;

        bool onefound = false;
        bool twofound = false;

        Vector3 substartOffsets = HexCoordinates.ToOffsetCoordinates(substart);
        int substartX = (int)substartOffsets.x;
        int substartZ = (int)substartOffsets.z;
        int perpX = -1 * (substartZ - midpointZ);
        int perpY = (substartX - midpointX);

        double perpangle = Math.Atan2(perpY, perpX);

        //int relperpX = midpointX + perpX;
        //int relperpY = midpointY + perpY;

        HexCoordinates newpoint = new HexCoordinates();

        bool checkone = true;
        bool checktwo = true;
        int magnitude = 0;
        while (checkone)
        {
            magnitude++;
            double XCheck = Math.Round(midpointX + (Math.Cos(perpangle) * magnitude));
            double YCheck = Math.Round(midpointZ + (Math.Sin(perpangle) * magnitude));
            HexCoordinates indexcheck = HexCoordinates.FromOffsetCoordinates((int)XCheck, (int)YCheck, 0);

            if (tilearray.IsTileNull(indexcheck))
            {
                checkone = false;
            }

            else if (tilearray.GetFloorType(indexcheck) != null)
            {
                List<HexCoordinates> line1 = HexCoordinates.cube_linedraw(riverCoords, indexcheck);
                List<HexCoordinates> line2 = HexCoordinates.cube_linedraw(indexcheck, riverGoal);
                if (!IsRiverBroken(tilearray, line1) && !IsRiverBroken(tilearray, line2))
                {
                    newpoint = indexcheck;
                    checkone = false;
                }
            }
        }

        if (onefound) { checktwo = false; }
        perpangle = perpangle + Math.PI;
        magnitude = 0;

        while (checktwo)
        {
            magnitude++;
            double XCheck = Math.Round(midpointX + (Math.Cos(perpangle) * magnitude));
            double YCheck = Math.Round(midpointZ + (Math.Sin(perpangle) * magnitude));
            HexCoordinates indexcheck = HexCoordinates.FromOffsetCoordinates((int)XCheck, (int)YCheck, 0);

            if (tilearray.IsTileNull(indexcheck))
            {
                checktwo = false;
            }

            else if (tilearray.GetFloorType(indexcheck) != null)
            {
                List<HexCoordinates> line1 = HexCoordinates.cube_linedraw(riverCoords, indexcheck);
                List<HexCoordinates> line2 = HexCoordinates.cube_linedraw(indexcheck, riverGoal);
                if (!IsRiverBroken(tilearray, line1) && !IsRiverBroken(tilearray, line2))
                {
                    newpoint = indexcheck;
                    twofound = true;
                    checktwo = false;
                }
            }
        }

        if (!onefound && !twofound)
        {
            return coords;
            throw new Exception("FUCK UH THATS NOT SUPPOSED TO HAPPEN AT ALL");
        }

        if (newpoint == riverCoords || newpoint == riverGoal)
        {
            return coords;
        }
        //Console.WriteLine("X:"+riverindex%tilearray.Width + " Y:" + riverindex/tilearray.Width + " | X:" + newpoint%tilearray.Width + " Y:" + newpoint/tilearray.Width + " | X:" + rivergoal%tilearray.Width + " Y:" + rivergoal/tilearray.Width);
        ReturnValue.AddRange(StupidRiverDraw(tilearray, riverCoords, newpoint));
        ReturnValue.AddRange(StupidRiverDraw(tilearray, newpoint, riverGoal));
        //Console.WriteLine(riverindex + " " + newpoint + " " + rivergoal + "DRAWN SUCCESFULLY!");
        return ReturnValue;
    }

    private static List<HexCoordinates> BruteForceVisability(ITileListControl tilearray, HexCoordinates location)
    {
        List<HexCoordinates> ReturnValue = new List<HexCoordinates>();
        for (int z = 0; z < tilearray.ZDim; z++)
        {
            for (int x = 0; x < tilearray.XDim; x++)
            {
                if (tilearray.IsBruteVisable(location, HexCoordinates.FromOffsetCoordinates(x, z, 0))) { ReturnValue.Add(HexCoordinates.FromOffsetCoordinates(x, z, 0)); }
            }
        }

        return ReturnValue;
    }

        private static HexCoordinates ClosestSharedVisable(ITileListControl tilearray, HexCoordinates HexA, HexCoordinates HexB)
        {
            List<HexCoordinates> VisFromA = BruteForceVisability(tilearray, HexA);
            List<HexCoordinates> VisFromB = BruteForceVisability(tilearray, HexB);

            if (VisFromA.Count == 0 || VisFromB.Count == 0)
            {
                return new HexCoordinates(-1, -1, -1);
            }

            List<HexCoordinates> ShorterList = VisFromA;
            List<HexCoordinates> LongerList = VisFromB;
            if (VisFromA.Count > VisFromB.Count)
            {
                ShorterList = VisFromB;
                LongerList = VisFromA;
            }

            HexCoordinates ReturnValue = new HexCoordinates(-1, -1, -1);

            for (int i = 0; i < ShorterList.Count; i++)
            {
                if (LongerList.Contains(ShorterList[i]))
                {
                    if (ReturnValue.X == -1)
                    {
                        ReturnValue = ShorterList[i];
                    }
                    else if (HexCoordinates.cube_distance(ShorterList[i], HexA) + HexCoordinates.cube_distance(ShorterList[i], HexB)
                            < HexCoordinates.cube_distance(ReturnValue, HexA) + HexCoordinates.cube_distance(ReturnValue, HexB))
                    {
                        ReturnValue = ShorterList[i];
                    }
                }
            }

            return ReturnValue;
        }

        private List<HexCoordinates> PathfindRiver(ITileListControl tilearray, HexCoordinates start, HexCoordinates end)
        {
            List<HexCoordinates> OpenList = new List<HexCoordinates>();
            OpenList.Add(start);
            List<HexCoordinates> ClosedList = new List<HexCoordinates>();
            HexCoordinates CameFrom = start;
            double gScore = double.MaxValue;
            //List<HexCubeCoords> ReturnList = new List<HexCubeCoords>();

            while (OpenList.Count != 0)
            {
                double bestscore = double.MaxValue;
            HexCoordinates? addthis = null;
                HexCoordinates check = OpenList[OpenList.Count - 1];
                List<HexCoordinates> neighbors = check.GetCoordsWithinDistance(1,false,false);
            neighbors.Remove(check);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    if (OpenList.Contains(neighbors[i]) ||
                       ClosedList.Contains(neighbors[i]))
                    {
                        continue;
                    }
                    if (tilearray.GetFloorType(neighbors[i]) == null)
                    {
                        continue;
                    }

                    gScore = OpenList.Count + HexCoordinates.cube_distance(end, neighbors[i]);
                    if (gScore < bestscore)
                    {
                        bestscore = gScore;
                        addthis = neighbors[i];
                    }

                }

                if (addthis.HasValue == false)
                {
                    ClosedList.Add(OpenList[OpenList.Count - 1]);
                    OpenList.RemoveAt(OpenList.Count - 1);
                    if (OpenList.Count == 0)
                    {
                        return OpenList;
                    }
                }

                else
                {
                    OpenList.Add(addthis.Value);
                    if (OpenList[OpenList.Count - 1].X == end.X && OpenList[OpenList.Count - 1].Y == end.Y && OpenList[OpenList.Count - 1].Z == end.Z)
                    {
                        return OpenList;
                    }
                }
            }
            return OpenList;
        }

    public List<List<HexCoordinates>> SeperateMovementAreas(ITileListControl tilearray)
    {
        List<List<HexCoordinates>> ReturnValue = new List<List<HexCoordinates>>();

        List<HexCoordinates> ToCheck = new List<HexCoordinates>();
        for (int z = 0; z < tilearray.ZDim; z++)
        {
            for (int x = 0; x < tilearray.XDim; x++)
            {
                ToCheck.Add(HexCoordinates.FromOffsetCoordinates(x,z,0));
            }
        }
        while (ToCheck.Count > 0)
        {
            HexCoordinates Sanityish = ToCheck[0];
            if (tilearray.GetFloorType(ToCheck[0]) == null)
            {
                ToCheck.RemoveAt(0);
                continue;
            }
            List<HexCoordinates> ConnectedTiles = new List<HexCoordinates>();
            List<HexCoordinates> ConnectedTileCheck = new List<HexCoordinates>();
            ConnectedTileCheck.Add(ToCheck[0]);
            while (ConnectedTileCheck.Count > 0)
            {
                List<HexCoordinates> CTCNeighbors = ConnectedTileCheck[0].GetCoordsWithinDistance(1);
                CTCNeighbors.Remove(ConnectedTileCheck[0]);
                for (int i = 0; i < CTCNeighbors.Count; i++) // for each neighbor
                {
                    if (!CanPathTo(CTCNeighbors[i], ConnectedTileCheck[0], Foot, tilearray) || //if i cant path to neighbor
                        ConnectedTileCheck[0].Depth != CTCNeighbors[i].Depth) //neighbor wrong height
                    {
                        continue; //dont add
                    }
                    if (ConnectedTileCheck.Contains(CTCNeighbors[i])) //already going to be checked
                    {
                        continue; //dont add
                    }
                    if (ConnectedTiles.Contains(CTCNeighbors[i])) //already marked as connected
                    {
                        continue; // dont add
                    }
                    ConnectedTileCheck.Add(CTCNeighbors[i]); //add if new and can path; will check its neighbors later
                }
                //if(ConnectedTiles.Count>0 && tilearray.IndexToCoords(ConnectedTileCheck[0]).Depth != tilearray.IndexToCoords(ConnectedTiles[0]).Depth) { throw new Exception("Fuck!"); }
                ConnectedTiles.Add(ConnectedTileCheck[0]); //add connected tile to set
                ToCheck.Remove(ConnectedTileCheck[0]); //dont check this tile again
                ConnectedTileCheck.RemoveAt(0); //pop the connected check set
            }
            ReturnValue.Add(ConnectedTiles); //add set to set of sets
            if (ToCheck.Count > 0) //still more to check
            {
                if (Sanityish == ToCheck[0]) //ToCheck never got removed somehow
                {
                    ToCheck.RemoveAt(0); //rectify that
                }
            }
        }
        return ReturnValue;
    }

        public void PlaceRampsAndLadders(ITileListControl tilearray, List<List<HexCoordinates>> SeperatedMovementAreas, MapEntity ramp, MapEntity ladder)
        {
            System.Random RNG = new System.Random((int)DateTime.Now.Ticks);

            while (SeperatedMovementAreas.Count > 1)
            {
                int SMARandomIndex = RNG.Next(SeperatedMovementAreas.Count);
                List<HexCoordinates> ZoneEdge = new List<HexCoordinates>();
            for (int i = 0; i < SeperatedMovementAreas[SMARandomIndex].Count; i++)
            {
                if (!tilearray.HasSolidFloor(SeperatedMovementAreas[SMARandomIndex][0])) { break; }
                //if (tilearray.GetTile(SeperatedMovementAreas[SMARandomIndex][i]).Features.Count > 0) { continue; } Gonna have to come up with something else here...
                List<HexCoordinates> Neighbors = SeperatedMovementAreas[SMARandomIndex][i].GetCoordsWithinDistance(1);
                Neighbors.Remove(SeperatedMovementAreas[SMARandomIndex][i]);
                

                for (int k = 0; k < Neighbors.Count; k++)
                {

                        if (!CanPathTo(Neighbors[k], Neighbors[k], Foot, tilearray))
                        //tilearray.GetTile(Neighbors[k]).Features.Count > 0) //implicit impassable neighbor
                    {
                        continue;
                    }
                    
                    if (SeperatedMovementAreas[SMARandomIndex].Contains(Neighbors[k])) //neighbor is in set being checked
                    {
 
                        continue;
                    }
                    if (SeperatedMovementAreas[SMARandomIndex][i].Depth == Neighbors[k].Depth) //heights arent different
                    {
                       //Debug.Log("Fuck");
                        continue;
                    }
                    //else { Debug.Log("Arse"); }
                    ZoneEdge.Add(SeperatedMovementAreas[SMARandomIndex][i]); //nonset neighbor is next to set member
                    break;
                }
            }
                if (ZoneEdge.Count == 0)
                {
                //Debug.Log("Movement zone has no edges");

                    SeperatedMovementAreas.RemoveAt(SMARandomIndex);
                    continue;
                }
                bool ConnectedToNeighbor = false;
                while (ConnectedToNeighbor != true)
                {
                    if (ZoneEdge.Count == 0)
                    {
                        break;
                    }
                    int ZoneRandomIndex = RNG.Next(ZoneEdge.Count);
                    List<HexCoordinates> Neighbors = ZoneEdge[ZoneRandomIndex].GetCoordsWithinDistance(1);
                Neighbors.Remove(ZoneEdge[ZoneRandomIndex]);
                    for (int i = 0; i < Neighbors.Count; i++)
                    {
                    
                        if (!CanPathTo(Neighbors[i], Neighbors[i], Foot, tilearray)) //innate impassable neighbor
                        {
                            continue;
                        }
                        
                        if (SeperatedMovementAreas[SMARandomIndex].Contains(Neighbors[i])) //neighbor is in set
                        {
                            continue;
                        }
                        int NeighborZoneIndex = int.MaxValue; //neighbor is not in the set

                        for (int k = 0; k < SeperatedMovementAreas.Count; k++)
                        {
                            if (SeperatedMovementAreas[k].Contains(Neighbors[i]))
                            {
                                NeighborZoneIndex = k;
                                break;
                            }
                        }

                        if (NeighborZoneIndex == int.MaxValue)
                        {
                            //Neighbor is from an area deemed unreachable
                            continue;
                        }
                    //Debug.Log("Seeking to place a ramp");
                    //Debug.Log("NeighborHeight:" + tilearray.IndexToCoords(Neighbors[i]).Depth.ToString() + " MyHeight: " + tilearray.IndexToCoords(ZoneEdge[ZoneRandomIndex]).Depth.ToString());
                        if (Neighbors[i].Depth + 1 == ZoneEdge[ZoneRandomIndex].Depth)
                        {
                       // Debug.Log("Neighbor is lower");
                            AddTileFeature(Neighbors[i],ramp);
                            if (!CanPathTo(ZoneEdge[ZoneRandomIndex], Neighbors[i], Foot, tilearray))
                            {
                               // tilearray.DeleteTileFeature(Neighbors[i],ramp);
                               // continue;
                            }
                            for (int k = 0; k < SeperatedMovementAreas[SMARandomIndex].Count; k++)
                            {
                                SeperatedMovementAreas[NeighborZoneIndex].Add(SeperatedMovementAreas[SMARandomIndex][k]);
                            }
                            ConnectedToNeighbor = true;
                            //Debug.Log("Made a ramp at " + Neighbors[i].ToString());
                            break;
                        }
                        if (Neighbors[i].Depth - 1 == ZoneEdge[ZoneRandomIndex].Depth)
                        {
                        //Debug.Log("Neighbor is higher");
                        AddTileFeature(ZoneEdge[ZoneRandomIndex],ramp);
                            if (!CanPathTo(Neighbors[i],ZoneEdge[ZoneRandomIndex], Foot, tilearray))
                            {
                               // tilearray.DeleteTileFeature(Neighbors[i],ramp);
                               // continue;
                            }
                            for (int k = 0; k < SeperatedMovementAreas[SMARandomIndex].Count; k++)
                            {
                                SeperatedMovementAreas[NeighborZoneIndex].Add(SeperatedMovementAreas[SMARandomIndex][k]);
                            }
                            ConnectedToNeighbor = true;
                           // Debug.Log("Made a ramp at " + ZoneEdge[ZoneRandomIndex].ToString());
                            break;
                        }
                    }
                    ZoneEdge.RemoveAt(ZoneRandomIndex);
                }
                SeperatedMovementAreas.RemoveAt(SMARandomIndex);
            }
        }

    private bool CanPathTo(HexCoordinates hexCoordinates1, HexCoordinates hexCoordinates2, IEntityMovement foot, ITileListControl map)
    {
        var HeighDiff = hexCoordinates1.Depth - hexCoordinates2.Depth;
        if (HeighDiff > 0 && HeighDiff > foot.ElevationUpMax) { return false; }
        else if (HeighDiff < 0 && HeighDiff * -1 > foot.ElevationDownMax) { return false; }

        if(map.HasSolidFloor(hexCoordinates2) && map.GetSolidType(hexCoordinates2) == null) { return true; }
        return false;
    }

    private void AddTileFeature(HexCoordinates hexCoordinates, MapEntity ramp)
    {
        MapEntity NewEntity = Instantiate<MapEntity>(ramp);
        NewEntity.Location = hexCoordinates;
        NewEntity.TeleportToMyLocation();
    }

    private int FaultIfNull(int? value)
    {
        if (value.HasValue) { return value.Value; }
        throw new Exception("Value was null.");
    }
}

public class OffsetNode
{
    public int _X;
    public int _Z;
    //private int _ArrayWidth;
    private List<OffsetNode> _ValidNodes;
    //public int z;

    public OffsetNode(int x, int z)
    {
        _X = x;
        _Z = z;
        _ValidNodes = new List<OffsetNode>();
    }

    public OffsetNode(HexCoordinates coords)
    {
        Vector3 Offsets = HexCoordinates.ToOffsetCoordinates(coords);
        _X = (int)Offsets.x;
        _Z = (int)Offsets.z;
        _ValidNodes = new List<OffsetNode>();
    }

    public void GiveNodes(List<OffsetNode> nodes)
    {
        List<OffsetNode> checkus = new List<OffsetNode>();
        for (int i = 0; i < nodes.Count; i++)
        {
            checkus.Add(nodes[i]);
        }

        for (int i = 0; i < _ValidNodes.Count; i++)
        {
            if (!checkus.Contains(_ValidNodes[i]))
            {
                checkus.Add(_ValidNodes[i]);
            }
        }

        if (checkus.Contains(this))
        {
            checkus.Remove(this);
        }

        _ValidNodes = new List<OffsetNode>();

        while (_ValidNodes.Count < checkus.Count)
        {
            OffsetNode closest = this;
            double closestdist = double.MaxValue;
            for (int i = 0; i < checkus.Count; i++)
            {
                if (!_ValidNodes.Contains(checkus[i]))
                {
                    double dist = Math.Sqrt(Math.Pow((this._X - checkus[i]._X), 2) + Math.Pow((this._Z - checkus[i]._Z), 2));
                    if (dist < closestdist)
                    {
                        closest = checkus[i];
                        closestdist = dist;
                    }
                }
            }
            _ValidNodes.Add(closest);
        }
    }

    public void GiveNode(OffsetNode newnode)
    {
        if (_ValidNodes.Contains(newnode))
        {
            return;
        }
        double dist = Math.Sqrt(Math.Pow((this._X - newnode._X), 2) + Math.Pow((this._Z - newnode._Z), 2));
        for (int i = 0; i < _ValidNodes.Count; i++)
        {
            if (dist < Math.Sqrt(Math.Pow((this._X - _ValidNodes[i]._X), 2) + Math.Pow((this._Z - _ValidNodes[i]._Z), 2)))
            {
                _ValidNodes.Insert(i, newnode);
                return;
            }
        }
        _ValidNodes.Add(newnode);
    }

    public OffsetNode PopClosestNode(List<OffsetNode> check)
    {
        for (int i = 0; i < _ValidNodes.Count; i++)
        {
            if (check.Contains(_ValidNodes[i]))
            {
                OffsetNode thing = _ValidNodes[0];
                _ValidNodes.RemoveAt(0);
                return thing;
            }
        }
        //Vector2[] ReturnValue = new Vector2[2];
        //ReturnValue[0] = new Vector2(this.x, this.y);
        //ReturnValue[1] = new Vector2(thing.x, thing.y);

        throw new Exception("Check if ValidNodes has what youre looking for first, please!!!");
    }

    public bool CanIPath(List<OffsetNode> check)
    {
        for (int i = 0; i < _ValidNodes.Count; i++)
        {
            if (check.Contains(_ValidNodes[i]))
            {
                return true;
            }
        }
        return false;
    }

    public static bool DetectIntersection(OffsetNode lineA1, OffsetNode lineA2, OffsetNode lineB1, OffsetNode lineB2)
    {
        double p0_x = lineA1._X;
        double p0_y = lineA1._Z;
        double p1_x = lineA2._X;
        double p1_y = lineA2._Z;

        double p2_x = lineB1._X;
        double p2_y = lineB1._Z;
        double p3_x = lineB2._X;
        double p3_y = lineB2._Z;

        //https://gist.github.com/Joncom/e8e8d18ebe7fe55c3894

        double s1_x, s1_y, s2_x, s2_y;

        s1_x = p1_x - p0_x; // s1 = Slope 1?
        s1_y = p1_y - p0_y; // slope formula is (y2-y1)/(x2-x1)
        s2_x = p3_x - p2_x; // s2 = slope 2?
        s2_y = p3_y - p2_y; // abs(s2_y) = distance between line B, point 1's y coords, and B2's y coords 

        double s, t;
        s = (s1_y * (p0_x - p2_x) - s1_x * (p0_y - p2_y)) / (s2_x * s1_y - s1_x * s2_y);
        t = (s2_y * (p0_x - p2_x) - s2_x * (p0_y - p2_y)) / (s2_x * s1_y - s1_x * s2_y);

        if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
        {
            // Collision detected
            return true;
        }

        return false; // No collision
    }
}
