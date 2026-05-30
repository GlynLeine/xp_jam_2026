using System;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class DayTime : MonoBehaviour
{
    [Min(0.1f)] public float summerDayTimeDuration = 240f;
    public float winterDayTimeDuration = 150f;
    public float winterDawnTime = 32f;
    public float unnormalizedTime;

    public float season = 1f;

    public float debugTimeScale = 1f;

    public float seasonInterpolator
    {
        get
        {
            float i = 0.5f + (season * 0.5f);
            if (i > 1f)
            {
                i = 1f - (i - 1f);
            }

            return i;
        }
    }

    public float dawnTime => math.lerp(winterDawnTime, 0f, seasonInterpolator);
    public float dawnOffsetTime => unnormalizedTime - dawnTime;
    public float dayTimeDuration => math.lerp(winterDayTimeDuration, summerDayTimeDuration, seasonInterpolator);

    [Header("Morning fog")] public FogSettings fogSettings;
    public float morningDensity;
    public float normalDensity;
    public float morningFogEndTime;

    private Light m_light;
    private quaternion m_summerZenithOrientation;
    private quaternion m_dawnOrientation;
    private quaternion m_duskOrientation;
    private float m_noon;
    private float3 m_south;
    private float m_zenithIntensity;

    private float time24Hours => (unnormalizedTime / summerDayTimeDuration) * 17f + 5.5f;

    private float SummerLightTemperature(float time24H)
    {
        return 4300f - math.pow(math.abs(time24H - 13.83f), 3.2f) / 0.59f;
    }

    private float WinterLightTemperature(float time24H)
    {
        float winterTimeRemap = 5f / ((winterDayTimeDuration / summerDayTimeDuration) * 17f);
        float winterDawnTime24Hours = (winterDawnTime / summerDayTimeDuration) * 17f + 5.5f;
        float remappedTime24Hours = (time24H - winterDawnTime24Hours) * winterTimeRemap + 10.5f;
        
        return 3200f - math.pow(math.abs(remappedTime24Hours - 12.7f), 2.32f) / 0.01f;
    }

    private float lightColorTemperature
    {
        get
        {
            float time24H = time24Hours;
            return math.lerp(WinterLightTemperature(time24H), SummerLightTemperature(time24H), seasonInterpolator);
        }
    }

    public void StartDay()
    {
        m_noon = dayTimeDuration * 0.5f;

        m_light.transform.rotation = m_dawnOrientation;
        unnormalizedTime = dawnTime;
    }

    private Vector3 debugZenith;
    
    private void Start()
    {
        m_light = GetComponent<Light>();
        Debug.Assert(m_light.type == LightType.Directional);

        float3 zenithDirection = debugZenith = -m_light.transform.forward;
        m_south = math.normalize(new float3(zenithDirection.x, 0, zenithDirection.z));

        m_summerZenithOrientation = m_light.transform.rotation;
        m_dawnOrientation = math.mul(quaternion.AxisAngle(m_south, math.PIHALF), m_summerZenithOrientation);
        m_duskOrientation = math.mul(quaternion.AxisAngle(m_south, -math.PIHALF), m_summerZenithOrientation);
        
        m_zenithIntensity = m_light.intensity;
        
        StartDay();
    }

    public void Update()
    {
        if (dawnOffsetTime >= dayTimeDuration)
        {
            return;
        }

        unnormalizedTime += Time.deltaTime * debugTimeScale;
        float time = dawnOffsetTime;

        float morningFogInterp = math.saturate(time / morningFogEndTime);
        fogSettings.density = math.lerp(morningDensity, normalDensity,
            morningFogInterp * morningFogInterp * morningFogInterp);

        bool isMorning = time < m_noon;
        quaternion targetOrientation = isMorning ? m_summerZenithOrientation : m_duskOrientation;
        quaternion srcOrientation = isMorning ? m_dawnOrientation : m_summerZenithOrientation;
        float interpolator = isMorning ? time / m_noon : (time - m_noon) / m_noon;

        m_light.transform.rotation = Quaternion.Lerp(srcOrientation, targetOrientation, interpolator);
        m_light.colorTemperature = lightColorTemperature;
        
        float3 lightDirection = -m_light.transform.forward;
        float3 projectedDirection = math.normalize(lightDirection - math.dot(lightDirection, m_south) * m_south);
        m_light.intensity = m_zenithIntensity * math.dot(projectedDirection, math.up());
    }
}