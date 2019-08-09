/*
 * Created by SharpDevelop.
 * User: adam.moseman
 * Date: 6/18/2017
 * Time: 5:26 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

/// <summary>
/// draws line between two points and shit like that
/// fuck you
/// </summary>

/*
the above is a good example of why i dont do comments while trying to code stuff
too spicy

in any case, this does NOT draw a line, it draws a path.
path can be a line, its often not technically a line.
used for pathfinding (duh), but also sometimes to just see if two things can reach each other, which is probably
not a particuarlly good use of the function.
*/
public static class MapPathfinder
{
    public static List<HexCoordinates> GetPath(IMapCollisionDetection map, EntityMapping entityMap, TileListBase tileMap, HexCoordinates start, HexCoordinates end, IEntityMovement movement)
    { //tilearray is an array full of tiles, which have cubecoords. start/end are obvious. movementtype influences some of the pathing decisions.
        if (!SanityCheck(map, entityMap, tileMap, start, end, movement)) { return null; }
        List<HexCoordinates> ReturnValue = new List<HexCoordinates>();

        List<HexNode> closedset = new List<HexNode>(); //nodes already examined
        List<HexNode> openset = new List<HexNode>(); //nodes that still can be examined

        /*
        having actual C# lists, instead of just having some kind of linkedlist setup may be a poor choice
        */

        HexNode thing = new HexNode(start, end, null); //location, goal, parent node. starting node has no parent.

        /*
        H is a herustic that is part of determining which node to check first.
        in this case, distance is a distance formula for xyz 
        since the vast majority of time, the closer two points in "real" space, the less nodes exist between them
        this is obviously not always the case (walls exist), but in the majority of cases, it holds true.

        score is H + G, lowest score gets picked from the opennode list
        G value is 1 + parent node's G.
        */
        thing.H = HexCoordinates.Distance(start, end);
        openset.Add(thing);

        while (openset.Count > 0)
        {
            HexNode current = openset[0];
            for (int i = 1; i < openset.Count; i++)
            {
                if (openset[i].Score < current.Score)
                {
                    current = openset[i];
                }
            }

            /*
            The above could, and maybe ought to, be replaced by keeping the openset list sorted after every
            addition, and just popping openset[0].
            I think doing 1-to-n amount of compares for each openset addition
            is more costly than just having a constant n each runthrough
            but maybe it works out so that most openset additions wind up near the front so often
            that it might become noticably faster if openset gets particuarlly large
            but I'm not a computer science guy and its a waste of time to care about stuff like this sometimes
            */

            if (current.Coords.X == end.X && current.Coords.Y == end.Y && current.Coords.Z == end.Z && current.Coords.Depth == end.Depth) //we have reached our goal
            {
                return RebuildPath(current);
            }

            openset.Remove(current); //"pop" current
            closedset.Add(current); //

            //get all tile coords that are adjacent to current
            List<HexCoordinates> neighbors = map.AdjacentMoveSpaces(entityMap, tileMap, current.Coords, movement);
            for (int i = 0; i < neighbors.Count; i++)
            {
                bool found = false;
                HexCoordinates NeighborCoords = neighbors[i];
                for (int j = 0; j < closedset.Count; j++)
                {
                    if (closedset[j].Coords.X == NeighborCoords.X &&
                       closedset[j].Coords.Y == NeighborCoords.Y &&
                       closedset[j].Coords.Z == NeighborCoords.Z &&
                       closedset[j].Coords.Depth == NeighborCoords.Depth)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) { continue; } //node was found in closedset, so dont do anything
                for (int j = 0; j < openset.Count; j++)
                {
                    if (openset[j].Coords.X == NeighborCoords.X &&
                       openset[j].Coords.Y == NeighborCoords.Y &&
                       openset[j].Coords.Z == NeighborCoords.Z &&
                       openset[j].Coords.Depth == NeighborCoords.Depth)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) { continue; } //neighbor found in openset

                HexNode NewNode = new HexNode(NeighborCoords, end, current); //neighbor is a new node, add it to openset.
                openset.Add(NewNode);
            }
        }
        return null; // ran out of currentset before goal was reached. no path is possible.
                     //returning null is kind of dangerous, but it communicates the idea in a self-evident manner, i feel.
    }

    internal static List<HexCoordinates> GetAllPathableTiles(EntityMapping entityMap, TileListBase tileMap, HexCoordinates source, IEntityMovement movement, IMapCollisionDetection map)
    {
        List<HexCoordinates> ret = new List<HexCoordinates>();
        IEntityMovement Move = movement;
        int MoveRange = movement.MovementRange;
        List<HexNode> closedset = new List<HexNode>(); //nodes already examined
        List<HexNode> openset = new List<HexNode>(); //nodes that still can be examined

        HexNode thing = new HexNode(source, source, null); 

        thing.H = HexCoordinates.Distance(source, source); //dont really care 'bout H
        openset.Add(thing);

        while (openset.Count > 0)
        {
            HexNode current = openset[0];
            openset.Remove(current);
            closedset.Add(current);
            if (current.G >= MoveRange) { continue; } //can't go any further from this location
            List<HexCoordinates> neighbors = map.AdjacentMoveSpaces(entityMap, tileMap, current.Coords, movement);
            for (int i = 0; i < neighbors.Count; i++)
            {
                bool found = false;
                HexCoordinates NeighborCoords = neighbors[i];
                for (int j = 0; j < closedset.Count; j++)
                {
                    if (closedset[j].Coords == NeighborCoords)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) { continue; } //node was found in closedset, so dont do anything
                for (int j = 0; j < openset.Count; j++)
                {
                    if (openset[j].Coords == NeighborCoords)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) { continue; } //neighbor found in openset, no need to add it again.

                HexNode NewNode = new HexNode(NeighborCoords, source, current); //neighbor is a new node, add it to openset.
                openset.Add(NewNode);
            }
        }
        for(int i=0; i<closedset.Count; i++)
        {
            ret.Add(closedset[i].Coords);
        }
        return ret;
    }

    internal static List<HexCoordinates> GetPathNextTo(EntityMapping entityMap, TileListBase tileMap, IMapCollisionDetection map, HexCoordinates start, HexCoordinates end, IEntityMovement actor)
    {
        List<HexCoordinates> ReturnValue = new List<HexCoordinates>();
        IEntityMovement Move = actor;

        List<HexNode> closedset = new List<HexNode>(); //nodes already examined
        List<HexNode> openset = new List<HexNode>(); //nodes that still can be examined

        HexNode thing = new HexNode(start, end, null); //location, goal, parent node. starting node has no parent.

        thing.H = HexCoordinates.Distance(start, end);
        List<HexCoordinates> AdjacentToEnd = end.GetCoordsWithinDistance(1);
        openset.Add(thing);

        while (openset.Count > 0)
        {
            HexNode current = openset[0];

            if (AdjacentToEnd.Contains(current.Coords)) //we have reached our goal
            {
                return RebuildPath(current);
            }

            openset.Remove(current); //"pop" current
            closedset.Add(current); //

            //get all tile coords that are adjacent to current
            List<HexCoordinates> neighbors = map.AdjacentMoveSpaces(entityMap, tileMap, current.Coords, actor);
            for (int i = 0; i < neighbors.Count; i++)
            {
                bool found = false;
                HexCoordinates NeighborCoords = neighbors[i];
                for (int j = 0; j < closedset.Count; j++)
                {
                    if (closedset[j].Coords == NeighborCoords)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) { continue; } //node was found in closedset, so dont do anything
                for (int j = 0; j < openset.Count; j++)
                {
                    if (openset[j].Coords == NeighborCoords)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) { continue; } //neighbor found in openset, no need to add it again.

                HexNode NewNode = new HexNode(NeighborCoords, end, current); //neighbor is a new node, add it to openset.
                int index = openset.Count;
                for (int j = 0; j < openset.Count; j++)
                {
                    if (NewNode.Score <= openset[j].Score) //newnode has lower or same score, should be inserted before checked node in openset
                    {
                        index = j;
                        break;

                        //doing this sort should improve pathfinding speed...
                    }
                }
                openset.Insert(index, NewNode);
            }
        }
        //UnityEngine.Debug.Log("Invalid path: Could not draw path.");
        return null;
    }
    

    internal static List<HexCoordinates> GetPath(EntityMapping entityMap, TileListBase tileMap, IMapCollisionDetection map, HexCoordinates start, HexCoordinates end)
    {
        IEntityMovement actor = new DefaultEnityMovement();
        return GetPath(map, entityMap, tileMap, start, end, actor);
        /*
        if (!SanityCheck(map, start, end, actor))
        {
            //UnityEngine.Debug.Log("Invalid path: Invalid paramaters.");
            return null;
        }
        List<HexCoordinates> ReturnValue = new List<HexCoordinates>();

        List<HexNode> closedset = new List<HexNode>(); //nodes already examined
        List<HexNode> openset = new List<HexNode>(); //nodes that still can be examined

        HexNode thing = new HexNode(start, end, null); //location, goal, parent node. starting node has no parent.

        thing.H = HexCoordinates.Distance(start, end);
        openset.Add(thing);

        while (openset.Count > 0)
        {
            HexNode current = openset[0];

            if (current.Coords == end) //we have reached our goal
            {
                return RebuildPath(current);
            }

            openset.Remove(current); //"pop" current
            closedset.Add(current); //

            //get all tile coords that are adjacent to current
            List<HexDirection3D> neighbors = map.ValidMoveDirections(current.Coords, actor);
            for (int i = 0; i < neighbors.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < closedset.Count; j++)
                {
                    if (closedset[j].Coords == neighbors[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (found) { continue; } //node was found in closedset, so dont do anything
                for (int j = 0; j < openset.Count; j++)
                {
                    if (openset[j].Coords == neighbors[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (found) { continue; } //neighbor found in openset, no need to add it again.

                HexNode NewNode = new HexNode(neighbors[i], end, current); //neighbor is a new node, add it to openset.
                int index = openset.Count;
                for (int j = 0; j < openset.Count; j++)
                {
                    if (NewNode.Score <= openset[j].Score) //newnode has lower or same score, should be inserted before checked node in openset
                    {
                        index = j;
                        break;

                        //doing this sort should improve pathfinding speed...
                    }
                }
                openset.Insert(index, NewNode);
            }
        }
        //UnityEngine.Debug.Log("Invalid path: Could not draw path.");
        return null;
        */
    }

    private static bool SanityCheck(IMapCollisionDetection map, EntityMapping entityMap, TileListBase tileMap, HexCoordinates start, HexCoordinates end, IEntityMovement movement)
    {
        if(!tileMap.ValidCoords(start) || !tileMap.ValidCoords(end)) { return false; } //one or both coords are not valid, dont path
        if(map.SpaceBlocked(entityMap, tileMap, end, movement)) { return false; } // end location is blocked, cannot path
        return true;
    }

    internal static bool CanPath(IMapCollisionDetection map, EntityMapping entityMap, TileListBase tileMap, IEntityMovement actor, HexCoordinates source, HexCoordinates target)
    {
        return (MapPathfinder.GetPath(map, entityMap, tileMap, source, target, actor) != null); //Slow
    }

    public static List<HexCoordinates> RebuildPath(HexNode current)
        {
            List<HexCoordinates> ReturnValue = new List<HexCoordinates>();
            ReturnValue.Add(current.Coords); //current ought to be the same as goal, here.
            while (current.Parent != null)
            {
                current = current.Parent;
                ReturnValue.Add(current.Coords);
            }

            ReturnValue.Reverse();
            return ReturnValue; 
        }
    }

    public class HexNode
    {
        private HexNode _Parent;
        private double _H;
        private HexCoordinates _Coords;

        public HexNode Parent
        {
            get { return _Parent; }
            set { _Parent = value; }
        }

        public HexCoordinates Coords
        {
            get { return _Coords; }
            set { _Coords = value; }

        }

        public double H
        {
            get
            {
                return _H;
            }
            set
            {
                _H = value;
            }
        }

        public double G //number of steps from node back to source
        {
            get { if (_Parent != null) { return 1 + _Parent.G; } return 0; }
            set { }
        }

        public double Score //estimated distance to go + distance gone
        {
            get { return G + H; }
            set { }
        }

        public HexNode(HexCoordinates location, HexCoordinates goal, HexNode parent = null)
        {
            _H = HexCoordinates.Distance(location, goal); //ought to maybe have a list of herustics it consults instead
            _Parent = parent;
            _Coords = location;
        }


    }