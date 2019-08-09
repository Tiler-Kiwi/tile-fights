using System;
using System.Collections;
using System.Collections.Generic;
using QuickGraph;
using UnityEngine;

[CreateAssetMenu]
public class Connection : ScriptableObject,QuickGraph.IEdge<GraphPoint> {

    public float DiffScore;

    [SerializeField]
    public GraphPoint _Source;
    [SerializeField]
    public GraphPoint _Target;

    GraphPoint IEdge<GraphPoint>.Source
    {
        get
        {
            return _Source;
        }
    }

    GraphPoint IEdge<GraphPoint>.Target
    {
        get
        {
            return _Target;
        }
    }
}
