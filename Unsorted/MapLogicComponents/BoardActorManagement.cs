using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Redundent.
/// </summary>
/*
    public class BoardActorManagement : MonoBehaviour
    {

    public EventHandler<ActorSetIntoReserve> ActorSetIntoReserveEvent;
    public EventHandler<ActorReleasedFromReserve> ActorReleasedFromReserveEvent;
    public EventHandler<NewActorOnBoard> NewActorOnBoardEvent;
    public EventHandler<ActorDeletedFromBoard> ActorDeletedFromBoardEvent;
    public EventHandler<ActorRelocated> ActorRelocatedEvent;
    public EventHandler<FactionVisionChanged> FactionVisionChangedEvent;

    private void Start()
    {
        TheMap = FindObjectOfType<EntityMapping>();
    }

    public void OnFactionVisionChanged(FactionVisionChanged e)
    {
        EventHandler<FactionVisionChanged> handler = FactionVisionChangedEvent;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    private Faction GetEntityFaction(MapEntity entity)
    {
        MapEntityFaction mef = entity.GetComponent<MapEntityFaction>();
        Faction Faction = null;
        if (mef != null)
        {
            Faction = mef.FactionObject;
        }
        return Faction;
    }

    private bool TriggerFactionVisionEvent(Faction faction)
    {
        if (faction != null)
        {
            FactionVisionChanged vc = new FactionVisionChanged();
            vc.Faction = faction;
            OnFactionVisionChanged(vc);
            return true;
        }
        return false;
    }
    
    public void OnActorDeletedFromBoard(ActorDeletedFromBoard e)
    {
        EventHandler<ActorDeletedFromBoard> handler = ActorDeletedFromBoardEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        Faction Faction = GetEntityFaction(e.Pawn);
        TriggerFactionVisionEvent(Faction);

    }
    public void OnActorReleasedFromReserve(ActorReleasedFromReserve e)
    {
        EventHandler<ActorReleasedFromReserve> handler = ActorReleasedFromReserveEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        Faction Faction = GetEntityFaction(e.Pawn);
        TriggerFactionVisionEvent(Faction);
    }
    public void OnActorRelocated(ActorRelocated e)
    {
        EventHandler<ActorRelocated> handler = ActorRelocatedEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        Faction Faction = GetEntityFaction(e.Pawn);
        TriggerFactionVisionEvent(Faction);
    }

    public void OnActorSetIntoReserve(ActorSetIntoReserve e)
    {
        EventHandler<ActorSetIntoReserve> handler = ActorSetIntoReserveEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        Faction Faction = GetEntityFaction(e.Pawn);
        TriggerFactionVisionEvent(Faction);
    }
    public void OnNewActorOnBoard(NewActorOnBoard e)
    {
        EventHandler<NewActorOnBoard> handler = NewActorOnBoardEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        Faction Faction = GetEntityFaction(e.Pawn);
        TriggerFactionVisionEvent(Faction);
    }

    public List<HexCoordinates> GetActorVision(MapEntity actor)
    {
        List<HexCoordinates> ret = new List<HexCoordinates>();
        EntityVision EntityEyes = actor.GetComponent<EntityVision>();
        if(EntityEyes == null) { return ret; }
        HexCoordinates? ActorLocation = actor.Location;
        if (!ActorLocation.HasValue) { return ret; }
        ret.Add(ActorLocation.Value);
        List<HexCoordinates> Check = ActorLocation.Value.GetCoordsWithinDistance(EntityEyes.VisionRange, true, true);
        for (int i = 0; i < Check.Count; i++)
        {
            if (!TheMap.ValidCoords(Check[i])) { continue; }
            if (TheMap.GetFloorType(Check[i]) == null) { continue; } //lets not care if you cant see a tile that never has anything on it
            EntityFacing Facing = actor.GetComponent<EntityFacing>();
            if (Facing != null)
            {
                HexDirection DirectionFromActor = ActorLocation.Value.GetFacing(Check[i]);
                if (DirectionFromActor == Facing.FacingDirection.Opposite() || 
                    DirectionFromActor == Facing.FacingDirection.Opposite().Next() || 
                    DirectionFromActor == Facing.FacingDirection.Opposite().Previous())
                { continue; }
            }
            List<HexCoordinates> PathOfVision = HexCoordinates.cube_linedraw(ActorLocation.Value, Check[i]);
            bool SightBlocked = false;
            for (int k = 0; k < PathOfVision.Count - 1; k++)
            {
                TileType tile = TheMap.GetSolidType(PathOfVision[k]);
                if (tile != null && tile.Solid)
                {
                    SightBlocked = true;
                    break;
                }
            }
            if (!SightBlocked) { ret.Add(Check[i]); }
        }
        return ret;
    }

    public List<MapEntity> GetFactionActors(Faction factionEnum)
    {
        List<MapEntity> ret = new List<MapEntity>();
        foreach(MapEntity entity in TheMap.EntityList)
        {
            if (GetEntityFaction(entity) == factionEnum)
            {
                ret.Add(entity);
            }
        }
        return ret;
    }

    public Faction GetActorFaction(MapEntity actor)
    {
        return GetEntityFaction(actor);
    }
    public HexCoordinates? GetActorLocation(MapEntity actor)
    {
        return actor.Location;
    }
    public List<MapEntity> GetActorAt(HexCoordinates source)
    {
        return TheMap.EntitysAtLocation(source);
    }

    public void DamageActor(float damage, HexCoordinates impactedTile, HexDirection impactDirection, bool isProjectile, WeaponDamageType weaponDamageType)
    {
        throw new NotImplementedException("DamageActor not implemented");
        //TheMap.DamageActor(damage, impactedTile, impactDirection, isProjectile, weaponDamageType);
    }
    public void DeleteActor(MapEntity actor)
    {
        ActorDeletedFromBoard e = new ActorDeletedFromBoard();
        e.Pawn = actor; e.SpaceRemovedFrom = GetActorLocation(actor);
        TheMap.UnRegisterEntity(actor);
        actor.Location = null;
        OnActorDeletedFromBoard(e);
    }
    public void MoveActor(HexCoordinates? coords, MapEntity targetActor)
    {
        Tuple<HexCoordinates?, Faction> old = new Tuple<HexCoordinates?, Faction>(targetActor.Location, GetEntityFaction(targetActor));
        if (old.Item1 == coords) { return; } //dont do anything if it isnt actually moving
        bool StartedOffBoard = true;
        bool EndedOffBoard = true;
        if (old.Item1.HasValue) //actor is on board presently
        {
            StartedOffBoard = false;
        }
        if (coords.HasValue)
        {
            EndedOffBoard = false;
        }
        targetActor.Location = coords;
        if (StartedOffBoard)
        {
            ActorReleasedFromReserve e = new ActorReleasedFromReserve();
            e.Pawn = targetActor; e.SpaceAddedTo = coords.Value;
            OnActorReleasedFromReserve(e);
        }
        else if (EndedOffBoard)
        {
            ActorSetIntoReserve e = new ActorSetIntoReserve();
            e.Pawn = targetActor; e.SpaceRemovedFrom = old.Item1.Value;
            OnActorSetIntoReserve(e);
        }
        else
        {
            ActorRelocated e = new ActorRelocated();
            e.Pawn = targetActor; e.SpaceAddedTo = coords.Value; e.SpaceRemovedFrom = old.Item1.Value;
            OnActorRelocated(e);
        }
    }
    public void PlaceAdditionalActor(MapEntity actor, HexCoordinates? hexCoordinates)
    {
        if (hexCoordinates.HasValue)
        {
            TheMap.RegisterEntity(actor);
        }
        NewActorOnBoard e = new NewActorOnBoard();
        e.Pawn = actor; e.SpaceAddedTo = hexCoordinates;
        OnNewActorOnBoard(e);
    }
    public void PushActor(HexCoordinates impactedTile, HexDirection directionalMod, int pushDistance)
    {
        throw new NotImplementedException("can't push things yet");
        //TheMap.PushActor(impactedTile, directionalMod, pushDistance);
    }
    /*
    void HandleActorClick(object obj, ActorClicked e)
    {
        HexCoordinates? ActorCoords = GetActorLocation(e.Target);
        if (!ActorCoords.HasValue) { return; }
        MapBoardChunk Chunk = MyBoard.GetChunkThatOwnsTile(ActorCoords.Value);
        Chunk.ClickedAtCoords(ActorCoords.Value);
    }

    void HandleActorDeath(object obj, ActorDied e)
    {
        MoveActor(null, e.Target);
    }

    void HandleActorVisionChange(object obj, VisionChanged e)
    {
        FactionVisionChanged fvc = new FactionVisionChanged();
        fvc.Board = MyBoard; fvc.Faction = e.Target.Faction;
        OnFactionVisionChanged(fvc);
    }
    */
//}
/*
*/