using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Fires an event when some other event occurs. Requires initalization.
/// </summary>
public abstract class TriggerCondition : ScriptableObject
{
    public IAbilityUser Source;
    public int TriggerLimit = -1;
    public int TriggerCount = 0;
    public bool IsClone = false;
    [SerializeField]
    protected bool initialized = false;
    public EventHandler<ConditionTriggered> ConditionTriggeredEvent;

    protected void OnConditionTriggered(ConditionTriggered e)
    {
        if (!IsClone) { throw new Exception("Non clone condition used."); }
        if (!initialized) { throw new Exception("non intitialized TriggerCondition fired OnConditionTrigger"); }
        Debug.Log("ConditionTriggered: " + e.TriggerCondition.name);
        if (TriggerCount == TriggerLimit) { return; }
        EventHandler<ConditionTriggered> handler = ConditionTriggeredEvent;
        if (handler != null)
        {
            handler.Invoke(this, e);
        }
        TriggerCount++;
    }

    public void Init(IAbilityUser user)
    {
        if (!IsClone) { throw new Exception("Non clone condition may not be initialized."); }
        if (initialized) { throw new Exception("do not reinitalize an initialized TriggerCondition"); }
        Source = user;
        _Init(user);
        initialized = true;
    }
    protected abstract void _Init(IAbilityUser user);
    protected abstract void _OnDestroy();
    protected void OnDestroy()
    {
        if (!IsClone) { throw new Exception("Don't destroy non clone trigger conditions"); }
        _OnDestroy();
    }

    public TriggerCondition Clone()
    {
        TriggerCondition ret = this._Clone();
        ret.name = this.name;
        ret.TriggerLimit = this.TriggerLimit;
        ret.IsClone = true;
        return ret;
    }
    internal abstract TriggerCondition _Clone();
}

public class ConditionTriggered : EventArgs
{
    public ConditionTriggered(TriggerCondition condition, IAbilityUser source, HexCoordinates? target)
    {
        TriggerCondition = condition;
        TriggerSource = source;
        TriggerTarget = target;
    }
    public TriggerCondition TriggerCondition;
    public IAbilityUser TriggerSource;
    public HexCoordinates? TriggerTarget;
}