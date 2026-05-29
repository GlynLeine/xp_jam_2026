Shader "CustomEffects/Volumetric Fog"
{

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType"="Lit"
        }
        LOD 100
        ZWrite Off Cull Off
        Blend Off
        Pass
        {
            Name "FogRenderPass"

            HLSLPROGRAM

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _LIGHT_LAYERS
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
                
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // the input structure (Attributes), and the output structure (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"			
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        
            float4 _Resolution;

            int _StepCount;
            float _Density;
            float4 _Scattering;
            float4 _Absorption;
            float4 _Extinction;
            float _LightScale;
			float _FogFadeHeight;
			float _FogFadeDistance;
			float _MinFogHeight;
			float _MaxMarchDistance;
			Texture2D _JitterTexture;
			float _JitterDistance;
			float2 _JitterTexcoordOffset;
			float _NearFieldAggressiveness;
			
            Texture2D _FogHistoryTexture;
			float _FogHistoryContribution;
			float3 _PrevWorldSpaceCameraPos;
			
            float3 GetRay(float2 texcoord)
            { 
                float3 worldPos = ComputeWorldSpacePosition(texcoord, SampleSceneDepth(texcoord), UNITY_MATRIX_I_VP);
                return normalize(worldPos - _WorldSpaceCameraPos);
            }

			float2 ReprojectCoords(float2 texcoord)
            {
                float3 worldPos = ComputeWorldSpacePosition(texcoord, SampleSceneDepth(texcoord), UNITY_MATRIX_I_VP);
                float3 rayDir = normalize(worldPos - _PrevWorldSpaceCameraPos);
            	float4 hpositionCS = mul(_PrevViewProjMatrix, rayDir * _ProjectionParams.y);
            	hpositionCS.xy = (hpositionCS.xy / hpositionCS.w) * 0.5 + float2(0.5, 0.5);
            	hpositionCS.y = 1.0 - hpositionCS.y;
				return hpositionCS;
            }

            float3 SampleSceneRadiance(float2 texcoord)
            {
                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, texcoord).rgb;
            }

            // Phase function
            // https://www.pbr-book.org/3ed-2018/Volume_Scattering/Phase_Functions
            float HenyeyGreenstein(float g, float cosTheta) {
	            return (1.0 / (4.0 * 3.1415926)) * ((1.0 - g * g) / pow(max(0.0, 1.0 + g * g - 2.0 * g * cosTheta), 1.5));
            }

            struct DensityData
            {
                float extinction;
                float3 scaledExtinction;
                float3 transmittance;
            };

			DensityData GetDensityData(float sampleHeight, float stepSize, float fogFadeStartHeight)
			{
                float densityScale = 1.0 - saturate((sampleHeight - fogFadeStartHeight) / _FogFadeDistance);

			    DensityData data;
                data.extinction = _Density * densityScale;
                data.scaledExtinction = data.extinction * _Extinction.rgb;
			    data.transmittance = exp(-stepSize * data.scaledExtinction);
				return data;
			}

            float GetStepDepth(float sampleTime, float traversalDepth)
            {
				float clampedTime = saturate(sampleTime);
	            float t5 = clampedTime * clampedTime * clampedTime * clampedTime * clampedTime;
	            float tangent = clampedTime * ((1.0 - _NearFieldAggressiveness) + ((_MaxMarchDistance - traversalDepth) / _MaxMarchDistance) * _NearFieldAggressiveness);
	            return lerp(tangent, clampedTime, t5) * traversalDepth;
            }
            
            float3 AdditionalLightInscatter(float3 rayDir, float extinction, Light light)
            {
                float cosTheta = dot(rayDir, normalize(light.direction));
				float phaseFunction = lerp(HenyeyGreenstein(-0.3, cosTheta), HenyeyGreenstein(0.3, cosTheta), 0.7);
                return _Scattering.rgb * light.color * light.distanceAttenuation * light.shadowAttenuation * _LightScale * phaseFunction * extinction;
            }
			
			float3 GetInscatter(float3 samplePoint, float3 rayDir, float mainLightPhaseFunction, float extinction)
			{
				Light light = GetMainLight(TransformWorldToShadowCoord(samplePoint), samplePoint, half4(1, 1, 1, 1));
				float3 inscatter = _Scattering.rgb * light.color * light.shadowAttenuation * _LightScale * mainLightPhaseFunction * extinction;
				
                // Get additional lights
                #if defined(_ADDITIONAL_LIGHTS)
				InputData inputData = (InputData)0;
				inputData.positionWS = samplePoint;
				inputData.normalWS = rayDir;
				inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(samplePoint);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(TransformWorldToHClip(samplePoint));

				// Additional light loop for non-main directional lights. This block is specific to Forward+.
				#if USE_CLUSTER_LIGHT_LOOP
				UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
				{
				    Light additionalLight = GetAdditionalLight(lightIndex, samplePoint, half4(1,1,1,1));
				    inscatter += AdditionalLightInscatter(rayDir, extinction, additionalLight);
				}
				#endif

                // Additional light loop.
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light additionalLight = GetAdditionalLight(lightIndex, samplePoint, half4(1,1,1,1));
                    inscatter += AdditionalLightInscatter(rayDir, extinction, additionalLight);
                LIGHT_LOOP_END
				#endif

				return inscatter;
			}

			float GetJitterDepth(float2 texcoord)
			{
				float2 texcoordScale = _Resolution.zw / float2(256.0, 256.0);
				return SAMPLE_TEXTURE2D(_JitterTexture, sampler_LinearRepeat, texcoord * texcoordScale + _JitterTexcoordOffset).r * _JitterDistance;
			}
			
            float3 MarchRay(float2 texcoord, inout float3 totalTransmittance)
            {
                float sampledDepth = SampleSceneDepth(texcoord);

                float3 rayDir = GetRay(texcoord);

                float cosTheta = dot(rayDir, _MainLightPosition.xyz);
	            float mainLightPhaseFunction = lerp(HenyeyGreenstein(-0.3, cosTheta), HenyeyGreenstein(0.3, cosTheta), 0.7);

	            float3 radiance = float3(0, 0, 0);
                float traversalDepth = min(_MaxMarchDistance, LinearEyeDepth(sampledDepth, _ZBufferParams) - 1.0);
	            float3 rayOrigin = _WorldSpaceCameraPos + rayDir;

				float startDepth = 0.0;
				startDepth += GetJitterDepth(texcoord);
								
				if (abs(rayDir.y) > 0.0001)
				{
					float endHeight = rayDir.y * traversalDepth;
					float targetEndHeight = clamp(rayOrigin.y + endHeight, _MinFogHeight, _FogFadeHeight) - rayOrigin.y;
					traversalDepth += (targetEndHeight - endHeight) / rayDir.y;

	                float targetStartHeight = clamp(rayOrigin.y, _MinFogHeight, _FogFadeHeight);
					
					startDepth += (targetStartHeight - rayOrigin.y) / rayDir.y;
                }

				if (startDepth < 0.0)
				{
					return radiance;
				}
			
				traversalDepth -= startDepth;
                rayOrigin += rayDir * startDepth;
				
				if (traversalDepth < 0.0)
				{
					return radiance;
				}
				
                float fogFadeStartHeight = _FogFadeHeight - _FogFadeDistance;
				float sampleTimeStep = 1.0 / _StepCount;
				float sampleTime = 0.0;
				float previousStepDepth = 0.0;
				
                for(int i = 0; i < _StepCount; i++)
                {
                    sampleTime += sampleTimeStep;
                    float stepDepth = GetStepDepth(sampleTime, traversalDepth);
                    float stepSize = stepDepth - previousStepDepth;
                    previousStepDepth = stepDepth;
                	
                    float3 samplePoint = rayOrigin + rayDir * stepDepth;
                    DensityData densityData = GetDensityData(samplePoint.y, stepSize, fogFadeStartHeight);
                	
                	if (densityData.extinction < 0.0001)
                	{
                		continue;
                	}
                	
			        float3 inscatter = GetInscatter(samplePoint, rayDir, mainLightPhaseFunction, densityData.extinction);

			        radiance += totalTransmittance * ((inscatter - inscatter * densityData.transmittance) / densityData.scaledExtinction);
			        totalTransmittance *= densityData.transmittance;

                    //if(totalTransmittance.r + totalTransmittance.g + totalTransmittance.b < 0.0001)
                    if (totalTransmittance.r < 0.0001)
                    {
	                    totalTransmittance = float3(0, 0, 0);
	                    break;
                    }
                }

                return radiance;
            }
    
            float4 ComputeFog (Varyings input) : SV_Target
            {
                if (_StepCount == 0)
                {
                    return float4(0, 0, 0, asfloat(0xFFFFFFFF));
                }
	
                float3 totalTransmittance = float3(1, 1, 1);

                float3 fogRadiance = MarchRay(input.texcoord, totalTransmittance);

            	float4 fogData = float4(fogRadiance, max(totalTransmittance.r, max(totalTransmittance.g, totalTransmittance.b)));

				float2 reprojectCoords = ReprojectCoords(input.texcoord);
            	if (all(reprojectCoords >= float2(0.0, 0.0) && reprojectCoords <= float2(1.0, 1.0)))
            	{
	                fogData = lerp(fogData, SAMPLE_TEXTURE2D(_FogHistoryTexture, sampler_LinearClamp, reprojectCoords), _FogHistoryContribution);
            	}
            	
                 return fogData;
                // uint transmittanceR = (uint)(totalTransmittance.r * 2047.0 + 0.5) << 21;
                // uint transmittanceG = (uint)(totalTransmittance.g * 2047.0 + 0.5) << 10;
                // uint transmittanceB = (uint)(totalTransmittance.b * 1023.0 + 0.5) << 0;
                //
                // return float4(fogRadiance, asfloat(transmittanceR | transmittanceG | transmittanceB));
            }

            #pragma vertex Vert
            #pragma fragment ComputeFog
            
            ENDHLSL
        }

        Pass
        {
            Name "FogCompositeRenderPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // the input structure (Attributes), and the output structure (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            TEXTURE2D_X_FLOAT(_FogTexture);

            float4 CompositeFog (Varyings input) : SV_Target
            {
                float4 fogData = SAMPLE_TEXTURE2D(_FogTexture, sampler_LinearClamp, input.texcoord);
                float3 transmittance;

                transmittance = fogData.aaa;
                // uint compressedTransmittance = asuint(fogData.a);
                // uint transmittanceR = (compressedTransmittance >> 21) & 0x000007FF;
                // uint transmittanceG = (compressedTransmittance >> 10) & 0x000007FF;
                // uint transmittanceB = (compressedTransmittance >> 0) & 0x000003FF;
                // transmittance.r = transmittanceR / 2047.0;
                // transmittance.g = transmittanceG / 2047.0;
                // transmittance.b = transmittanceB / 1023.0;

                return float4(transmittance * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord).rgb + fogData.rgb, 1);
            }
            
            #pragma vertex Vert
            #pragma fragment CompositeFog
            
            ENDHLSL
        }
    }
}