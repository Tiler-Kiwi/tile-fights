using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpIDW;
using QuickGraph;
using UnityEngine;


    public class GraphAndTreeContainer
    {
    public GraphAndTreeContainer(GraphPoint prefabPoint, Connection prefabConnection)
    {
        PointPrefab = prefabPoint;
        ConnectionPrefab = prefabConnection;
    }

    const int DIMENSIONS = 2;
    public List<GraphPoint> GPoints = new List<GraphPoint>();
    public List<Connection> GConnections = new List<Connection>();
    public GraphPoint PointPrefab;
    public Connection ConnectionPrefab;
    private System.Random rng = new System.Random();

    private bool NewInterpGraph;

    UndirectedGraph<GraphPoint, Connection> MapGraph = new UndirectedGraph<GraphPoint, Connection>();

    CSharpIDW.IdwInterpolator Interpolator = new CSharpIDW.IdwInterpolator(DIMENSIONS);

    public void GenerateRandomPoints(int count)
    {
        Clear();
        System.Random rng = new System.Random();
        for (int i = 0; i < count; i++)
        {
            float X = (float)rng.NextDouble();
            float Z = (float)rng.NextDouble();
            float diff = (float)rng.NextDouble();
            AddPoint(X, Z, diff);
            //Debug.Log(X + " "+ Z + " " + diff);
        }
    }

    public void Clear()
    {
        GPoints = new List<GraphPoint>();
        GConnections = new List<Connection>();
        MapGraph = new UndirectedGraph<GraphPoint, Connection>();
        Interpolator = new IdwInterpolator(DIMENSIONS);
        NewInterpGraph = false;
    }

    public void AddPoint(float pointX, float pointZ)
    {
        AddPoint(pointX, pointZ, RandomDiff());
    }

    public void AddPoint(float pointX, float pointZ, float diff)
    {
        GraphPoint GPoint = GraphPoint.Instantiate(PointPrefab);
        GPoint.XCoord = pointX;
        GPoint.ZCoord = pointZ;
        GPoint.Diff = diff;
        MapGraph.AddVertex(GPoint);
        GPoints.Add(GPoint);

        NewInterpGraph = true;
    }

    public void AddGraphConnection(GraphPoint pointA, GraphPoint pointB, float diff)
    {
        Connection NewConnection = Connection.Instantiate(ConnectionPrefab);
        NewConnection.DiffScore = diff;
        NewConnection._Source = pointA;
        NewConnection._Target = pointB;

        MapGraph.AddEdge(NewConnection);
        GConnections.Add(NewConnection);
        NewInterpGraph = true;
    }

    public void GraphPointsToIntPoints()
    {
        Interpolator = new CSharpIDW.IdwInterpolator(2);
        for (int i = 0; i < GPoints.Count; i++)
        {
            GraphPoint gp = GPoints[i];
            CSharpIDW.Point NewPoint = new CSharpIDW.Point(gp.Diff, gp.XCoord, gp.ZCoord);
            Interpolator.AddPoint(NewPoint);
        }
    }

    public float InterpolateDiff(float x, float z)
    {
        if (NewInterpGraph) { GraphPointsToIntPoints(); }
        return (float)Interpolator.Interpolate(x, z).Value;
    }
    private float RandomDiff()
    {
        return (float)rng.NextDouble();
    }
}

