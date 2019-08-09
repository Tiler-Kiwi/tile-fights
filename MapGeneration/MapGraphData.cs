using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;

[CreateAssetMenu]
public class MapGraphData : ScriptableObject {

    public GraphPoint PointPrefab;
    public Connection ConnectionPrefab;
    public GraphAndTreeContainer GTContainer;

    public bool GenerateRandomData = true;
    public int PointsToGenerate = 6;

    public void FuckYou()
    {
        GTContainer = new GraphAndTreeContainer(PointPrefab, ConnectionPrefab);
        if (GenerateRandomData)
        {
            GTContainer.GenerateRandomPoints(PointsToGenerate);
        }

    }
    private void Awake()
    {
        if (GTContainer == null) { FuckYou(); } //Why is a function called via Start calling a function that hasn't done Awake yet is beyond me, and is also PROFOUNDLY IRRITATING
    }

    public void GenerateRandomPoints(int count)
    {
        GTContainer.GenerateRandomPoints(count);
    }

    public void Clear()
    {
        GTContainer.Clear();
    }

    public void AddPoint(float pointX, float pointZ)
    {
        GTContainer.AddPoint(pointX, pointZ);
    }

        public void AddPoint(float pointX, float pointZ, float diff)
        {
        GTContainer.AddPoint(pointX, pointZ, diff);
        }

        public void AddConnection(GraphPoint pointA, GraphPoint pointB, float diff)
    {
        GTContainer.AddGraphConnection(pointA, pointB, diff);
    }

    public float Interpolate(float x, float z)
    {
        return GTContainer.InterpolateDiff(x, z);
    }

}
