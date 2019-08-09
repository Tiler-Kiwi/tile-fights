using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MovementCondition 
{
    public abstract bool Validate(IEntityMovement movement, IMapCollisionDetection seer, HexCoordinates PathOrigin, EntityMapping entityMap, TileListBase tileMap);
}
