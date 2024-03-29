﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Container for a series of "Effect" commands, that ougt to be understood by AI / UI shit.
/// Generated by abilities
/// </summary>
/// 
public class AbilityEffect : IAbilityEffect
{
    List<SpaceDamage> SpaceDamageList = new List<SpaceDamage>();

    public int Count { get { return SpaceDamageList.Count; } }

    public void AddDamage(SpaceDamage damage)
    {
        SpaceDamageList.Add(damage);
    }
    public SpaceDamage GetDamage(int index)
    {
        return SpaceDamageList[index];
    }

    public List<SpaceDamage> GetDamageList()
    {
        return new List<SpaceDamage>(SpaceDamageList);
    }
}
