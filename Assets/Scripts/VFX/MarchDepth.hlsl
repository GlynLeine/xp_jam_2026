#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

float SampleDepth(uint2 texcoords)
{
    float2 colorScale = 0.5 / _CameraOpaqueTexture_TexelSize.xy;
    float2 depthScale = 0.5 / _CameraDepthTexture_TexelSize.xy;
    
    texcoords = (uint2) round((texcoords - colorScale) / colorScale * depthScale + depthScale);
    
    return LinearEyeDepth(LoadSceneDepth(texcoords), _ZBufferParams) * _ProjectionParams.x;
}

uint2 ToScreenSpacePos(float3 viewSpace)
{
    float4 result = mul(UNITY_MATRIX_P, float4(viewSpace, 1.0));
    result.xy = result.xy / result.w;
#if UNITY_UV_STARTS_AT_TOP
    result.y = -result.y;
#endif
    float2 screenScale = 0.5 / _CameraOpaqueTexture_TexelSize.xy;
    
    return (uint2) round(result.xy * screenScale + screenScale);
}

float2 ToSceneColorUV(uint2 sceneColorTexel)
{
    return sceneColorTexel * _CameraOpaqueTexture_TexelSize.xy;
}

uint2 RoundParam(float2 val)
{
    float2 dirSign = sign(val);
    return (uint2) (dirSign * floor(dirSign * val) + dirSign);
}

uint3 MarchDepth(float3 rayOrigin, float3 rayDir, float comparisonSign)
{
    const float distanceBias = 0.05;
    const float marchDist = 20.0;
    const float stepSize = 0.2;
    const float backtrackDist = stepSize;
    const float backtrackStepSize = backtrackDist * 0.1;
    const int iterationCount = (int) round(marchDist / stepSize);
    
    uint2 currentTexel;
    float3 rayStep = rayDir * stepSize;
    float3 rayPos = rayOrigin;
    float depthDist;
    
    int i = 0;
    for (; i < iterationCount; i++)
    {
        currentTexel = ToScreenSpacePos(rayPos);
        float sampleDepth = SampleDepth(currentTexel);
        
        depthDist = (rayPos.z - sampleDepth) * comparisonSign;
        if (abs(depthDist) < distanceBias)
        {
            return uint3(currentTexel, 1);
        }
        
        if (depthDist < 0.0)
        {
            break;
        }
        
        rayPos += rayStep;
        rayStep *= 1.05;
    }
    
    for (; i < iterationCount; i++)
    {
        rayStep *= 0.5;
        rayPos += rayStep * sign(depthDist);
			
        currentTexel = ToScreenSpacePos(rayPos);
        float sampleDepth = SampleDepth(currentTexel);
        depthDist = (rayPos.z - sampleDepth) * comparisonSign;
			
        if (abs(depthDist) < distanceBias)
        {
            return uint3(currentTexel, 1);
        }
    }
    
    return uint3(currentTexel, 0);
}