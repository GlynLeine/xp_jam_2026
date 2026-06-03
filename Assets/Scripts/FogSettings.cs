using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogSettings : MonoBehaviour
{
    public SettingsScreen settingsScreen;
    
    private Volume m_volume;
    private VolumetricFogVolumeComponent m_volumetricFog;
    private float m_historyContribution;

    private void Start()
    {
        m_volume = GetComponent<Volume>();
        m_volume.profile.TryGet(out m_volumetricFog);
        m_historyContribution = m_volumetricFog.fogHistoryContribution.value;

        if (settingsScreen != null)
        {
            settingsScreen.onSettingsChanged = OnSettingsChanged;
        }

        OnSettingsChanged();
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
        get => m_volumetricFog.fogDensity.value;
    }
    
    void OnSettingsChanged()
    {
        m_volumetricFog.resolutionScale.value = GameManager.instance.fogResolution;
        m_volumetricFog.resolutionScale.overrideState = true;
        m_volumetricFog.stepCount.value = GameManager.instance.fogStepCount;
        m_volumetricFog.stepCount.overrideState = true;
    }

    
    void Update()
    {
        float framerate = 1f / Time.unscaledDeltaTime;
        m_volumetricFog.fogHistoryContribution.value = math.lerp(0.0f, m_historyContribution, math.saturate(framerate / 60f - 1f));
        m_volumetricFog.fogHistoryContribution.overrideState = true;
    }
}
