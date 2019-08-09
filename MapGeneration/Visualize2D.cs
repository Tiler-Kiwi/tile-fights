using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;
using CSharpIDW;

public class Visualize2D : MonoBehaviour {

    public MapGraphData _MapData;
    private Texture2D _Texture;
    public double Resolution = 256;

    private double _StepSize;



    private void Start()
    {
        _StepSize = 1 / Resolution;
        _MapData.FuckYou();
        _MapData.GenerateRandomPoints(6);
        _Texture = new Texture2D((int)Resolution, (int)Resolution, TextureFormat.RGB24, true);
        _Texture.name = "MapGen Visualize Texture";
        GetComponent<MeshRenderer>().material.mainTexture = _Texture;
        FillTexture();
        _Texture.filterMode = FilterMode.Point;
        _Texture.wrapMode = TextureWrapMode.Clamp;
    }

    public void NewRandom()
    {
        _MapData.GenerateRandomPoints(6);
        FillTexture();
    }

    private void FillTexture()
    {
        for (double z = 0; z < Resolution; z++)
        {
            //Debug.Log(z * _StepSize);
            for (double x = 0; x < Resolution; x++)
            {
                float result = _MapData.Interpolate((float)(x*_StepSize),(float)(z*_StepSize));

                //Debug.Log(result.Value);
                _Texture.SetPixel((int)x, (int)z, new Color((float)result, (float)(1.0d-result),0f));
            }
        }
        _Texture.Apply();
    }

    private void PixelColor(float x, float z)
    {
        _MapData.Interpolate(x, z);
    }
}
