using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu]
    public class EntityWeapon : ScriptableObject
    {
    public EntityWeaponType WeaponType;
    public string WeaponName { get { return WeaponType.name; } }
    public WeaponDamageType DamageType { get { return WeaponType.DamageType; } }
    public int AttackRange { get { return WeaponType.Range; } }
    public bool IsShield { get { return false; } }
    public float Damage { get { return WeaponType.Damage; } }
}
