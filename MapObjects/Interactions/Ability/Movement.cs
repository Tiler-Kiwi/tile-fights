using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Movement/Default")]
public class Movement : Ability
{
    public Movement()
    {
        AbilityName = "Movement";
        IsMovement = true;
        BasicAbility = true;
    }
    void MoveAction(HexCoordinates Start, HexCoordinates Target, MapEntity actor)
    {
        throw new NotImplementedException();
       // SourceMap.PathActor(Start, Target, actor);
    }

    public override AbilityEffect GetAbilityEffect(HexCoordinates source, HexCoordinates target)
    {
        AbilityEffect ret = new AbilityEffect();
        SpecialEffect KludgeMove = new SpecialEffect(source, source.GetFacing(target), WeaponDamageType.UNASSIGNED);
        HexCoordinates KludgeSource = source;
        HexCoordinates KludgeTarget = target;
        EntityAbilityUser AA = AbilityUser as EntityAbilityUser;
        if (!AA) { return ret; }
        MapEntity PathingActor = AA.GetComponent<MapEntity>();
        KludgeMove.KludgeAction = () => MoveAction(source, target, PathingActor);
        ret.AddDamage(KludgeMove);
        return ret;
    }

    public override List<HexCoordinates> GetTargetSpaces(HexCoordinates source, HexDirection dir, EntityMapping entityMap, TileListBase tileMap, IMapCollisionDetection collide)
    {
        EntityAbilityUser AAUser = AbilityUser as EntityAbilityUser;
        if (AAUser)
        {
            List<HexCoordinates> ret = MapPathfinder.GetAllPathableTiles(entityMap, tileMap, source, AAUser.GetComponent<IEntityMovement>(), collide); //probably not good
            return ret;
        }
        else return new List<HexCoordinates>();
    }
}

