using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EntityAbilityUser : MonoBehaviour, IAbilityUser
{
    public List<Ability> MyAbilities
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public WeaponDamageType WeaponDamageType
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public float BaseDamage
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public int AttackRange
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public Faction Faction
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public HexCoordinates? AbilitySourceHex
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public void AssignAbility(Ability ability)
    {
        throw new NotImplementedException();
    }

    public void RemoveAbility(Ability ability)
    {
        throw new NotImplementedException();
    }

    public HexCoordinates? Step(HexCoordinates source, HexDirection dir, EntityMapping sourceMap)
    {
        throw new NotImplementedException();
    }
}

