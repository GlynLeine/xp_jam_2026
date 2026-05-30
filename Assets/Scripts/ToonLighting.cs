using System;
using UnityEngine;

public class ToonLighting : MonoBehaviour
{
    [Range(2f, 5f)]
    public int steps = 1;
    
    [ColorUsage(false, true)]
    public Color ambientLight;

    private void OnValidate()
    {
        Shader.SetGlobalFloat("_Steps", steps - 1);
        Shader.SetGlobalColor("_AmbientLight", ambientLight);
    }

    private void Awake()
    {
        OnValidate();
    }
}
