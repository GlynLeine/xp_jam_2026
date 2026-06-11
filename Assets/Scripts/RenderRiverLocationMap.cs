using UnityEngine;
#if UNITY_EDITOR
using Unity.Collections;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using File = System.IO.File;
using Object = UnityEngine.Object;
#endif

public class RenderRiverLocationMap : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Render River Location Texture")]
    public ComputeShader jumpFloodShader;
    public Material riverLocationMaterial;
    public TerrainData terrainData;
    public Object textureAsset;
    
    public bool isBusyRendering = false;
    
    private void OnValidate()
    {
        if(textureAsset == null || terrainData == null || riverLocationMaterial == null || jumpFloodShader == null)
        {
            return;
        }

        if (isBusyRendering)
        {
            return;
        }

        isBusyRendering = true;
        string outputPath = AssetDatabase.GetAssetPath(textureAsset);
        textureAsset = null;
        int riverLocationResolution = 256;
        
        RenderTexture seedTexture = new RenderTexture(new RenderTextureDescriptor(riverLocationResolution, riverLocationResolution, GraphicsFormat.R8G8_UNorm, 0, 1)){enableRandomWrite = true};
        RenderTexture jumpFloodBuffer = new RenderTexture(new RenderTextureDescriptor(riverLocationResolution, riverLocationResolution, GraphicsFormat.R32G32_SFloat, 0, 1)){enableRandomWrite = true};
        RenderTexture riverLocationTexture = new RenderTexture(new RenderTextureDescriptor(riverLocationResolution, riverLocationResolution, GraphicsFormat.R8G8_UNorm, 0, 1)){enableRandomWrite = true};
        
        NativeArray<byte> buffer = new NativeArray<byte>(riverLocationResolution * riverLocationResolution * 8, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        Debug.Log($"Creating River Location Texture at: \"{outputPath}\"");
                
        Debug.Log("Rendering River Location Texture");
        RenderTexture activeRenderTexture = RenderTexture.active;
        Graphics.Blit(terrainData.heightmapTexture, seedTexture, riverLocationMaterial);
        RenderTexture.active = activeRenderTexture;
        
        jumpFloodShader.SetTexture(0, "seeds", seedTexture);
        jumpFloodShader.SetTexture(0, "jumpFloodBuffer", jumpFloodBuffer);
        jumpFloodShader.SetTexture(0, "result", riverLocationTexture);
        jumpFloodShader.SetInt("jumpSteps", 8);
        jumpFloodShader.SetBool("initialize", true);
        jumpFloodShader.Dispatch(0, 16, 16, 1);
        
        jumpFloodShader.SetTexture(0, "seeds", seedTexture);
        jumpFloodShader.SetTexture(0, "jumpFloodBuffer", jumpFloodBuffer);
        jumpFloodShader.SetTexture(0, "result", riverLocationTexture);
        jumpFloodShader.SetInt("jumpSteps", 8);
        jumpFloodShader.SetBool("initialize", false);
        jumpFloodShader.Dispatch(0, 16, 16, 1);

        Debug.Log("Reading back River Location Texture");
        AsyncGPUReadback.RequestIntoNativeArray(ref buffer, riverLocationTexture, 0, (AsyncGPUReadbackRequest request) => {
            if (request.hasError)
            {
                Debug.Log("GPU readback error detected.");
                return;
            }
            
            Debug.Log("Encoding River Location Texture");
            var encoded = ImageConversion.EncodeNativeArrayToPNG(buffer, GraphicsFormat.R8G8_UNorm,
                (uint)riverLocationResolution, (uint)riverLocationResolution);
            Debug.Log("Writing River Location Texture");
            File.WriteAllBytes(outputPath, encoded.ToArray());

            DestroyImmediate(seedTexture);
            DestroyImmediate(riverLocationTexture);
            buffer.Dispose();
            
            AssetDatabase.ImportAsset(outputPath);
            
            Debug.Log("Finished River Location Texture!");
            isBusyRendering = false;
        });
    }
#endif
}
