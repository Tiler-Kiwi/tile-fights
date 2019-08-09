using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Interface used to designate an object capable of providing data on the state of a map; primarially concerned with entities and their positions, for use in pathfinding / object placement.
/// ITS A COLLISION DETECTOR IM DUMB
/// </summary>
public interface IMapCollisionDetection 
{
    bool SpaceBlocked(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement); //Blocked spaces may not be entered at all.
    SpaceNodeStatus NodeStatus(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement);
    bool CanEnterViaDirection(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement, HexDirection3D dir); //returns true if a space may be entered from a direction
    SpacePathStatus PathStatus(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement, HexDirection3D dir);
    List<HexCoordinates> AdjacentMoveSpaces(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement); //Returns list of directions that an entitys movement rules permit it to move into
}

