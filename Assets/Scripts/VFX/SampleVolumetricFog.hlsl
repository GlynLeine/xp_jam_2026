#ifndef SAMPLE_VOLUMETRIC_FOG
#define SAMPLE_VOLUMETRIC_FOG

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

#include "LightingUtil.hlsl"

#if !defined(SHADERGRAPH_PREVIEW)
float3 CalcAdditionalLight_SVF(uint lightIndex, float3 worldPos)
{
#if USE_FORWARD_PLUS
    const int perObjectLightIndex = lightIndex;
#else
    const int perObjectLightIndex = GetPerObjectLightIndex(lightIndex);
#endif

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
	const float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
	half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
    const half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
    const half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
    const half4 occlusionProbeChannels = _AdditionalLightsBuffer[perObjectLightIndex].occlusionProbeChannels;
#else
    const float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    const half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    const half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
    const half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes[perObjectLightIndex];
#endif

	const float3 lightVector = lightPositionWS.xyz - worldPos * lightPositionWS.w;
	const float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    const half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));

    float attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

	attenuation *= AdditionalLightShadow(perObjectLightIndex, worldPos, lightDirection, half4(1.0, 1.0, 1.0, 1.0), occlusionProbeChannels);

#if defined(_LIGHT_COOKIES)
    const real3 cookieColor = SampleAdditionalLightCookie(perObjectLightIndex, worldPos);
    color *= cookieColor;
#endif

	return color * attenuation;
}

#endif

float3 LightingAtWorldPos_SVF(float3 fogColor, float3 worldPos)
{
#if defined(SHADERGRAPH_PREVIEW)
	return fogColor;
#else
	const half4 shadowMask = half4(1.0, 1.0, 1.0, 1.0);

	float3 color = fogColor * MainLightShadow_GLU(TransformWorldToShadowCoord(worldPos), worldPos, shadowMask, _MainLightOcclusionProbes) * _MainLightColor.rgb;

    const uint lightCount = GetAdditionalLightsCount();

#if USE_FORWARD_PLUS
	for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        color += fogColor * CalcAdditionalLight_SVF(lightIndex, worldPos);
    }

	{
		uint lightIndex;
		ClusterIterator _urp_internal_clusterIterator = ClusterInit(GetNormalizedScreenSpaceUV(TransformWorldToHClip(worldPos)), worldPos, 0);
		[loop] while (ClusterNext(_urp_internal_clusterIterator, lightIndex)) {
			lightIndex += URP_FP_DIRECTIONAL_LIGHTS_COUNT;
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

#elif !_USE_WEBGL1_LIGHTS
    for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex) {
#else
    // WebGL 1 doesn't support variable for loop conditions
    for (int lightIndex = 0; lightIndex < _WEBGL1_MAX_LIGHTS; ++lightIndex) {
        if (lightIndex >= (int)lightCount) break;
#endif

        color += fogColor * CalcAdditionalLight_SVF(lightIndex, worldPos);
	
#if USE_FORWARD_PLUS
		}
#endif
	}

	return color;
#endif
}

void SampleVolumetricFog_float(float2 uv, float steps, float maxStepDepth, float fogDensity, float3 fogColor, out float3 output)
{
    output = SampleSceneColor(uv);

	[branch]
    if (steps == 0.0)
    {
	    return;
    }
	
    const float depth = SampleSceneDepth(uv);

    const float3 sampleWorldPos = ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
	
    float3 cameraRayDir = sampleWorldPos - _WorldSpaceCameraPos;
    const float sceneDepth = length(cameraRayDir);
    cameraRayDir /= sceneDepth;
	
    const float3 cameraRayStart = ComputeWorldSpacePosition(uv, 1.0, UNITY_MATRIX_I_VP);
	
    const float actualSteps = steps + 1;

    const float stepSize = sceneDepth / actualSteps;
    const float stepAlpha = saturate(fogDensity * stepSize);

    for (float i = 1.0; i < actualSteps; i++)
    {
        const float3 worldPos = cameraRayStart + (cameraRayDir * (sceneDepth - stepSize * i));

        output += LightingAtWorldPos_SVF(fogColor, worldPos) * stepAlpha;
    }
}

#endif // SAMPLE_VOLUMETRIC_FOG