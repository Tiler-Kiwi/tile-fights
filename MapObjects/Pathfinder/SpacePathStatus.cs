using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public enum SpacePathStatus
    {
    UNASSIGNED = 0,
    Blocked = 1, //Space is totally inaccesable; path is invalid
    Clear = 2 //Path may be taken
    }

public enum SpaceNodeStatus
{
    UNASSIGNED = 0,
    Blocked = 1, //space is totally blocked and may not be entered for any reason
    InvalidTerrain = 2, //space is not blocked but for one reason or another cannot be accessed anyways (but maybe can be pushed into it)
    SoftClear = 3, //space is clear, but cannot be "stood" in; nto a valid end node for a path
    HardClear = 4 //space is "empty" and can be accessed normally
}

[Flags]
public enum OccupyLayers
{
    UNASSIGNED = 0,
    Actor = 1,
    Feature = 2
}

