using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Entity mandates the checking of additional paths; ramps, ladders, etc.
/// </summary>
[RequireComponent(typeof(MapEntity))]
internal class EntityMapPathModifier : MonoBehaviour
{
    public ExtraPath[] Paths;
}