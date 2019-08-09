using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
/*
/// <summary>
/// Monobehavior fulcrum for a collection of tiles; renders them via chunks, provides access methods without permitting direct referencing of the map or its pawns.
/// </summary>
[RequireComponent(typeof(MeshRenderer))] [RequireComponent(typeof(MeshFilter))]
public class MapBoard : MonoBehaviour
{
    MeshRenderer MyMeshR;
    MeshFilter MyMeshF;

    public List<Condition> PlayerWin;
    public List<Condition> FoeWin;

    public EventHandler<ActorSetIntoReserve> ActorSetIntoReserveEvent;
    public EventHandler<ActorReleasedFromReserve> ActorReleasedFromReserveEvent;
    public EventHandler<NewActorOnBoard> NewActorOnBoardEvent;
    public EventHandler<ActorDeletedFromBoard> ActorDeletedFromBoardEvent;
    public EventHandler<ActorRelocated> ActorRelocatedEvent;
    public EventHandler<TurnChanged> TurnChangedEvent;
    public EventHandler<FactionVisionChanged> FactionVisionChangedEvent;

    List<GameObject> SlowEater = new List<GameObject>();
    float EaterSecsPerFrame = .1f;

    internal List<HexCoordinates> GetAllValidCoords()
    {
        List<HexCoordinates> ret = new List<HexCoordinates>();
        for (int i = 0; i < ArrayTiles.Count; i++)
        {
            HexCoordinates coord = ArrayTiles.IndexToCoords(i);
            if(coord.Depth < 0) { throw new Exception(coord.ToString() + " " + i.ToString()); }
            ret.Add(coord);
        }
        return ret;
    }

    internal Color? GetTileColor(HexCoordinates hexCoordinates)
    {
        MapBoardChunk Chunk = GetChunkThatOwnsTile(hexCoordinates);
        if (Chunk == null) { return null; }
        return GetChunkThatOwnsTile(hexCoordinates).GetTileColor(hexCoordinates);
    }

    float DeltaTime = 0;

    EntityFactionEnum TurnTaker { get; set; }
    int TurnCount;
    [SerializeField]
    public bool _Paused = true;
    public bool Paused { get { return (ActorsOnBoard.Count == 0 || _Paused); } set { _Paused = value; } }
    public bool ForceEndTurn = true;

    Dictionary<Actor, Tuple<HexCoordinates?, EntityFactionEnum>> ActorsOnBoard;

    [SerializeField]
    TileList ArrayTiles;
    [SerializeField]
    int _Width;
    [SerializeField]
    int _Height;
    [SerializeField]
    int _Depth;
    [SerializeField]
    bool DFlag; //signals need to redraw mesh

    [SerializeField]
    List<MapBoardChunk> ChunkList;

    [SerializeField]
    MapBoardChunk ChunkPrefab;

    const int CHUNK_HEIGHT = 10;
    const int CHUNK_WIDTH = 10;
    const int CHUNK_DEPTH = 10;
    const decimal divbyCW = 1 / (decimal)CHUNK_WIDTH;
    const decimal divbyCH = 1 / (decimal)CHUNK_HEIGHT;
    const decimal divbyCD = 1 / (decimal)CHUNK_DEPTH;

    decimal Wc;
    decimal Hc;
    decimal Dc;

    public void Update()
    {
        while (DeltaTime < EaterSecsPerFrame)
        {
            DateTime start = DateTime.Now;
        if(SlowEater.Count>0)
        {
            Debug.Log(String.Format("Destroying {0}, Count {1}", SlowEater[0].name, SlowEater.Count));
            Destroy(SlowEater[0]);
            SlowEater.RemoveAt(0);
        }
            else { break; }
            DeltaTime = DeltaTime + (float)TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalSeconds;
        }
        DeltaTime = 0;

        if(!Paused)
        {
            CheckVictoryStatus();
            CheckTurn();
        }
    }

    private void CheckTurn()
    {
        List<Actor> ActorList = ActorsOnBoard.Keys.ToList();
        for (int i = 0; i < ActorList.Count; i++)
        {
            if (ActorsOnBoard[ActorList[i]].Item2 == TurnTaker) //Actor is from active team
            {
                if (!ActorList[i].NoActionsLeft) //Actors turn is not over
                {
                    return;
                }
            }
        }

        if (ForceEndTurn) { AdvanceTurn(); }
    }

    private void CheckVictoryStatus()
    {
       for(int i=0;i<FoeWin.Count;i++)
        {
            if (FoeWin[i].Evaluate(FindObjectOfType<BattleMap>()))
            {
                DoVictoryShit(EntityFactionEnum.Foe);
            }
        }
       for(int i=0;i<PlayerWin.Count;i++)
        {
            if (PlayerWin[i].Evaluate(FindObjectOfType<BattleMap>()))
            {
                DoVictoryShit(EntityFactionEnum.Player);
            }
        }
    }

    public EntityFactionEnum Winner = EntityFactionEnum.UNASSIGNED;
    private void DoVictoryShit(EntityFactionEnum player)
    {
        for(int i=0;i<ChunkList.Count;i++)
        {
            ChunkList[i].GetComponent<MeshRenderer>().enabled = false;
        }
        this.Paused = true;
        Winner = player;
    }

    public void AdvanceTurn()
    {
        TurnTaker = TurnTaker.Next();
        TurnChanged e = new TurnChanged();
        if (TurnTaker == EntityFactionEnum.Player)
        {
            TurnCount++;
            e.TurnCountIncrerased = true;
        }

        e.ActiveFaction = TurnTaker;
        e.Board = this;
        e.CurrentTurnCount = TurnCount;
        e.PriorActiveFaction = TurnTaker.Previous();
        OnTurnChanged(e);

        List<Actor> ActorList = ActorsOnBoard.Keys.ToList();
        for (int i = 0; i < ActorList.Count; i++)
        {
            if (ActorsOnBoard[ActorList[i]].Item2 == TurnTaker)
            {
                ActorList[i].StartTurn();
            }
            else if (ActorsOnBoard[ActorList[i]].Item2 == TurnTaker.Previous())
            {
                ActorList[i].EndTurn();
            }
        }
    }

    internal void ChangeTileColor(HexCoordinates coords, Color? highlightColor)
    {
        MapBoardChunk Chunk = GetChunkThatOwnsTile(coords);
        if(Chunk == null) { return; }
        //Debug.Log(Chunk? Chunk.name:"FUCK" + " " + coords.ToString());
        int? ChunkTileIndex = Chunk.GetTileChunkIndex(coords);
        if (!ChunkTileIndex.HasValue) { return; }
        if (highlightColor.HasValue)
        {
            //ArrayTiles.GetTile(coords).MyFloorColor = highlightColor.Value;
            Chunk.ChangeTileColor(ChunkTileIndex.Value, highlightColor.Value);
        }
        else
        {
            //ArrayTiles.GetTile(coords).UseOverrideColor = false;
            Chunk.ResetTileColor(ChunkTileIndex.Value);
        }
    }

    MapBoardChunk GetChunkThatOwnsTile(int tileIndex)
    {
        Vector3 Offsets = ArrayTiles.IndexToOffsets(tileIndex);
        return GetChunkThatOwnsTile(Offsets);
    }
    MapBoardChunk GetChunkThatOwnsTile(HexCoordinates coords)
    {
        Vector3 Offsets = ArrayTiles.CoordsToOffsets(coords);
        return GetChunkThatOwnsTile(Offsets);
    }
    MapBoardChunk GetChunkThatOwnsTile(Vector3 offsets)
    {
        int ChunkIndex = (int)(Math.Floor((decimal)offsets.x * divbyCW) + Wc * (Math.Floor((decimal)offsets.z * divbyCH) + Hc * Math.Floor((decimal)offsets.y * divbyCD)));
        if (ChunkIndex < 0 || ChunkIndex >= ChunkList.Count) { return null; } //location is outside map bounds  //{ throw new Exception("Something fucked up! " + ChunkIndex.ToString() + offsets.ToString()); }
        return ChunkList[ChunkIndex];
    }
    public void Awake()
    {
        ActorsOnBoard = new Dictionary<Actor, Tuple<HexCoordinates?, EntityFactionEnum>>();
        ArrayTiles = new TileList(_Width, _Height, _Depth);
        TurnCount = 0;
        ChunkList = new List<MapBoardChunk>();
        DFlag = false;

        MyMeshF = GetComponent<MeshFilter>();
        MyMeshR = GetComponent<MeshRenderer>();
    }

    public TileType GetFloorType(HexCoordinates hexCoordinates)
    {
        int? index = ArrayTiles.CoordsToIndex(hexCoordinates);
        if (index.HasValue)
        {
            TileType floor = ArrayTiles.GetFloorType(index.Value);
            return floor;
        }
        return null;
    }

    public TileType GetSolidType(HexCoordinates hexCoordinates)
    {
        int? index = ArrayTiles.CoordsToIndex(hexCoordinates);
        if (index.HasValue)
        {
            return ArrayTiles.GetSolidType(index.Value);
        }
        return null;
    }

    public void AssignTiles(TileList tilelist)
    {
        _Width = tilelist.XDim;
        _Height = tilelist.ZDim;
        _Depth = tilelist.YDim;
        Wc = Math.Ceiling((decimal)_Width / CHUNK_WIDTH);
        Hc = Math.Ceiling((decimal)_Height / CHUNK_HEIGHT);
        Dc = Math.Ceiling((decimal)_Depth / CHUNK_DEPTH);
        Clear();
        ArrayTiles = tilelist;
        GenerateChunks();

        Debug.Log(ArrayTiles.Count);
    }

    public void Clear() // Not really a good thing to use, as destroying a large amount of objects can take FOREVER AND EVER AND EVER AND EVER.
    {
for(int i = 0; i < ChunkList.Count; i++) {
            if(ChunkList[i]!=null)
            {
                //Destroy(ChunkList[i].gameObject);
                //ChunkList[i].enabled = false;
                SlowEater.Add(ChunkList[i].gameObject);
            }
        }
        ArrayTiles = new TileList(_Width, _Height, _Depth);
        ChunkList = new List<MapBoardChunk>();
        DFlag = false;
    }

    public void BlankBoard(int width, int height, int depth)
    {
        _Width = width;
        _Height = height;
        _Depth = depth;
        ArrayTiles = new TileList(_Width, _Height, _Depth);

        if (width == 0 || height == 0 || depth == 0)
        {
            return;
        }
        for (int i = 0; i < _Width * _Height * _Depth; i++)
        {
            Tile NewTile = new Tile();
            ArrayTiles.SetTile(i, NewTile);
        }
       // ArrayTiles.SetTileTransforms();
        GenerateChunks();
    }

    private void GenerateChunks()
    {
        decimal ChunkCount = Wc * Hc * Dc;

        ChunkList = new List<MapBoardChunk>();
        List<List<int>> toBeAssignedToChunk = new List<List<int>>();
        for(int i = 0; i<ChunkCount;i++)
        {
            toBeAssignedToChunk.Add(new List<int>());
        }

        for (decimal y = 0; y < _Depth; y++)
        {
            for(decimal z =0; z<_Height;z++)
            {
                for (decimal x = 0; x < _Width; x++)
                {
                    decimal index = x + _Width * (z + _Height * y);
                    decimal ChunkIndex = Math.Floor(x * divbyCW) + Wc * (Math.Floor(z * divbyCH) + Hc * Math.Floor(y * divbyCD)); //this fucking sucks

                        toBeAssignedToChunk[(int)ChunkIndex].Add((int)index);
                }
            }
        }
        for(int i = 0; i< toBeAssignedToChunk.Count; i++)
        {
            if (toBeAssignedToChunk[i].Count == 0) { continue; }
            MapBoardChunk newChunk = MapBoardChunk.Instantiate<MapBoardChunk>(ChunkPrefab);
            newChunk.name = "Chunk " + i;
            newChunk.transform.SetParent(this.transform, false);
            ChunkList.Add(newChunk);
            ChunkList[ChunkList.Count-1].AssignTiles(toBeAssignedToChunk[i], ArrayTiles);
            ChunkList[ChunkList.Count - 1].CreateChunk();
        }
    }

    public void ForceRedraw()
    {
        for (int i = 0; i < ChunkList.Count; i++)
        {
            ChunkList[i].UpdateChunk(false);
        }
    }

    public void RedrawDirtyChunks()
    {
        for(int i=0; i < ChunkList.Count; i++)
        {
            if(ChunkList[i].dFlagChunk)
            {
                ChunkList[i].UpdateChunk(true);
            }
        }
    }


//ArrayTiles junk...
    public float Count
    {
        get { return ArrayTiles.Count; }
        set { }
    }

    public void SetTile(HexCoordinates coords, Tile tile)
    {
        ArrayTiles.SetTile(coords, tile);
    }

    int? CoordsToIndex(HexCoordinates coords)
    {
        return ArrayTiles.CoordsToIndex(coords);
    }

    HexCoordinates IndexToCoords(int index)
    {
        return ArrayTiles.IndexToCoords(index);
    }

    public bool IsAccesibleNeighbor(HexCoordinates source, HexCoordinates target, HexDirection dir)
    {
        List<HexCoordinates> check = source.GetCoordsWithinDistance(1);
        if (!check.Contains(target)) { return false; } // not a neighbor
        return ArrayTiles.IsAccessible(source, target, dir);
    }

    private bool HasFloor(HexCoordinates source)
    {
        return ArrayTiles.HasSolidFloor(source);
    }

    public Actor GetActorAt(HexCoordinates source)
    {
        return ArrayTiles.GetActorAt(source);
    }

    public void PushActor(HexCoordinates impactedTile, HexDirection directionalMod, int pushDistance)
    {
        ArrayTiles.PushActor(impactedTile, directionalMod, pushDistance);
    }

    public void DamageActor(float damage, HexCoordinates impactedTile, HexDirection impactDirection, bool isProjectile, WeaponDamageType weaponDamageType)
    {
        ArrayTiles.DamageActor(damage, impactedTile, impactDirection, isProjectile, weaponDamageType);
    }

    public HexCoordinates OffsetToHex(int x, int z, int y)
    {
        return HexCoordinates.FromOffsetCoordinates(x, z, y);
    }
    public bool IsValidSpawnPoint(HexCoordinates hexCoordinates)
    {
        return ArrayTiles.IsValidSpawnPoint(hexCoordinates);
    }

    public void PlaceAdditionalActor(Actor actor, HexCoordinates? hexCoordinates, EntityFactionEnum faction)
    {
        if (ActorsOnBoard.ContainsKey(actor)) { throw new Exception(string.Format( "Actor {0} already exists on {1}",actor, ActorsOnBoard[actor].ToString())); }
        else { ActorsOnBoard.Add(actor, new Tuple<HexCoordinates?, EntityFactionEnum>(hexCoordinates, faction));}
        if (hexCoordinates.HasValue)
        {
            ArrayTiles.AddActor(actor, hexCoordinates.Value);
            actor.FactionFunction = GetActorFaction;
            actor.LocationFunc = GetActorLocation;
            actor.GetComponent<ActorColor>().ResetActorColor();
        }
        actor.ActorClickedEvent += HandleActorClick;
        actor.ActorDiedEvent += HandleActorDeath;
        actor.VisionChangedEvent += HandleActorVisionChange;
        NewActorOnBoard e = new NewActorOnBoard();
        e.Board = this;e.Pawn = actor;e.SpaceAddedTo = hexCoordinates;
        OnNewActorOnBoard(e);
    }

    public EntityFactionEnum GetActorFaction(Actor actor)
    {
        return this.ActorsOnBoard[actor].Item2;
    }

    public TileList UnsafeGetTileList()
    {
        return ArrayTiles as TileList;
    }

    public void AddFeature(HexCoordinates coords, Feature feature)
    {
        ArrayTiles.AddTileFeature(ArrayTiles.CoordsToIndex(coords).Value, feature);
        FactionVisionChanged e = new FactionVisionChanged();
        e.Board = this; e.Faction = GetActiveFaction(); e.dFlag = true;
        OnFactionVisionChanged(e);
    }

    public bool TileHasWall(HexCoordinates value, HexDirection dir)
    {
        return ArrayTiles.TileHasWall(value, dir);
    }

    public bool TileBlocked(HexCoordinates value)
    {
        return ArrayTiles.TileBlocked(value);
    }

    public Vector3 TileLocation(HexCoordinates coords)
    {
        return ArrayTiles.TileLocation(coords);
    }

    /// <summary>
    /// Move an actor that already exists on the current map
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="targetActor"></param>
    public void MoveActor(HexCoordinates? coords, Actor targetActor)
    {
        Tuple<HexCoordinates?, EntityFactionEnum> old = ActorsOnBoard[targetActor];
        if(old.Item1 == coords) { return; } //dont do anything if it isnt actually moving
        bool StartedOffBoard = true;
        bool EndedOffBoard = true;
        if (old.Item1.HasValue) //actor is on board presently
        {
            StartedOffBoard = false;
            ArrayTiles.RemoveActor(targetActor, old.Item1.Value);
        }
        ActorsOnBoard[targetActor] = new Tuple<HexCoordinates?, EntityFactionEnum>(coords, old.Item2);
        if (coords.HasValue)
        {
            EndedOffBoard = false;
            ArrayTiles.AddActor(targetActor, coords.Value);
        }
        if(StartedOffBoard)
        {
            ActorReleasedFromReserve e = new ActorReleasedFromReserve();
            e.Board = this; e.Pawn = targetActor; e.SpaceAddedTo = coords.Value;
            OnActorReleasedFromReserve(e);
        }
        else if(EndedOffBoard)
        {
            ActorSetIntoReserve e = new ActorSetIntoReserve();
            e.Board = this; e.Pawn = targetActor; e.SpaceRemovedFrom = old.Item1.Value;
            OnActorSetIntoReserve(e);
        }
        else
        {
            ActorRelocated e = new ActorRelocated();
            e.Board = this; e.Pawn = targetActor; e.SpaceAddedTo = coords.Value; e.SpaceRemovedFrom = old.Item1.Value;
            OnActorRelocated(e);
        }
    }

    public HexCoordinates? GetActorLocation(Actor actor)
    {
        return ActorsOnBoard[actor].Item1;
    }

    public void DeleteActor(Actor actor)
    {
        ActorDeletedFromBoard e = new ActorDeletedFromBoard();
        e.Board = this; e.Pawn = actor; e.SpaceRemovedFrom = GetActorLocation(actor);
        if (ActorsOnBoard[actor].Item1.HasValue)
        {
            ArrayTiles.RemoveActor(actor, ActorsOnBoard[actor].Item1.Value);
        }
       ActorsOnBoard.Remove(actor);
        actor.ActorClickedEvent -= HandleActorClick;
        actor.ActorDiedEvent -= HandleActorDeath;
        actor.VisionChangedEvent -= HandleActorVisionChange;
        OnActorDeletedFromBoard(e);
    }

    public bool CanPath(HexCoordinates source, HexCoordinates target, Actor actor)
    {
        throw new Exception("dont use this");
    }

    public bool IsAccessible(HexCoordinates source, HexCoordinates target, HexDirection dir, Actor actor)
    {
        return ArrayTiles.IsAccessible(source, target, dir, actor);
    }

    public void HighlightTiles(List<HexCoordinates> coords, bool toggle)
    {
        TileHighlight Highlighter = this.GetComponent<TileHighlight>();
        if(Highlighter == null) { return; }

        if(toggle)
        {
            Highlighter.AssignHighlights(coords);
            Highlighter.HighlightToggle(true);
        }
        else
        {
            Highlighter.HighlightToggle(false);
        }
    }

    public bool ValidCoords(HexCoordinates coords)
    {
        return ArrayTiles.GetTile(coords) != null;
    }

    public int GetCurrentTurn()
    {
        return TurnCount;
    }


    public EntityFactionEnum GetActiveFaction()
    {
        return TurnTaker;
    }

    public List<Actor> GetFactionActors(EntityFactionEnum factionEnum)
    {
        List<Actor> ret = new List<Actor>();
        List<Actor> PresentActors = ActorsOnBoard.Keys.ToList();
        for(int i=0; i< PresentActors.Count; i++)
        {
            if(PresentActors[i].Faction == factionEnum)
            {
                ret.Add(PresentActors[i]);
            }
        }
        return ret;
    }

    public List<Feature> GetFeaturesAtCoords(HexCoordinates value)
    {
        return ArrayTiles.GetTile(value).Features; //kludgy
    }

    void HandleActorClick(object obj, ActorClicked e)
    {
        HexCoordinates? ActorCoords = GetActorLocation(e.Target);
        if (!ActorCoords.HasValue) { return; }
        MapBoardChunk Chunk = GetChunkThatOwnsTile(ActorCoords.Value);
        Chunk.ClickedAtCoords(ActorCoords.Value);
    }

    void HandleActorDeath(object obj, ActorDied e)
    {
        MoveActor(null, e.Target);
    }
    void HandleActorVisionChange(object obj, VisionChanged e)
    {
        FactionVisionChanged fvc = new FactionVisionChanged();
        fvc.Board = this; fvc.Faction = e.Target.Faction;
        OnFactionVisionChanged(fvc);
    }
    public void OnActorSetIntoReserve( ActorSetIntoReserve e)
    {
        EventHandler<ActorSetIntoReserve> handler = ActorSetIntoReserveEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        EntityFactionEnum Faction = ActorsOnBoard[e.Pawn].Item2;
        FactionVisionChanged vc = new FactionVisionChanged();
        vc.Board = this; vc.Faction = Faction;
        OnFactionVisionChanged(vc);
    }
    public void OnActorReleasedFromReserve(ActorReleasedFromReserve e)
    {
        EventHandler<ActorReleasedFromReserve> handler = ActorReleasedFromReserveEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        EntityFactionEnum Faction = ActorsOnBoard[e.Pawn].Item2;
        FactionVisionChanged vc = new FactionVisionChanged();
        vc.Board = this; vc.Faction = Faction; vc.dFlag = true;
        OnFactionVisionChanged(vc);
    }
    public void OnNewActorOnBoard(NewActorOnBoard e)
    {
        EventHandler<NewActorOnBoard> handler = NewActorOnBoardEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        if (ActorsOnBoard[e.Pawn].Item1.HasValue)
        {
            EntityFactionEnum Faction = ActorsOnBoard[e.Pawn].Item2;
            FactionVisionChanged vc = new FactionVisionChanged();
            vc.Board = this; vc.Faction = Faction; vc.dFlag = true;
            OnFactionVisionChanged(vc);
        }
    }
    public void OnActorDeletedFromBoard(ActorDeletedFromBoard e)
    {
        EventHandler<ActorDeletedFromBoard> handler = ActorDeletedFromBoardEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        EntityFactionEnum Faction = ActorsOnBoard[e.Pawn].Item2;
        FactionVisionChanged vc = new FactionVisionChanged();
        vc.Board = this; vc.Faction = Faction;
        OnFactionVisionChanged(vc);
    }
    public void OnActorRelocated(ActorRelocated e)
    {
        EventHandler<ActorRelocated> handler = ActorRelocatedEvent;
        if (handler != null)
        {
            handler(this, e);
        }
        EntityFactionEnum Faction = ActorsOnBoard[e.Pawn].Item2;
        FactionVisionChanged vc = new FactionVisionChanged();
        vc.Board = this; vc.Faction = Faction; vc.dFlag = true;
        OnFactionVisionChanged(vc);
        
    }
    public void OnTurnChanged(TurnChanged e)
    {
        EventHandler<TurnChanged> handler = TurnChangedEvent;
        if (handler != null)
        {
            handler(this, e);
        }
    }
    public void OnFactionVisionChanged(FactionVisionChanged e)
    {
        EventHandler<FactionVisionChanged> handler = FactionVisionChangedEvent;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    public List<HexCoordinates> GetActorVision(Actor actor)
    {
        List<HexCoordinates> ret = new List<HexCoordinates>();
        HexCoordinates? ActorLocation = GetActorLocation(actor);
        if (!ActorLocation.HasValue) { return ret; }
        ret.Add(ActorLocation.Value);
        List<HexCoordinates> Check = ActorLocation.Value.GetCoordsWithinDistance(actor.VisionRange, true, true);
        for(int i=0;i<Check.Count;i++)
        {
            if (!ValidCoords(Check[i])){ continue; }
            if (GetFloorType(Check[i]) == null) { continue; } //lets not care if you cant see a tile that never has anything on it
            HexDirection DirectionFromActor = ActorLocation.Value.GetFacing(Check[i]);
            if (DirectionFromActor == actor.Facing.Opposite() || DirectionFromActor == actor.Facing.Opposite().Next() || DirectionFromActor == actor.Facing.Opposite().Previous())
            { continue; }
            List<HexCoordinates> PathOfVision = HexCoordinates.cube_linedraw(ActorLocation.Value, Check[i]);
            bool SightBlocked = false;
            for(int k=0;k<PathOfVision.Count-1;k++)
            {
                Tile tile = ArrayTiles.GetTile(PathOfVision[k]);
                if (tile != null && tile.BlocksVisibility)
                {
                    SightBlocked = true;
                    break;
                }
            }
            if (!SightBlocked) { ret.Add(Check[i]); }
        }
        return ret;
    }

    public void RemoveFeature(Feature featurePrefab, HexCoordinates location)
    {
        ArrayTiles.DeleteTileFeature(location, featurePrefab);
        FactionVisionChanged e = new FactionVisionChanged();
        e.Board = this;e.Faction = GetActiveFaction();e.dFlag = true;
        OnFactionVisionChanged(e);
    }
}

/// <summary>
/// Actor has been placed into reserve, but not deleted from the board
/// </summary>

    */