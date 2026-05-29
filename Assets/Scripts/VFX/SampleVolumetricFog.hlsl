#ifndef SAMPLE_VOLUMETRIC_FOG
#define SAMPLE_VOLUMETRIC_FOG

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

#if !defined(SHADERGRAPH_PREVIEW)
half MainLightRealtimeShadow_SVF(float4 shadowCoord) {
	ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
	half4 shadowParams = GetMainLightShadowParams();
	return SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData, shadowParams, false);
}

half MainLightShadow_SVF(float4 shadowCoord, float3 positionWS, half4 shadowMask, half4 occlusionProbeChannels) {
    half realtimeShadow = MainLightRealtimeShadow_SVF(shadowCoord);

#ifdef CALCULATE_BAKED_SHADOWS
    half bakedShadow = BakedShadow(shadowMask, occlusionProbeChannels);
#else
    half bakedShadow = half(1.0);
#endif

    half shadowFade = GetMainLightShadowFade(positionWS);

    return MixRealtimeAndBakedShadows(realtimeShadow, bakedShadow, shadowFade);
}

int GetAdditionalLightsCount_SVF() {
    // TODO: we need to expose in SRP api an ability for the pipeline cap the amount of lights
    // in the culling. This way we could do the loop branch with an uniform
    // This would be helpful to support baking exceeding lights in SH as well
    return int(min(_AdditionalLightsCount.x, unity_LightData.y));
}

float3 CalcAdditionalLight_SVF(uint lightIndex, float3 worldPos) {
#if USE_FORWARD_PLUS
    int perObjectLightIndex = lightIndex;
#else
    int perObjectLightIndex = GetPerObjectLightIndex(lightIndex);
#endif

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
	float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
	half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
    half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
    half4 occlusionProbeChannels = _AdditionalLightsBuffer[perObjectLightIndex].occlusionProbeChannels;
#else
    float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
    half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes[perObjectLightIndex];
#endif

	float3 lightVector = lightPositionWS.xyz - worldPos * lightPositionWS.w;
	float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));

    float attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

	attenuation *= AdditionalLightShadow(perObjectLightIndex, worldPos, lightDirection, half4(1.0, 1.0, 1.0, 1.0), occlusionProbeChannels);

#if defined(_LIGHT_COOKIES)
    real3 cookieColor = SampleAdditionalLightCookie(perObjectLightIndex, worldPos);
    color *= cookieColor;
#endif

	return color * attenuation;
}

#endif

float3 LightingAtWorldPos_SVF(float3 fogColor, float3 worldPos) {
#if defined(SHADERGRAPH_PREVIEW)
	return fogColor;
#else
	const half4 shadowMask = half4(1.0, 1.0, 1.0, 1.0);

	float3 color = fogColor * MainLightShadow_SVF(TransformWorldToShadowCoord(worldPos), worldPos, shadowMask, _MainLightOcclusionProbes) * _MainLightColor.rgb;

    uint lightCount = GetAdditionalLightsCount();

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

void SampleVolumetricFog_float(float2 uv, float steps, float maxStepDepth, float fogDensity, float3 fogColor, out float3 output) {
    output = SampleSceneColor(uv);

	[branch]
    if (steps == 0.0)
        return;
	
    float depth = SampleSceneDepth(uv);

    float3 sampleWorldPos = ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);

	
    float3 cameraRayDir = sampleWorldPos - _WorldSpaceCameraPos;
    float sceneDepth = length(cameraRayDir);
    cameraRayDir /= sceneDepth;
	
    float3 cameraRayStart = ComputeWorldSpacePosition(uv, 1.0, UNITY_MATRIX_I_VP);
	
    sceneDepth = sceneDepth;

    float actualSteps = steps + 1;

    float stepSize = sceneDepth / actualSteps;
    float stepAlpha = saturate(fogDensity * stepSize);

    for (float i = 1.0; i < actualSteps; i++)
    {
        float3 worldPos = cameraRayStart + (cameraRayDir * (sceneDepth - stepSize * i));

        output += LightingAtWorldPos_SVF(fogColor, worldPos) * stepAlpha;
    }
}

#endif // SAMPLE_VOLUMETRIC_FOG