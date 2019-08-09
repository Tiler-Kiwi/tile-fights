using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Rendering element for a section of the map board; redesigned to behave autonomously rather than being directed by logical commands.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IsolatedChunk : MonoBehaviour
{
    public Mesh MapChunkMesh;
    [SerializeField]
    TileListBase BaseTiles;
    List<HexCoordinates> ChunkTile; //tile coords that chunk manages
    List<int> TileTriangleCount; //number of triangles owned by each tile
    List<Vector3> ChunkVectors; //vectors held by chunk for drawing triangles
    List<int> ChunkTriangles; //triangles' vectors' indexes
    MeshCollider ChunkCollider; //collider used by chunk
    List<Color> ChunkColors; //colors used by chunk
    public bool dFlagChunk; //signals need for redrawing

    public EventHandler<IsoChunkSelected> ChunkOnMouseEnterEvent;
    public EventHandler<IsoChunkSelected> ChunkClickEvent;
    public EventHandler<IsoChunkDeleted> ChunkDeletedEvent;

    public void Awake()
    {
        GetComponent<MeshFilter>().mesh = MapChunkMesh = new Mesh();
        ChunkVectors = new List<Vector3>();
        ChunkTriangles = new List<int>();
        //ChunkCollider = gameObject.GetComponent<MeshCollider>();Location
        ChunkCollider = gameObject.AddComponent<MeshCollider>();
    }

    internal Color? GetTileColor(HexCoordinates coords)
    {
        int? TileChunkIndex = GetTileChunkIndex(coords);
        if (!TileChunkIndex.HasValue) { return null; }
        int TriangleIndex = 0;
        for (int i = 0; i < TileChunkIndex.Value; i++)
        {
            TriangleIndex += TileTriangleCount[i];
        }
        TriangleIndex = TriangleIndex * 3;
        for (int i = 0; i < TileTriangleCount[TileChunkIndex.Value] * 3; i++)
        {
            if (ChunkColors[TriangleIndex + i] != null)
            {
                return ChunkColors[TriangleIndex + i];
            }
        }
        return null;
    }

    public void OnDestroy()
    {
        IsoChunkDeleted deletedargs = new IsoChunkDeleted();
        deletedargs.Array = BaseTiles;
        deletedargs.Chunk = this;
        OnChunkDeleted(deletedargs);
    }

    public void AssignTiles(List<HexCoordinates> tiles, TileListBase baseTiles)
    {
        BaseTiles = baseTiles;
        ChunkTile = tiles;
        TileTriangleCount = new List<int>();
        for (int i = 0; i < ChunkTile.Count; i++)
        {
            TileTriangleCount.Add(0);
        }
        MapChunkMesh.Clear();
        ChunkVectors = new List<Vector3>();
        ChunkTriangles = new List<int>();
        ChunkColors = new List<Color>();

        for (int i = 0; i < ChunkTile.Count; i++)
        {
            BaseTiles.GetTile(ChunkTile[i]).TileTypeChangedEvent += TileTypeChanged;
        }

        CreateChunk();
    }

    public void Update()
    {
        if (dFlagChunk)
        {
            this.UpdateChunk(true);
        }
    }

    private void CreateChunk()
    {
        MapChunkMesh.Clear();
        ChunkVectors = new List<Vector3>();
        ChunkTriangles = new List<int>();
        ChunkColors = new List<Color>();

        for (int k = 0; k < ChunkTile.Count; k++)
        {
            if (BaseTiles.GetFloorType(ChunkTile[k]) == null) { continue; }
            Tile TheTile = BaseTiles.GetTile(ChunkTile[k]);
            Vector3 center = BaseTiles.TileLocation(ChunkTile[k]);
            if (BaseTiles.GetTile(ChunkTile[k]).solidType != null) { DrawSolid(center, TheTile.MySolidColor.Value, k); }
            else
            {
                Color? FloorColor = BaseTiles.GetFloorType(ChunkTile[k]).TileTypeColor;
                if (!FloorColor.HasValue)
                {
                    FloorColor = BaseTiles.GetTile(ChunkTile[k].Below).MySolidColor;
                    if (!FloorColor.HasValue)
                    {
                        continue; //this shouldnt come up but oh well
                    }
                }
                DrawFloor(center, FloorColor.Value, k);
            }
        }
        MapChunkMesh.vertices = ChunkVectors.ToArray();
        MapChunkMesh.triangles = ChunkTriangles.ToArray();
        MapChunkMesh.colors = ChunkColors.ToArray();
        MapChunkMesh.RecalculateNormals();
        ChunkCollider.sharedMesh = MapChunkMesh;
        dFlagChunk = false;
    }

    public int? GetTileChunkIndex(HexCoordinates coords)
    {
        if (!ChunkTile.Contains(coords)) { return null; }
        return ChunkTile.IndexOf(coords);
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, int tileIndex)
    //mesh draws whatever is in TileTriangles. every three things = one triangle.
    //you could probably re-use vectors instead of re-adding them if they already exist but thats just a waste of effort
    {
        int vertexIndex = ChunkVectors.Count;
        ChunkVectors.Add(v1);
        ChunkVectors.Add(v2);
        ChunkVectors.Add(v3);
        ChunkTriangles.Add(vertexIndex);
        ChunkTriangles.Add(vertexIndex + 1);
        ChunkTriangles.Add(vertexIndex + 2);

        TileTriangleCount[tileIndex] = TileTriangleCount[tileIndex] + 1;
    }

    private void AddTriangleColor(Color color)
    {
        ChunkColors.Add(color);
        ChunkColors.Add(color);
        ChunkColors.Add(color);
    }

    private void UpdateChunk(bool respectDflag)
    {
        if (dFlagChunk || !respectDflag)
        {

            MapChunkMesh.Clear();

            MapChunkMesh.vertices = ChunkVectors.ToArray();
            MapChunkMesh.triangles = ChunkTriangles.ToArray();
            MapChunkMesh.colors = ChunkColors.ToArray();
            MapChunkMesh.RecalculateNormals();

            // CreateChunk();
        }
        dFlagChunk = false;
    }

    private void DrawFloor(Vector3 center, Color color, int tileIndex)
    {
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(
            center,
            center + HexMetrics.GetFirstSolidCorner((HexDirection)i),
            center + HexMetrics.GetSecondSolidCorner((HexDirection)i),
            tileIndex);


            AddTriangleColor(color);
        }
    }

    private void DrawSolid(Vector3 center, Color color, int tileIndex)
    {
        Vector3 StepItUp = new Vector3(0, HexMetrics.elevationStep, 0);
        DrawFloor(StepItUp + center, color, tileIndex);
        for (int i = 0; i < 6; i++)
        {
            Vector3 NewVect1 = center + HexMetrics.GetFirstSolidCorner((HexDirection)i);
            Vector3 NewVect2 = center + HexMetrics.GetSecondSolidCorner((HexDirection)i);
            AddTriangle(
                center + HexMetrics.GetFirstSolidCorner((HexDirection)i) + StepItUp,
                NewVect1,
                center + HexMetrics.GetSecondSolidCorner((HexDirection)i) + StepItUp,
                tileIndex);
            AddTriangle(
                center + HexMetrics.GetSecondSolidCorner((HexDirection)i) + StepItUp,
                NewVect1,
                NewVect2,
                tileIndex);

            AddTriangleColor(color);
            AddTriangleColor(color);
        }
    }

    void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; }
        HexCoordinates? tileCoords = CursorToCoords();
        if (!tileCoords.HasValue) { return; }
        IsoChunkSelected e = new IsoChunkSelected();
        e.Array = BaseTiles;
        e.Chunk = this;
        e.TileCoords = tileCoords.Value;
        OnChunkEntered(e);

    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; }
        HexCoordinates? tileCoords = CursorToCoords();
        if (!tileCoords.HasValue) { Debug.Log("Fuck you Unity"); return; }
        IsoChunkSelected e = new IsoChunkSelected();
        e.Array = BaseTiles;
        e.Chunk = this;
        e.TileCoords = tileCoords.Value;
        OnChunkClicked(e);

    }

    private HexCoordinates? CursorToCoords()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return null; }
        Vector3 mousepos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousepos);
        RaycastHit[] hit = Physics.RaycastAll(ray);
        if (0 < hit.Length)
        {
            if (hit[0].transform == this.transform) //hit the tile directly
            {
                Vector3 offsets = hit[0].point;
                offsets.z = offsets.z * -1;
                double q = (Math.Sqrt(3) / 3 * offsets.x - 1.0 / 3 * offsets.z) / HexMetrics.outerRadius; //x
                double r = (2.0 / 3 * offsets.z) / HexMetrics.outerRadius; //z
                HexCoordinates? tileCoords = HexCoordinates.RoundedCubes(q, -q - r, r, offsets.y / HexMetrics.elevationStep);
                return tileCoords;
            }
            return null;
        }
        return null;
    }

    // These methods have been changed to private. The renderer should reflect the data given to it, not carry out any sort of modification of the data.
    // Changing the tile color for the visibility ought to be done elsewhere, and the renderer ought to reflect the changes.

    private void ChangeTileColor(int tileIndex, Color color)
    {
        int TriangleIndex = 0;
        for (int i = 0; i < tileIndex; i++)
        {
            TriangleIndex += TileTriangleCount[i];
        }
        TriangleIndex = TriangleIndex * 3;
        for (int i = 0; i < TileTriangleCount[tileIndex] * 3; i++)
        {
            ChunkColors[TriangleIndex + i] = color;
        }
        dFlagChunk = true;
    }

    private void ResetTileColor(int tileIndex) //Don't like this.
    {
        Color BaseColor;
        TileType tt = BaseTiles.GetTile((ChunkTile[tileIndex])).solidType;
        if (tt == null) { tt = BaseTiles.GetTile((ChunkTile[tileIndex])).floorType; }
        if (tt == null) { BaseColor = Color.clear; } //probably not a good thing to do
        else { BaseColor = tt.TileTypeColor; }
        ChangeTileColor(tileIndex, BaseColor);
    }
    

    //This wasn't ever called and I don't think it would really work anyways so whatever. Keeping it in comment containment hell in case it ends up being useful.
/*
    /// <param name="objTransform">mesh bearing object centered on the tile</param>
    public void ObjectOnChunkClicked(GameObject obj)
    {
        Vector3 position = obj.GetComponent<MeshCollider>().bounds.extents;
        //Some hex math trickery, dont remember source.
        double q = (Math.Sqrt(3) / 3 * position.x - 1.0 / 3 * position.z);
        double r = (2.0 / 3 * position.z);
        HexCoordinates tileLocation = (HexCoordinates.RoundedCubes(q, -q - r, r, position.y / HexMetrics.elevationStep));
        IsoChunkSelected e = new IsoChunkSelected();
        e.Array = BaseTiles;
        e.Chunk = this;
        e.TileCoords = tileLocation;
        OnChunkClicked(e);
    }
    */

    public void ClickedAtCoords(HexCoordinates coords)
    {
        IsoChunkSelected e = new IsoChunkSelected();
        e.Array = BaseTiles;
        e.Chunk = this;
        e.TileCoords = coords;
        OnChunkClicked(e);
    }

    private void OnChunkEntered(IsoChunkSelected e)
    {
        EventHandler<IsoChunkSelected> handler = ChunkOnMouseEnterEvent;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    private void OnChunkClicked(IsoChunkSelected e)
    {
        EventHandler<IsoChunkSelected> handler = ChunkClickEvent;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    private void OnChunkDeleted(IsoChunkDeleted e)
    {
        EventHandler<IsoChunkDeleted> handler = ChunkDeletedEvent;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    private void TileTypeChanged(object obj, EventArgs e)
    {
        dFlagChunk = true;
    }

}

public class IsoChunkSelected : EventArgs
{
    public TileListBase Array;
    public IsolatedChunk Chunk;
    public HexCoordinates TileCoords;
    //public Tile Tile;
}

public class IsoChunkDeleted : EventArgs
{
    public TileListBase Array;
    public IsolatedChunk Chunk;
}

