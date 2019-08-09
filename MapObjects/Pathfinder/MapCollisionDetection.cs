using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Used for pathfinding / validation of entity movement
/// </summary>
public class MapCollisionDetection : MonoBehaviour, IMapCollisionDetection
{

    /// <summary>
    /// Returns a list of locations an entity can reach from their available movement choices (directional movement, leaping, falling)
    /// </summary>
    public List<HexCoordinates> AdjacentMoveSpaces(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement)
    {
        List<HexCoordinates> ReturnVals = new List<HexCoordinates>();
        if(movement == null) { return ReturnVals; }

        //basic directional movement
        foreach(HexCoordinates val in CheckBasicDirections(entityMap, tileMap, coords, movement)) { ReturnVals.Add(val); }

        //default jump
        HexCoordinates CheckCoords = coords;
        for(int i=0;i<movement.ElevationUpMax;i++)
        {
            if(!DirectionMoveClear(entityMap, tileMap, CheckCoords, movement, HexDirection3D.UP)) { break; }
            CheckCoords = CheckCoords.DirectionTransform(HexDirection3D.UP);
            if (!tileMap.ValidCoords(CheckCoords)){ break; }
            if(!CanEnterViaDirection(entityMap, tileMap, CheckCoords, movement, HexDirection3D.DOWN)) { break; }
            foreach (HexCoordinates val in CheckBasicDirections(entityMap, tileMap, CheckCoords, movement)) { ReturnVals.Add(val); }
        }

        //basic fall
        for(int i=0;i<6;i++)
        {
            if(PathStatus(entityMap, tileMap, coords, movement, (HexDirection3D)i) != SpacePathStatus.Clear) { continue; }
            if(NodeStatus(entityMap, tileMap, coords.DirectionTransform((HexDirection3D)i),movement) == SpaceNodeStatus.Blocked) { continue; }
            CheckCoords = coords.DirectionTransform((HexDirection3D)i);
            for(int j=0;j<movement.ElevationDownMax;j++)
            {
                if(PathStatus(entityMap, tileMap, CheckCoords, movement, HexDirection3D.DOWN)==SpacePathStatus.Clear)
                {
                    CheckCoords = CheckCoords.Below;
                    continue;
                }
                SpaceNodeStatus NodeStat = NodeStatus(entityMap, tileMap, CheckCoords, movement);
                if(NodeStat==SpaceNodeStatus.SoftClear || NodeStat == SpaceNodeStatus.HardClear) { ReturnVals.Add(CheckCoords); }
                break;
            }
        }

        //check special things i guess
        List<ExtraPath> EntityPaths = entityMap.ExtraPathsAtLocation(coords);
        for(int i=0; i< EntityPaths.Count; i++)
        {
            if(EntityPaths[i].Validate(movement, coords, entityMap, tileMap, this)) //kind of... dangerous, actually. Hrm.\

            {
                ReturnVals.Add(EntityPaths[i].Destination);
            }
        }
            return ReturnVals;
    }

    public bool CanEnterViaDirection(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement, HexDirection3D dir)
    {
        if (!CanEnterSpace(entityMap, tileMap, coords, movement)) { return false; }

        List<MapEntity> EntitysAtLocation = entityMap.EntitysAtLocation(coords);
        foreach (MapEntity i in EntitysAtLocation)
        {
            EntityMapNodeModifier ETPM = i.GetComponent<EntityMapNodeModifier>();
            if (ETPM != null)
            {
                if (ETPM.BlockingObject) { return false; }
                if (ETPM.BlockedDirections.CompareToNotFlag(dir)) { return false; }
            }
        }

        return true;
    }

    public bool DirectionMoveClear(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement, HexDirection3D dir)
    {
        return CanEnterViaDirection(entityMap, tileMap, coords, movement, dir.Opposite());
    }

    private bool CanEnterSpace(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement)
    {
        if (!tileMap.ValidCoords(coords)) { return false; }
        if (tileMap.GetSolidType(coords) != null) { return false; }
        List<MapEntity> EntitysAtLocation = entityMap.EntitysAtLocation(coords);
        bool EntityFloor = false;
        foreach (MapEntity i in EntitysAtLocation)
        {
            EntityMapNodeModifier ETPM = i.GetComponent<EntityMapNodeModifier>();
            if (ETPM != null)
            {
                if (ETPM.BlockingObject) { return false; }
                if (ETPM.SolidFloor) { EntityFloor = true; }
            }
        }
        if (EntityFloor) { return true; }
        if (tileMap.GetFloorType(coords) == null) { return false; }
        return tileMap.GetFloorType(coords).Solid;
    }

    public bool SpaceBlocked(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement)
    {
        if (!tileMap.ValidCoords(coords)) { return true; }
        SpaceNodeStatus NodeStat = NodeStatus(entityMap, tileMap, coords, movement);
        if (NodeStat != SpaceNodeStatus.SoftClear && NodeStat != SpaceNodeStatus.HardClear) { return true; }
        return false;
    }

    public SpacePathStatus PathStatus(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement, HexDirection3D dir)
    {
        if (!tileMap.ValidCoords(coords)) { return SpacePathStatus.Blocked; }
        if (!DirectionMoveClear(entityMap, tileMap, coords, movement, dir)){ return SpacePathStatus.Blocked; }
        if(!CanEnterViaDirection(entityMap, tileMap, coords.DirectionTransform(dir), movement, dir.Opposite())) { return SpacePathStatus.Blocked; }
        SpaceNodeStatus NodeStat = NodeStatus(entityMap, tileMap, coords, movement);
        if(NodeStat== SpaceNodeStatus.Blocked) { return SpacePathStatus.Blocked; }
        return SpacePathStatus.Clear;
    }

    public SpaceNodeStatus NodeStatus(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement)
    {
        if (tileMap.GetSolidType(coords) != null) { return SpaceNodeStatus.Blocked; } //placeholder, later should ask movement for info

        List<MapEntity> EntitysAtLocation = entityMap.EntitysAtLocation(coords);

        bool EntityFloor = false;
        bool Soft = false;
        foreach (MapEntity i in EntitysAtLocation)
        {
            EntityMapNodeModifier ETPM = i.GetComponent<EntityMapNodeModifier>();
            if (ETPM != null)
            {
                if (ETPM.BlockingObject)
                {
                    if (ETPM.IsEntityBlocked(movement.MyEntity))
                    {
                        return SpaceNodeStatus.Blocked;
                    }
                    if (ETPM.SoloTileObject) { Soft = true; }
                }
                if (ETPM.SolidFloor) { EntityFloor = true; }
            }
        }
        if (EntityFloor) { return Soft?SpaceNodeStatus.SoftClear:SpaceNodeStatus.HardClear; }
        if (tileMap.GetFloorType(coords) == null) { return SpaceNodeStatus.InvalidTerrain; }
        if (tileMap.GetFloorType(coords).Solid == true) { return Soft?SpaceNodeStatus.SoftClear:SpaceNodeStatus.HardClear; }
        return SpaceNodeStatus.InvalidTerrain;
    }

    private List<HexCoordinates> CheckBasicDirections(EntityMapping entityMap, TileListBase tileMap, HexCoordinates coords, IEntityMovement movement)
    {
        List<HexCoordinates> ReturnVals = new List<HexCoordinates>();
        for (int i = 0; i < 6; i++)
        {
            if (!DirectionMoveClear(entityMap, tileMap, coords, movement, (HexDirection3D)i)) { continue; }
            if (!CanEnterViaDirection(entityMap, tileMap, coords.DirectionTransform((HexDirection3D)i), movement, HexDirection3DExtends.Opposite((HexDirection3D)i))) { continue; }
            SpaceNodeStatus NodeStat = NodeStatus(entityMap, tileMap, coords.DirectionTransform((HexDirection3D)i), movement);
            SpacePathStatus PathStat = PathStatus(entityMap, tileMap, coords, movement, (HexDirection3D)i);
            if (PathStat == SpacePathStatus.Blocked) { continue; }
            if (NodeStat != SpaceNodeStatus.SoftClear && NodeStat != SpaceNodeStatus.HardClear) { continue; }
            ReturnVals.Add(coords.DirectionTransform((HexDirection3D)i));
        }
        return ReturnVals;
    }
}

