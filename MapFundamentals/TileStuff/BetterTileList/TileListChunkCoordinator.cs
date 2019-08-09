using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// Creates and manages chunks used to render the tile list.
/// </summary>
//[RequireComponent(typeof(TileListBase))]
public class TileListChunkCoordinator : MonoBehaviour
{
    const int CHUNK_HEIGHT = 10;
    const int CHUNK_WIDTH = 10;
    const int CHUNK_DEPTH = 10;
    const decimal divbyCW = 1 / (decimal)CHUNK_WIDTH;
    const decimal divbyCH = 1 / (decimal)CHUNK_HEIGHT;
    const decimal divbyCD = 1 / (decimal)CHUNK_DEPTH;

    decimal Wc;
    decimal Hc;
    decimal Dc;

    [SerializeField]
    List<IsolatedChunk> ChunkList;

    [SerializeField]
    IsolatedChunk ChunkPrefab;

    TileListBase BaseTiles;
    private void Start()
    {
        //BaseTiles = GetComponent<TileListBase>();
    }

    public void AssignList(TileListBase tileList)
    {
        BaseTiles = tileList;
        Wc = Math.Ceiling(BaseTiles.XDim * divbyCW);
        Hc = Math.Ceiling(BaseTiles.ZDim * divbyCH);
        Dc = Math.Ceiling(BaseTiles.YDim * divbyCD);
        GenerateChunks();
    }

    public void ForceRedraw() //stupid
    {
        for (int i = 0; i < ChunkList.Count; i++)
        {
            ChunkList[i].dFlagChunk = true;
        }
    }

    /*
    public void HighlightTiles(List<HexCoordinates> coords, bool toggle)
    {
        TileHighlight Highlighter = this.GetComponent<TileHighlight>();
        if (Highlighter == null) { return; }

        if (toggle)
        {
            Highlighter.AssignHighlights(coords);
            Highlighter.HighlightToggle(true);
        }
        else
        {
            Highlighter.HighlightToggle(false);
        }
    }
    */

    IsolatedChunk GetChunkThatOwnsTile(int tileIndex)
    {
        Vector3 Offsets = BaseTiles.IndexToOffsets(tileIndex);
        return GetChunkThatOwnsTile(Offsets);
    }
    IsolatedChunk GetChunkThatOwnsTile(HexCoordinates coords)
    {
        Vector3 Offsets = BaseTiles.CoordsToOffsets(coords);
        return GetChunkThatOwnsTile(Offsets);
    }
    IsolatedChunk GetChunkThatOwnsTile(Vector3 offsets)
    {
        int ChunkIndex = (int)(Math.Floor((decimal)offsets.x * divbyCW) + Wc * (Math.Floor((decimal)offsets.z * divbyCH) + Hc * Math.Floor((decimal)offsets.y * divbyCD)));
        if (ChunkIndex < 0 || ChunkIndex >= ChunkList.Count) { throw new Exception("Something fucked up! " + ChunkIndex.ToString() + offsets.ToString()); }
        return ChunkList[ChunkIndex];
    }

    private void GenerateChunks()
    {
        decimal ChunkCount = Wc * Hc * Dc;

        ChunkList = new List<IsolatedChunk>();
        List<List<HexCoordinates>> toBeAssignedToChunk = new List<List<HexCoordinates>>();
        for (int i = 0; i < ChunkCount; i++)
        {
            toBeAssignedToChunk.Add(new List<HexCoordinates>());
        }

        for (decimal y = 0; y < BaseTiles.YDim; y++)
        {
            for (decimal z = 0; z < BaseTiles.ZDim; z++)
            {
                for (decimal x = 0; x < BaseTiles.XDim; x++)
                {
                    decimal index = x + BaseTiles.XDim * (z + BaseTiles.ZDim * y);
                    decimal ChunkIndex = Math.Floor(x * divbyCW) + Wc * (Math.Floor(z * divbyCH) + Hc * Math.Floor(y * divbyCD)); //this fucking sucks
                    
                    toBeAssignedToChunk[(int)ChunkIndex].Add(BaseTiles.IndexToCoords((int)index));
                }
            }
        }
        for (int i = 0; i < toBeAssignedToChunk.Count; i++)
        {
            if (toBeAssignedToChunk[i].Count == 0) { continue; }
            IsolatedChunk newChunk = IsolatedChunk.Instantiate<IsolatedChunk>(ChunkPrefab);
            newChunk.name = "Chunk " + i;
            newChunk.transform.SetParent(this.transform, false);
            ChunkList.Add(newChunk);
            ChunkList[ChunkList.Count - 1].AssignTiles(toBeAssignedToChunk[i], BaseTiles);
        }
    }
}
