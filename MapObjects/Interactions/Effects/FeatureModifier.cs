using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "ConstantMod/Feature")]
   public class FeatureModifier : ConstantModifier
{
    protected override void _Init(IConstantHolder holder)
    {
        
    }

    internal override ConstantModifier _Clone()
    {
        ConstantModifier mod = ScriptableObject.CreateInstance<FeatureModifier>();
        return mod;
    }
}

