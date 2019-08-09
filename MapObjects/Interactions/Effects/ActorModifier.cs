using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "ConstantMod/Actor")]
public class ActorModifier : ConstantModifier
    {
    protected override void _Init(IConstantHolder holder)
    {
        
    }

    internal override ConstantModifier _Clone()
    {
        ConstantModifier mod = ScriptableObject.CreateInstance<ActorModifier>();
        mod.name = this.name;
        mod.Name = this.Name;
        mod.NegativeEffect = this.NegativeEffect;
        mod.TriggeredEffects = new List<TriggeredEffect>();
        for (int i = 0; i < TriggeredEffects.Count; i++)
        {
            mod.TriggeredEffects.Add(this.TriggeredEffects[i].Clone());
        }
        return mod;
    }

    public int BaseDamageMod = 0;
    public int MoveRangeMod = 0;
    public int VisionRangeMod = 0;
    public int CharacterHeightMoveMod = 0;
    public bool DisallowMovement = false;
    public bool DisallowAction = false;
    public bool DisablePassives = false;
    public bool DisableNonBasicActive = false;
    public bool ChangeWeaponProfToD = false;
}

public enum ActorStatEnum
{
    UNASSIGNED = 0,
    BaseDamage = 1,
    MoveRange = 2,
    VisionRange = 3,
    MovementUsed = 4,
    ActionUsed = 5
}
