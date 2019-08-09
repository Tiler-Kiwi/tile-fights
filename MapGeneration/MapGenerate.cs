using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

    public class MapGenerate : MonoBehaviour
    {
    System.Random rng = new System.Random();

    [SerializeField]
    TileListBase BaseList;
    [SerializeField]
    List<TileType> TileTypes;
    [SerializeField]
    //List<Feature> Features;
    //[SerializeField]
    MapGraphData MapData;
    [SerializeField]

    public Texture2D Fallback;
    public bool UseFallBack = false;

    private void Start()
    {
        MapData = MapGraphData.Instantiate(MapData);
    }

    public TileListBase GetABasicMap(int XDim,int ZDim, int YDim, TileType terrain, TileType terrainB, Tile tilePrefab)
    {
        //TileListBase NewTileList = new TileListBase(XDim, ZDim, YDim);
        List<double> DiffScore = new List<double>();
        double XStep = 1 / (double)XDim;
        double ZStep = 1 / (double)ZDim;
        double YStep = 1 / (double)YDim;
        MapData.GenerateRandomPoints(6);
        for (double z = 0; z < ZDim; z++)
        {
            for(double x = 0; x < XDim; x++)
            {
                if (!UseFallBack)
                {
                    float result = MapData.Interpolate((float)(x * XStep), (float)(z * ZStep));
                    DiffScore.Add(result);
                }
                else
                {
                    DiffScore.Add(Fallback.GetPixel((int)x, (int)z).grayscale);
                }
            }
        }
        return NaiveRandom(DiffScore, BaseList, tilePrefab, terrain, terrainB);
    }

    private TileListBase HardCutOff(List<double> diffScore, TileListBase clearTileList, Tile tilePrefab, TileType terrain, TileType terrainB)
    {
        double Cutoff = .5;
        int XDim = clearTileList.XDim;
        int YDim = clearTileList.YDim;
        int ZDim = clearTileList.ZDim;
        int YLevel = (int)YDim / 2;

        for (int z = 0; z < ZDim; z++)
        {
            for (int x = 0; x < XDim; x++)
            {
                Tile newTile = new Tile();
                if (diffScore[x + z * XDim] > Cutoff)
                {
                    newTile.AssignSolidType(terrainB);
                    newTile.AssignFloorType(terrainB);
                }
                else
                {
                    newTile.AssignSolidType(terrain);
                    newTile.AssignFloorType(terrain);
                }
                clearTileList.SetTile(new Vector3(x, YLevel, z), newTile);
            }
        }
        return clearTileList;
    }

    private TileListBase NaiveRandom(List<double> diffScore, TileListBase clearTileList, Tile tilePrefab, TileType terrain, TileType terrainB)
    {
        double Ceiling = 1;
        double Floor = .50;
        int XDim = clearTileList.XDim;
        int YDim = clearTileList.YDim;
        int ZDim = clearTileList.ZDim;
        double XStep = 1 / (double)XDim;
        double ZStep = 1 / (double)ZDim;
        double YStep = 1 / (double)YDim;
        int YLevel = (int)YDim / 2;

        for(int z=0;z<ZDim;z++)
        {
            for(int x=0;x<XDim;x++)
            {
                Tile newTile = new Tile();
                double Check = diffScore[x + z * XDim];
                if (Check < Floor)
                {
                    newTile.AssignSolidType(null);
                    newTile.AssignFloorType(Instantiate(terrain));
                }
                else if (Check > Ceiling)
                {
                    newTile.AssignSolidType(null);
                    newTile.AssignFloorType(Instantiate(terrainB));
                }
                else
                {
                    Check = Check - Floor;
                    Check = Check / Ceiling;
                    if(Check > rng.NextDouble())
                    {
                        newTile.AssignSolidType(null);
                        newTile.AssignFloorType(Instantiate(terrainB));
                    }
                    else
                    {
                        newTile.AssignSolidType(null);
                        newTile.AssignFloorType(Instantiate(terrain));
                    }
                }
                double pNoise = Mathf.PerlinNoise(.13f+(float)(x*XStep),.13f+(float)(z*ZStep));
                //Debug.Log(pNoise);
                clearTileList.SetTile(new Vector3(x, (int)(pNoise*YDim), z), newTile);
            }
        }
        return clearTileList;
    }
    private TileListBase DoDithering(List<double> diffScore, TileListBase clearTileList, Tile tilePrefab, TileType terrain, TileType terrainB)
    {
        int XDim = clearTileList.XDim;
        int YDim = clearTileList.YDim;
        int ZDim = clearTileList.ZDim;
        double GreaterThan = .5;
        double oldpixel;
        double newpixel;
        double quant_error;
        int YLevel = (int)YDim / 2;
        for (int z = 0; z < ZDim; z++)
        {
            for (int x = 0; x < XDim; x++)
            {
                oldpixel = diffScore[x + z * XDim];
                newpixel = (oldpixel > GreaterThan ) ? 1 : 0;
                Tile newTile = new Tile();
                if (newpixel == 1)
                {
                    newTile.AssignSolidType(Instantiate(terrain));
                    newTile.AssignFloorType(Instantiate(terrain));
                }
                if (newpixel == 0)
                {
                    newTile.AssignSolidType(Instantiate(terrainB));
                    newTile.AssignFloorType(Instantiate(terrainB));
                }
                clearTileList.SetTile(new Vector3(x, YLevel, z), newTile);
                quant_error = oldpixel - newpixel;
                if (x + z * XDim + 1 > diffScore.Count - 1) { continue; }
                diffScore[x + z * XDim + 1] = diffScore[x + z * XDim + 1] + quant_error * 7 / 16;
                if (x + (1 + z) * XDim - 1 > diffScore.Count - 1) { continue; }
                diffScore[x + (1 + z) * XDim - 1] = diffScore[x + (1 + z) * XDim - 1] + quant_error * 3 / 16;
                if (x + (1 + z) * XDim > diffScore.Count - 1) { continue; }
                diffScore[x + (1 + z) * XDim] = diffScore[x + (1 + z) * XDim] + quant_error * 5 / 16;
                if (x + (1 + z) * XDim + 1 > diffScore.Count - 1) { continue; }
                diffScore[x + (1 + z) * XDim + 1] = diffScore[x + (1 + z) * XDim + 1] + quant_error * 1 / 16;
            }
        }
        return clearTileList;
    }

    public void AddPoint(float x, float z)
    {
        MapData.AddPoint(x, z);
    }
    public void AddPoint(float x, float z, float diff)
    {
        MapData.AddPoint(x, z,diff);
    }

    
}

