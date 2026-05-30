#ifndef SAMPLE_TOON_LIGHT
#define SAMPLE_TOON_LIGHT

#include "Assets/Scripts/VFX/LightingUtil.hlsl"

float QuantizeAttenuation(float attenuation, float steps)
{
	const float stepSize = rcp(steps);
	return ceil(saturate(attenuation) * steps) * stepSize;
}

float3 LightingToonSpecular(float3 lightColor, float lightAttenuation, float3 lightDir, float3 normal, float3 viewDir, half smoothness)
{
	float3 halfVec = SafeNormalize(lightDir + viewDir);
	float NdotH = float(saturate(dot(normal, halfVec)));
	float modifier = pow(NdotH, exp2(10 * smoothness + 1)) * lightAttenuation; // Half produces banding, need full precision
	// NOTE: In order to fix internal compiler error on mobile platforms, this needs to be float3
	float3 specularReflection = (modifier > 0.001f).xxx * smoothness * 2.0;
	return lightColor * specularReflection;
}

float3 SampleMainLight(float3 positionWS, float3 normalWS, float3 viewDirectionWS, float smoothness, float steps, half4 shadowMask)
{
	const MainLight_GLU light = GetMainLight_GLU(positionWS, shadowMask);
	const float attenuation = saturate(dot(normalWS, light.direction)) * light.shadowAttenuation;
	const float3 diffuse = light.color * light.distanceAttenuation * QuantizeAttenuation(attenuation, steps);
	
	const float3 specular = LightingToonSpecular(light.color, light.distanceAttenuation * attenuation, light.direction, normalWS, viewDirectionWS, smoothness);
	return diffuse + specular;
}

#if !defined(SHADERGRAPH_PREVIEW)
float3 SampleAdditionalLight(uint lightIndex, float3 positionWS, float3 normalWS, float3 viewDirectionWS, float smoothness, float steps, half4 shadowMask)
{
	const AdditionalLight_GLU light = GetAdditionalLight_GLU(lightIndex, positionWS, shadowMask);
	const float attenuation = saturate(dot(normalWS, light.direction)) *
						light.shadowAttenuation *
						DistanceAttenuation(light.distanceSqr, light.distanceAndSpotAttenuation.xy) *
						AngleAttenuation(light.spotDirection.xyz, light.direction, light.distanceAndSpotAttenuation.zw);
	const float3 diffuse = light.color * QuantizeAttenuation(attenuation, steps);
	
	const float3 specular = LightingToonSpecular(light.color, attenuation, light.direction, normalWS, viewDirectionWS, smoothness);
	
	return diffuse + specular;
}
#endif

void SampleToonLight_float(float2 normalizedScreenSpaceUV, float3 positionWS, float3 normalWS, float smoothness, float ambientOcclusion, float3 ambientLight, float steps, out float3 output)
{
	const half4 shadowMask = half4(1.0, 1.0, 1.0, 1.0);
	const float3 viewDirectionWS = (float3)GetWorldSpaceNormalizeViewDir(positionWS);
	
	output = SampleMainLight(positionWS, normalWS, viewDirectionWS, smoothness, steps, shadowMask);
	
#if !defined(SHADERGRAPH_PREVIEW)	
	const AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(normalizedScreenSpaceUV, (half)ambientOcclusion);
	output += ambientLight * aoFactor.indirectAmbientOcclusion;
	
	uint lightCount = GetAdditionalLightsCount();

#if USE_FORWARD_PLUS
	for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
	{
		FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

		output += SampleAdditionalLight(lightIndex, positionWS, normalWS, viewDirectionWS, smoothness, steps, shadowMask);
	}

	{
		uint lightIndex;
		ClusterIterator _urp_internal_clusterIterator = ClusterInit(GetNormalizedScreenSpaceUV(TransformWorldToHClip(worldPos)), worldPos, 0);
		[loop] while (ClusterNext(_urp_internal_clusterIterator, lightIndex)) {
			lightIndex += URP_FP_DIRECTIONAL_LIGHTS_COUNT;
			FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

#elif !_USE_WEBGL1_LIGHTS
	for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex)
	{
#else
	// WebGL 1 doesn't support variable for loop conditions
	for (int lightIndex = 0; lightIndex < _WEBGL1_MAX_LIGHTS; ++lightIndex) {
		if (lightIndex >= (int)lightCount) break;
#endif

		output += SampleAdditionalLight(lightIndex, positionWS, normalWS, viewDirectionWS, smoothness, steps, shadowMask);
	
#if USE_FORWARD_PLUS
		}
#endif
	}
#else
	output += ambientLight * ambientOcclusion;
#endif
}

#endif // SAMPLE_TOON_LIGHT