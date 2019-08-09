using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class MC_SpaceClear : MovementCondition
{
    public HexCoordinates ClearSpace; //localcoords
    public SpaceNodeStatus MinimumClarity;

    public override bool Validate(IEntityMovement movement, IMapCollisionDetection seer, HexCoordinates origin, EntityMapping entityMap, TileListBase tileMap)
    {
        HexCoordinates CheckSpace = ClearSpace + origin;
        return (int)seer.NodeStatus(entityMap, tileMap, CheckSpace, movement) >= (int)MinimumClarity;
    }
}

