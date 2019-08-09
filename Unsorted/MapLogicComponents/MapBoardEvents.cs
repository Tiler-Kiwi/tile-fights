using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ActorSetIntoReserve : EventArgs
{
    public MapEntity Pawn;
    public HexCoordinates SpaceRemovedFrom;
}

public class ActorReleasedFromReserve : EventArgs
{
    public MapEntity Pawn;
    public HexCoordinates SpaceAddedTo;
}

public class NewActorOnBoard : EventArgs
{
    public MapEntity Pawn;
    public HexCoordinates? SpaceAddedTo;
}

public class ActorDeletedFromBoard : EventArgs
{
    public MapEntity Pawn;
    public HexCoordinates? SpaceRemovedFrom;
}

public class ActorRelocated : EventArgs
{
    public MapEntity Pawn;
    public HexCoordinates SpaceRemovedFrom;
    public HexCoordinates SpaceAddedTo;
}

public class TurnChanged : EventArgs
{
   // public IMapBoard Board;
    public int CurrentTurnCount;
    public bool TurnCountIncrerased;
    public Faction ActiveFaction;
    public Faction PriorActiveFaction;
}

public class FactionVisionChanged : EventArgs
{
    public Faction Faction;
    public bool dFlag; //if actor or feature has moved, redraw all your vision ranges
}
