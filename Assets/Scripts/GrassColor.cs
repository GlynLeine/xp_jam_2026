using System;
using Unity.Mathematics;
using UnityEngine;

public class GrassColor : MonoBehaviour
{
    public Terrain terrain;

    private void OnValidate()
    {
        Start();
    }

    private void Start()
    {
        Shader.SetGlobalTexture("_TerrainMaskTex", terrain.terrainData.GetAlphamapTexture(0));
        
        int layerCount = math.min(terrain.terrainData.terrainLayers.Length, 4);
        
        for (int i = 0; i < layerCount; ++i)
        {
            Shader.SetGlobalTexture($"_TerrainDiffuseTex{i}", terrain.terrainData.terrainLayers[i].diffuseTexture);
            Shader.SetGlobalTexture($"_TerrainMaskMapTex{i}", terrain.terrainData.terrainLayers[i].maskMapTexture);
        }
    }
}
