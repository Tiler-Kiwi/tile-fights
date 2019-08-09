using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum WeaponDamageType
{
    UNASSIGNED = 0,
    Blunt = 1,
    Edged = 2,
    Pierce = 3,
    Magic = 4
}

[CreateAssetMenu]
    public class EntityWeaponType : ScriptableObject
    {
    public string Name;
    public int Range;
    public float Damage;
    public WeaponDamageType DamageType;
    }

public static class WeaponDamageTypeExtensions
{
    /// <summary>
    /// Get the damage amount for an attack made with damageType.
    /// </summary>
    /// <param name="damageType">damageType of damaging weapon</param>
    /// <param name="damageAmount">raw damage dealth via attack</param>
    /// <param name="armorAmount">target's armor</param>
    /// <param name="flanking">is the target being flanked?</param>
    /// <param name="cavbonus">is the attacker a horsie?</param>
    /// <returns>tuple HealthDam,ArmorDam</returns>
    public static Tuple<float,float> HealthDamage(this WeaponDamageType damageType, float damageAmount, float armorAmount, bool flanking, bool cavbonus)
    {
        float HealthDam = 0;
        float ArmorDam = 0;
        float FlankingBonus = flanking? (cavbonus ? 1.33f : 1.5f): 1;

        switch (damageType)
        {
            case WeaponDamageType.Blunt:
                ArmorDam = damageAmount * FlankingBonus;
                if (ArmorDam > armorAmount)
                {
                    HealthDam = ArmorDam - armorAmount;
                    ArmorDam = armorAmount;
                }
                HealthDam += ArmorDam * .33f;
                return new Tuple<float, float>(HealthDam, ArmorDam);
            case WeaponDamageType.Edged:
                if (!flanking)
                {
                    if (armorAmount > 0)
                    {
                        return new Tuple<float, float>(0, 0);
                    }
                    return new Tuple<float, float>(damageAmount * FlankingBonus, 0);
                }
                ArmorDam = damageAmount * FlankingBonus - damageAmount;
                if (ArmorDam > armorAmount)
                {
                    HealthDam = ArmorDam - armorAmount;
                    ArmorDam = armorAmount;
                }
                return new Tuple<float, float>(HealthDam, ArmorDam);
            case WeaponDamageType.Magic:
                return new Tuple<float, float>(damageAmount, 0);
            case WeaponDamageType.Pierce:
                ArmorDam = damageAmount * FlankingBonus;
                if (ArmorDam > armorAmount)
                {
                    HealthDam = ArmorDam - armorAmount;
                    ArmorDam = armorAmount;
                }
                HealthDam += ArmorDam * .5f;
                return new Tuple<float, float>(HealthDam, ArmorDam);
            case WeaponDamageType.UNASSIGNED:
                return new Tuple<float, float>(0, 0);
        }
        throw new Exception("Unrecongized damage type!");
    }
}

