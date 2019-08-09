using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// Applies an effect when triggered, including applying a constant modifier 
/// </summary>
public abstract class TriggeredEffect: ScriptableObject, IAbility
    {
    bool IsClone = false;
    public bool Initialized = false;
    public void Init(IAbilityUser user)
    {
        if (!IsClone) { throw new Exception("Non clone triggered effect may not be initialized! " + this.name); }
        if(Initialized) { throw new Exception("Init called on Initialized triggered effect " + this.name); }
        Source = user;
        

        for(int i=0;i<TriggerConditions.Count;i++)
        {
            TriggerConditions[i].Init(user);
            TriggerConditions[i].ConditionTriggeredEvent += HandleConditionTriggered;
            //this.TrigEffectTriggeredEvent += map.HandleTriggeredEffect;
        }
        Initialized = true;
    }

    /// <summary>
    /// returns a non-initalized deep copy of this triggered effect
    /// </summary>
    /// <returns></returns>
    internal abstract TriggeredEffect _Clone();
    public TriggeredEffect Clone()
    {
        TriggeredEffect ret = _Clone();
        ret.name = this.name;
        ret.TriggerConditions = new List<TriggerCondition>();
        for (int i = 0; i < this.TriggerConditions.Count; i++)
        {
            ret.TriggerConditions.Add(TriggerConditions[i].Clone());
        }
        ret.IsClone = true;
        return ret;
    }

    internal void RefreshTriggers()
    {
        for(int i=0;i<TriggerConditions.Count;i++)
        {
            TriggerConditions[i].TriggerCount = 0;
        }
    }

    protected IAbilityUser Source;
    protected BattleMap MyMap;

    private void OnModifierTriggered(TrigEffectTriggered e)
    {
        EventHandler<TrigEffectTriggered> handler = TrigEffectTriggeredEvent;
        if (handler != null)
        {
            handler.Invoke(this, e);
        }
    }
    public EventHandler<TrigEffectTriggered> TrigEffectTriggeredEvent; //fires when effect is triggered

    [SerializeField]
    protected List<TriggerCondition> TriggerConditions; //fires ModifierTriggerEvent when condition is triggered

    public BattleMap SourceMap
    {
        get
        {
            return MyMap;
        }

        set
        {
            MyMap = value;
        }
    }

    private void HandleConditionTriggered(object obj, ConditionTriggered e)
    {
        TrigEffectTriggered mt = new TrigEffectTriggered();
        mt.Modifier = this;
        mt.CondTriggeredArgs = e;
        OnModifierTriggered(mt);
    }


    private void OnDestroy()
    {
        if (!IsClone) { throw new Exception("dont call Destroy on non clone triggered effect"); }
        if(this.Initialized)
        {
            for (int i = 0; i < TriggerConditions.Count; i++)
            {
                TriggerConditions[i].ConditionTriggeredEvent -= HandleConditionTriggered;
                //this.TrigEffectTriggeredEvent -= MyMap.HandleTriggeredEffect;
                Destroy(TriggerConditions[i]);
            }
        }
    }

    public abstract AbilityEffect GetAbilityEffect(HexCoordinates source, HexCoordinates target);

    public void Initialize(IAbilityUser user)
    {
        throw new NotImplementedException();
    }
}

public class TrigEffectTriggered : EventArgs
    {
    public TriggeredEffect Modifier;
    public ConditionTriggered CondTriggeredArgs;
    }

//effects should live more independent from iabilityuser / iconstantholder. they should live "on the map"
//they are essentially some kind of hybrid class? need proper "constructor", need instantiation, require outside control to make sense