using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogSettings : MonoBehaviour
{
    private Volume m_volume;
    private VolumetricFogVolumeComponent m_volumetricFog;

    private void Start()
    {
        m_volume = GetComponent<Volume>();
        m_volume.profile.TryGet(out m_volumetricFog);
    }

    public float resolution
    {
        set
        {
            m_volumetricFog.resolutionScale.value = value;
            m_volumetricFog.resolutionScale.overrideState = true;
        }
    }

    public int stepCount
    {
        set
        {
            m_volumetricFog.stepCount.value = value;
            m_volumetricFog.stepCount.overrideState = true;
        }
    }

    public float density
    {
        set
        {
            m_volumetricFog.fogDensity.value = value;
            m_volumetricFog.fogDensity.overrideState = true;
        }
    }
    
    void Update()
    {
        float framerate = 1f / Time.unscaledDeltaTime;
        m_volumetricFog.fogHistoryContribution.value = math.lerp(0.1f, 0.9f, math.saturate(framerate / 60f - 1f));
        m_volumetricFog.fogHistoryContribution.overrideState = true;
    }
}
