using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


    public abstract class Condition : ScriptableObject
    {
    public abstract bool Evaluate(EntityMapping map);
    }
