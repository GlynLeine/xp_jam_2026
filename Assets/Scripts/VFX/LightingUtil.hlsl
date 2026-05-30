#ifndef LIGHTING_UTIL_GLYN
#define LIGHTING_UTIL_GLYN

#if !defined(SHADERGRAPH_PREVIEW)
half MainLightRealtimeShadow_GLU(float4 shadowCoord)
{
    const ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    const half4 shadowParams = GetMainLightShadowParams();
    return SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData, shadowParams, false);
}

half MainLightShadow_GLU(float4 shadowCoord, float3 positionWS, half4 shadowMask, half4 occlusionProbeChannels)
{
    const half realtimeShadow = MainLightRealtimeShadow_GLU(shadowCoord);

    #ifdef CALCULATE_BAKED_SHADOWS
    const half bakedShadow = BakedShadow(shadowMask, occlusionProbeChannels);
    #else
    const half bakedShadow = half(1.0);
    #endif

    const half shadowFade = GetMainLightShadowFade(positionWS);

    return MixRealtimeAndBakedShadows(realtimeShadow, bakedShadow, shadowFade);
}
#endif

struct MainLight_GLU
{
    half3 direction;
    half3 color;
    float distanceAttenuation;
    half  shadowAttenuation;
};

MainLight_GLU GetMainLight_GLU(float3 positionWS, half4 shadowMask)
{
    MainLight_GLU light;
    
#if !defined(SHADERGRAPH_PREVIEW)
    light.direction = half3(_MainLightPosition.xyz);
    light.color = _MainLightColor.rgb;
#else
    light.direction = half3(1.0, 1.0, 1.0);
    light.color = half3(1.0, 1.0, 1.0);
#endif
    
#if USE_CLUSTER_LIGHT_LOOP
    #if defined(LIGHTMAP_ON) && defined(LIGHTMAP_SHADOW_MIXING)
    light.distanceAttenuation = _MainLightColor.a;
    #else
    light.distanceAttenuation = 1.0;
    #endif
#else
#if !defined(SHADERGRAPH_PREVIEW)
    light.distanceAttenuation = unity_LightData.z; // unity_LightData.z is 1 when not culled by the culling mask, otherwise 0.
#else
    light.distanceAttenuation = 1.0;
#endif    
#endif
    
#if defined(SHADERGRAPH_PREVIEW)
    light.shadowAttenuation = 1.0;
#else
    light.shadowAttenuation = MainLightShadow_GLU(TransformWorldToShadowCoord(positionWS), positionWS, shadowMask, _MainLightOcclusionProbes);
#endif
    
    return light;
}

struct AdditionalLight_GLU
{
    float4 positionWS;
    half3 color;
    half4 distanceAndSpotAttenuation;
    half4 spotDirection;
    half3 direction;
    half shadowAttenuation;
    float distanceSqr;
};

#if !defined(SHADERGRAPH_PREVIEW)
AdditionalLight_GLU GetAdditionalLight_GLU(uint lightIndex, float3 positionWS, half4 shadowMask)
{
#if USE_FORWARD_PLUS
    const int perObjectLightIndex = lightIndex;
#else
    const int perObjectLightIndex = GetPerObjectLightIndex(lightIndex);
#endif
    
    AdditionalLight_GLU additionalLight;
        
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    additionalLight.positionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
    additionalLight.color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
    additionalLight.distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
    additionalLight.spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
    const half4 occlusionProbeChannels = _AdditionalLightsBuffer[perObjectLightIndex].occlusionProbeChannels;
#else
    additionalLight.positionWS = _AdditionalLightsPosition[perObjectLightIndex];
    additionalLight.color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    additionalLight.distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    additionalLight.spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
    const half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes[perObjectLightIndex];
#endif
    
    const float3 lightVector = additionalLight.positionWS.xyz - positionWS * additionalLight.positionWS.w;
    additionalLight.distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);
    additionalLight.direction = half3(lightVector * rsqrt(additionalLight.distanceSqr));
    
    additionalLight.shadowAttenuation = AdditionalLightShadow(perObjectLightIndex, positionWS, additionalLight.direction, shadowMask, occlusionProbeChannels);
    
#if defined(_LIGHT_COOKIES)
    const real3 cookieColor = SampleAdditionalLightCookie(perObjectLightIndex, positionWS);
    additionalLight.color *= cookieColor;
#endif
    
    return additionalLight;
}
#endif
#endif // LIGHTING_UTIL_GLYN
