using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[System.Serializable]
public class MC_PathStatus : MovementCondition
{
    public HexDirection3D Direction;
    public SpacePathStatus MininimumPath;

    public override bool Validate(IEntityMovement movement, IMapCollisionDetection seer, HexCoordinates PathOrigin, EntityMapping entityMap, TileListBase tileMap)
    {
        return ((int)MininimumPath >= (int)seer.PathStatus(entityMap, tileMap, PathOrigin, movement, Direction));
    }
}

