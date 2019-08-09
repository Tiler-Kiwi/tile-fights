using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Does something as long as it exists, including carrying a triggered effect 
/// </summary>
    public abstract class ConstantModifier : Modifier
    {
    public bool IsClone = false;
    [SerializeField]
    private bool Initialized = false;
    public bool InnatePassive = false;
    public void Init(IConstantHolder holder)
    {
        if (!IsClone) { throw new Exception("Non clone Constant Modifier cannot be initialized!"); }
        if (Initialized) { throw new System.Exception("dont re init me"); }
        IAbilityUser User = holder.GetAbilityUser();
        for(int i=0;i<TriggeredEffects.Count;i++)
        {
            TriggeredEffects[i].Init(User);
        }
        _Init(holder);
        Initialized = true;
    }
    protected abstract void _Init(IConstantHolder holder);
    public List<TriggeredEffect> TriggeredEffects;

    public void RefreshTriggers()
    {
        for(int i=0;i<TriggeredEffects.Count;i++)
        {
            TriggeredEffects[i].RefreshTriggers();
        }
    }
    public ConstantModifier Clone()
    {
        ConstantModifier ret = _Clone();
        ret.name = this.name;
        ret.Name = this.Name;
        ret.NegativeEffect = this.NegativeEffect;
        ret.TriggeredEffects = new List<TriggeredEffect>();
        for (int i = 0; i < TriggeredEffects.Count; i++)
        {
            ret.TriggeredEffects.Add(this.TriggeredEffects[i].Clone());
        }
        ret.IsClone = true;
        return ret;
    }
    internal abstract ConstantModifier _Clone();
    protected void OnDestroy()
    {
        if (!IsClone) { throw new Exception("dont destroy non clone modifiers"); }
        for(int i=0;i<TriggeredEffects.Count;i++)
        {
            Destroy(TriggeredEffects[i]);
        }
    }
}

public enum ConstantModifierEnum
{
    UNASSIGNED = 0,
    Add = 1,
    Scalar = 2
}
