using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class ExtraPath
{
    public List<MovementCondition> MovementConditions;
    public HexCoordinates Destination; //Use local coordinates

    public bool Validate(IEntityMovement movement, HexCoordinates source, EntityMapping entityMap, TileListBase tileMap, IMapCollisionDetection collide)
    {
        foreach(MovementCondition c in MovementConditions)
        {
            if(c.Validate(movement, collide, source, entityMap, tileMap) == false) { return false; }
        }
        return true;
    }

    public HexCoordinates GlobalDestination(HexCoordinates source)
    {
        return Destination + source;
    }

    //public bool TwoWay; no more two way paths because fuck that, it destroys the purpose to a lot of shit.
}


