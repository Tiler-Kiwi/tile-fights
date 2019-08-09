using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu]
public class Faction : ScriptableObject
{
    public string FactionName;
    public Guid FactionGUID = Guid.NewGuid();

    public bool IsHostile(Faction otherFaction)
    {
        return otherFaction != this;
    }
}

