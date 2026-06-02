using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class VolumetricFogSettings
{
    [Range(0f, 1f)]
    public float resolutionScale = 0.25f;
    [Min(0)]
    public int stepCount = 100;
    [Min(0)]
    public float fogDensity = 0.1f;
    public Color scatteringCoefficients = Color.white;
    public Color absorptionCoefficients = Color.black;
    [Min(0)]
    public float lightScale = 1f;
    public float fadeHeight = 5f;
    [Min(0)]
    public float fadeDistance = 4f;
    public float minFogHeight = 0f;
    [Min(0)]
    public float maxMarchDistance = 100f;
    [Min(0)]
    public float jitterDistance = 0.1f;
    [Range(0f, 1f)]
    public float nearFieldAggressiveness = 0.8f;
    [Range(0f, 1f)]
    public float fogHistoryContribution = 0.2f;
    public List<Texture> blueNoiseTextures;
}

public class VolumetricFogRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private VolumetricFogSettings settings;
    [SerializeField] private Shader shader;
    
    private Material material;
    private VolumetricFogRenderPass renderPass;

    public static Mesh s_TriangleMesh;
    public static Mesh s_QuadMesh;

    public override void Create()
    {
        if (shader == null)
        {
            return;
        }

        material = new Material(shader);
        renderPass = new VolumetricFogRenderPass(material, settings);

        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        float nearClipZ = -1;
        if (SystemInfo.usesReversedZBuffer)
            nearClipZ = 1;
        if (SystemInfo.graphicsShaderLevel < 30)
        {
            if (!s_TriangleMesh)
            {
                s_TriangleMesh = new Mesh();
                s_TriangleMesh.vertices = GetFullScreenTriangleVertexPosition(nearClipZ);
                s_TriangleMesh.uv = GetFullScreenTriangleTexCoord();
                s_TriangleMesh.triangles = new int[3] { 0, 1, 2 };
            }
        }
        if (!s_QuadMesh)
        {
            s_QuadMesh = new Mesh();
            s_QuadMesh.vertices = GetQuadVertexPosition(nearClipZ);
            s_QuadMesh.uv = GetQuadTexCoord();
            s_QuadMesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
        }

        // Should match Common.hlsl
        static Vector3[] GetFullScreenTriangleVertexPosition(float z /*= UNITY_NEAR_CLIP_VALUE*/)
        {
            var r = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {
                Vector2 uv = new Vector2((i << 1) & 2, i & 2);
                r[i] = new Vector3(uv.x * 2.0f - 1.0f, uv.y * 2.0f - 1.0f, z);
            }
            return r;
        }

        // Should match Common.hlsl
        static Vector2[] GetFullScreenTriangleTexCoord()
        {
            var r = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                if (SystemInfo.graphicsUVStartsAtTop)
                    r[i] = new Vector2((i << 1) & 2, 1.0f - (i & 2));
                else
                    r[i] = new Vector2((i << 1) & 2, i & 2);
            }
            return r;
        }

        // Should match Common.hlsl
        static Vector3[] GetQuadVertexPosition(float z /*= UNITY_NEAR_CLIP_VALUE*/)
        {
            var r = new Vector3[4];
            for (uint i = 0; i < 4; i++)
            {
                uint topBit = i >> 1;
                uint botBit = (i & 1);
                float x = topBit;
                float y = 1 - (topBit + botBit) & 1; // produces 1 for indices 0,3 and 0 for 1,2
                r[i] = new Vector3(x, y, z);
            }
            return r;
        }

        // Should match Common.hlsl
        static Vector2[] GetQuadTexCoord()
        {
            var r = new Vector2[4];
            for (uint i = 0; i < 4; i++)
            {
                uint topBit = i >> 1;
                uint botBit = (i & 1);
                float u = topBit;
                float v = (topBit + botBit) & 1; // produces 0 for indices 0,3 and 1 for 1,2
                if (SystemInfo.graphicsUVStartsAtTop)
                    v = 1.0f - v;

                r[i] = new Vector2(u, v);
            }
            return r;
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderPass == null)
        {
            return;
        }

        if (renderingData.cameraData.cameraType is CameraType.Game or CameraType.SceneView)
        {
            renderer.EnqueuePass(renderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }

        CoreUtils.Destroy(s_TriangleMesh);
        s_TriangleMesh = null;
        CoreUtils.Destroy(s_QuadMesh);
        s_QuadMesh = null;
    }
}
