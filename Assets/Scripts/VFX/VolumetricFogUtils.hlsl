#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

float3 SampleWorldPos(float2 texcoords)
{    
    return ComputeWorldSpacePosition(texcoords, SampleSceneDepth(texcoords), UNITY_MATRIX_I_VP);
}

//uint2 ToScreenSpacePos(float3 viewSpace)
//{
//    float4 result = mul(UNITY_MATRIX_P, float4(viewSpace, 1.0));
//    result.xy = result.xy / result.w;
//#if UNITY_UV_STARTS_AT_TOP
//    result.y = -result.y;
//#endif
//    float2 screenScale = 0.5 / _CameraOpaqueTexture_TexelSize.xy;
    
//    return (uint2) round(result.xy * screenScale + screenScale);
//}

//float2 ToSceneColorUV(uint2 sceneColorTexel)
//{
//    return sceneColorTexel * _CameraOpaqueTexture_TexelSize.xy;
//}

//uint2 RoundParam(float2 val)
//{
//    float2 dirSign = sign(val);
//    return (uint2) (dirSign * floor(dirSign * val) + dirSign);
//}

void CalcVolumetricFog_float(float2 UV, float3 rayOrigin, float3 rayDir, out float3 color, out float alpha)
{
    float3 rayPos = rayOrigin;
    float dist = 0;
    float depth = length(SampleWorldPos(UV) - rayOrigin);
    
    color = float3(0.0, 0.0, 0.0);
    
    for (int i = 0; i < 100; i++)
    {
        dist += 0.5;
        rayPos += rayDir * 0.5;
        
        Light mainLight = GetMainLight(TransformWorldToShadowCoord(rayPos), rayPos, half4(1, 1, 1, 1));
        
        color += mainLight.color * mainLight.shadowAttenuation;
        
        if (dist > depth)
        {
            rayPos += (depth - dist) * rayDir;
            break;
        }
    }
    
    color /= 100.0;
    
    alpha = saturate(depth * 0.01);
}