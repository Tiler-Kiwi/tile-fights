using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
    public interface IConstantHolder
    {
    List<ConstantModifier> HeldConstantMods { get; }
    void AddConstantMod(ConstantModifier appliedMod, BattleMap map);
    List<ConstantModifier> GetNotInnateMods();
    List<ConstantModifier> GetInnateMods();
    void RemoveConstantMod(ConstantModifier baseModifier);
    IAbilityUser GetAbilityUser();
}

