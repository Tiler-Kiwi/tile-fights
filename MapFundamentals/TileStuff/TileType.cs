using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu][System.Serializable]
public class TileType : ScriptableObject
{
    public Color TileTypeColor;

    [SerializeField]
    private bool _InnateImpassable;

    [SerializeField]
    private bool _Solid;

    public bool Solid { get { return _Solid; } }

    public bool Impassable()
    {
        return _InnateImpassable;
    }
}

public enum TileColor
{
    NONE = 0,
    Black = 1,
    Red = 2,
    Yellow = 3,
    Blue = 4,
    Green = 5,
    Orange = 6,
    Purple = 7,
    Grey = 8,
    White = 9,
    Pink = 10
}
