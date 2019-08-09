using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to issue commands to a MapBoard
/// 
/// </summary>
public class BattleMap : MonoBehaviour
{
    /*
    IMapSeer MyMapSeer;
    GameBoard MyGameBoard;

    private List<AbilityEffect> EffectQueue = new List<AbilityEffect>();

    private bool Waiting = false;
    [SerializeField]
    private int _mapWidth = 0;

    [SerializeField]
    private int _mapHeight = 0;

    internal List<MapEntity> GetFactionActors(Faction factionEnum)
    {
        List<MapEntity> ret = new List<MapEntity>();
        foreach(MapEntity entity in MyBoard.EntityList)
        {
            MapEntityFaction mef = entity.GetComponent<MapEntityFaction>();
            if (mef !=null)
            {
                if(mef.FactionObject == factionEnum)
                {
                    ret.Add(entity);
                }
            }
        }
        return ret;
    }

    [SerializeField]
    private int _mapDepth = 0;

    public float TileSpacing = 100;

 //   public MapBoard MapBoardPrefab;

    internal List<MapEntity> GetFeaturesAtCoords(HexCoordinates value)
    {
        return MyBoard.GetFeaturesAtCoords(value);
    }

    private void Update()
    {
        if (Waiting) { return; }
        PopEffectQueue();
    }
    /// <summary>
    /// Like "IsBlocked" but ignores actors and features, but also excludes completely empty spaces
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    internal bool MayTarget(HexCoordinates source, HexCoordinates target)
    {
        if(!MyBoard.ValidCoords(target) ||
            MyBoard.GetSolidType(target) != null ||
            MyBoard.GetFloorType(target) == null ||
            MyBoard.TileHasWall(source, source.GetFacing(target)) ||
            MyBoard.TileHasWall(target, target.GetFacing(source)))
        {
            return false;
        }
        return true;
    }

    internal int GetCurrentTurn()
    {
        return MyBoard.GetCurrentTurn();
    }

   // public TilePositionTracker TileTracker;

    internal Faction GetActiveFaction()
    {
        return MyBoard.GetActiveFaction();
    }

    public int mapWidth { get { return _mapWidth; } set { } }
    public int mapHeight { get { return _mapHeight; } set { } }
    public int mapDepth { get { return _mapDepth; } set { } }

    public GameBoard MyBoard;

    public void Awake()
    {
        MyBoard = FindObjectOfType<GameBoard>();
        //GenerateEmptyMap();
    }

    public void Start()
    {
        //tileArray.SetTileTransforms();
    }

    public void GenerateEmptyMap()
    {
        throw new NotImplementedException();
    }

    internal TileType GetSolidType(HexCoordinates coords)
    {
        return MyBoard.GetSolidType(coords);
    }

    internal TileType GetFloorType(HexCoordinates coords)
    {
        return MyBoard.GetFloorType(coords);
    }

    internal Actor GetActorAt(HexCoordinates source)
    {
        return MyBoard.GetActorAt(source);
    }

    public HexCoordinates? GetActorLocation(Actor actor)
    {
        return MyBoard.GetActorLocation(actor);
    }

    public void TeleportActor(HexCoordinates coords, Actor targetActor)
    {
        MyBoard.MoveActor(coords, targetActor);
    }

    internal bool CanPath(HexCoordinates source, HexCoordinates target, Actor actor)
    {
        return MapPathfinder.CanPath(this, actor, source, target);
    }

    /// <summary>
    /// Removes actor from all tiles, but does not remove from the board
    /// </summary>
    /// <param name="targetActor"></param>
    public void HideActor(Actor targetActor)
    {
        MyBoard.MoveActor(null, targetActor);
    }

    public void DeleteActor(Actor actor)
    {
        MyBoard.DeleteActor(actor);
    }

    internal bool TileBlocked(HexCoordinates value)
    {
        return MyBoard.TileBlocked(value);
    }

    internal bool TileHasWall(HexCoordinates value, HexDirection dir)
    {
        return MyBoard.TileHasWall(value, dir);
    }

    public void PathActor(HexCoordinates sourceTile, HexCoordinates destTile, Actor targetActor)
    {
        Waiting = true;
        StartCoroutine(SmoothActorPathing(targetActor, MapPathfinder.GetPath(this, sourceTile, destTile, targetActor)));
        //TeleportActor(destTile, targetActor);
        targetActor.MovementUsed = true;
    }

    public void ChangeTileType(Tile targetTile, TileType tileType)
    {
        targetTile.AssignSolidType(tileType);
    }

    internal bool IsAccesibleNeighbor(HexCoordinates source, HexCoordinates target)
    {
        HexDirection DirectionFromSource = source.GetFacing(target);
        return MyBoard.IsAccesibleNeighbor(source, target, DirectionFromSource);
    }

    internal List<HexCoordinates> ValidNeighbors(HexCoordinates coords)
    {
        List<HexCoordinates> ret = coords.GetCoordsWithinDistance(1, true, true);
        ret.Remove(coords); //dont want to check self, only looking for neighbors
        List<HexCoordinates> ret2 = new List<HexCoordinates>();
        for(int i=0;i<ret.Count;i++)
        {
            if(ValidCoords(ret[i]))
            {
                ret2.Add(ret[i]);
            }
        }
        return ret2;
    }

    public void ExecuteAbility(Ability ability, HexCoordinates targetCoords) //have map board queue / handle this later
    {
        HexCoordinates? Source = ability.AbilityUser.AbilitySourceHex;
        if (Source.HasValue)
        {
            //ability.SourceMap = this;
            ExecuteAbilityEffect(ability.GetAbilityEffect(Source.Value, targetCoords));
        }
        ActorAbility AAUser = ability.AbilityUser as ActorAbility;
        if (AAUser)
        {
            if (ability.IsMovement) { AAUser.Actor.MovementUsed = true; }
            else { AAUser.Actor.ActionUsed = true; };
        }
        ability.TimesUsed++;
    }

    public void ExecuteAbilityEffect(AbilityEffect effect) //have map board queue / handle this later
    {
        EffectQueue.Add(effect);
    }

    private void PopEffectQueue()
    {
        if (EffectQueue.Count == 0) { return; }
        AbilityEffect effect = EffectQueue[0];
        EffectQueue.RemoveAt(0);

        for(int i=0;i<effect.Count;i++)
        {
            SpaceDamage dam = effect.GetDamage(i);
            Debug.Log(dam.ToString());
            if (dam.ChangeFacing)
            {
                Actor actor = MyBoard.GetActorAt(dam.ImpactedTile);
                if (actor)
                {
                    MyBoard.GetActorAt(dam.ImpactedTile).Facing = dam.DirectionalMod;
                }
            }
            if(dam.PushDistance!=0)
            {
                MyBoard.PushActor(dam.ImpactedTile, dam.DirectionalMod, dam.PushDistance);
            }
            if (dam.Damage != 0)
            {
                MyBoard.DamageActor(dam.Damage, dam.ImpactedTile, dam.ImpactDirection, dam.IsProjectile, dam.WeaponDamageType);
            }
            if (dam._IsSpecial)
            {
                SpecialEffect kludge = dam as SpecialEffect;
                if(kludge!= null)
                {
                    kludge.KludgeAction();
                }
            }
            if(dam.RemoveModifier)
            {
                Actor Target = GetActorAt(dam.ImpactedTile);
                if (Target)
                {
                    IConstantHolder TargetAA = Target.GetComponent<IConstantHolder>();
                    if (TargetAA != null)
                    { TargetAA.RemoveConstantMod(dam.RemovedMod); }
                }
            }
            if (dam.ApplyModifier)
            {
                Actor Target = GetActorAt(dam.ImpactedTile);
                if (Target)
                {
                    IConstantHolder TargetAA = Target.GetComponent<IConstantHolder>();
                    if (TargetAA != null)
                    { TargetAA.AddConstantMod(dam.AppliedMod, this); }
                }
            }
        }
    }

    internal List<HexCoordinates> GetEmptySpaces(int widthstart, int widthend, int heightstart, int heightend)
    {
        List<HexCoordinates> ret = new List<HexCoordinates>();
        for(int x=widthstart; x<widthend; x++)
        {
            for(int z=heightstart; z<heightend; z++)
            {
                for(int y=0; y<mapDepth;y++)
                {
                    //Debug.Log(tileArray.OffsetToHex(x, y, z));
                    if(MyBoard.IsValidSpawnPoint(MyBoard.OffsetToHex(x,z,y)))
                    {
                        ret.Add(MyBoard.OffsetToHex(x, z, y));
                    }
                }
            }
        }
        return ret;
    }

    internal void AddNewActor(Actor actor, HexCoordinates hexCoordinates, EntityFactionEnum player)
    {
        MyBoard.PlaceAdditionalActor(actor, hexCoordinates, player);
    }

    internal TileList UnsafeGetTileList()
    {
        return MyBoard.UnsafeGetTileList();
    }

    internal void AddFeature(HexCoordinates coords, Feature feature)
    {
        MyBoard.AddFeature(coords, feature);
    }

    IEnumerator SmoothActorPathing(Actor actor, List<HexCoordinates> path)
    {
        actor.MovementUsed = true;
        float ObjectHeight = actor.transform.GetComponent<MeshRenderer>().bounds.extents.y;
        float travelSpeed = 4f;
        if(path == null) { Waiting = false; yield break; }
        for(int i=1; i< path.Count; i++)
        {
            Vector3 a = MyBoard.TileLocation(path[i - 1]);
            Vector3 b = MyBoard.TileLocation(path[i]);
            a= new Vector3(a.x, a.y + ObjectHeight, a.z);
            b = new Vector3(b.x, b.y + ObjectHeight, b.z);
            HexDirection dir = path[i - 1].GetFacing(path[i]);
            //actor.transform.localRotation.eulerAngles.Set(rotation.x, rotation.y, rotation.z); kept for posterity to demonstrate that THIS DOES NOTHING AT ALL 
            actor.Facing = dir;
            for (float t=0f; t< 1f; t += Time.deltaTime * travelSpeed)
            {
                actor.transform.localPosition = Vector3.Lerp(a, b, t);
                yield return null;
            }
        }
        TeleportActor(path[path.Count - 1], actor);
        Waiting = false;
    }

    internal List<Ability> GetValidAbilities(Actor focusedActor)
    {
        List<Ability> ActorAbilities = focusedActor.MyAbilties.MyAbilities;
        List<Ability> ret = new List<Ability>();

        for(int i=0;i<ActorAbilities.Count;i++)
        {
            if (ActorAbilities[i].OnCooldown()) { continue; }
            if (ActorAbilities[i].TimesUsed == ActorAbilities[i].UseLimit) { continue; }
            if(ActorAbilities[i].IsMovement)
            {
                if(!focusedActor.MovementUsed)
                {
                    ret.Add(ActorAbilities[i]);
                }
                continue;
            }
            if(!focusedActor.ActionUsed)
            {
                ret.Add(ActorAbilities[i]);
            }
        }
        return ret;
    }

    internal bool IsAccessible(HexCoordinates source, HexCoordinates target, Actor actor)
    {
        HexDirection dir = source.GetFacing(target);
        return MyBoard.IsAccessible(source, target, dir, actor);
    }

    internal void HighlightTiles(List<HexCoordinates> coords, bool toggle)
    {
        MyBoard.HighlightTiles(coords, toggle);
    }

    /// <summary>
    /// Returns true if coords can access a tile that exists / is on the map
    /// </summary>
    /// <param name="coords"></param>
    /// <returns></returns>
    internal bool ValidCoords(HexCoordinates coords)
    {
        return MyBoard.ValidCoords(coords);
    }

    public void UpdateFactionVisability(object obj, FactionVisionChanged e)
    {

    }

    public List<HexCoordinates> GetActorVision(Actor actor)
    {
        return MyBoard.GetActorVision(actor);
    }

    public void HandleTriggeredEffect(object obj, TrigEffectTriggered e)
    {
        HexCoordinates? Source = e.CondTriggeredArgs.TriggerSource.AbilitySourceHex;
        if (Source.HasValue)
        {
            //ability.SourceMap = this;
            ExecuteAbilityEffect(e.Modifier.GetAbilityEffect(Source.Value, e.CondTriggeredArgs.TriggerTarget.Value));
        }
    }
    */
}