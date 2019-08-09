using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GraphPoint : ScriptableObject {

    //[Range(0, 1)]
    public float XCoord;
    //[Range(0, 1)]
    public float ZCoord;

    public float Diff;
}
