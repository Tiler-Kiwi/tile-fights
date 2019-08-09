using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MapEntityFaction : MonoBehaviour
{
    public Faction FactionObject;

    public bool IsHostile(MapEntityFaction entityFaction)
    {
        return IsHostile(entityFaction.FactionObject);
    }

    public bool IsHostile(Faction faction)
    {
        return (faction != FactionObject) ;
    }
}

