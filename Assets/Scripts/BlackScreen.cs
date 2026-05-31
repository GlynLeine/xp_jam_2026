using System;
using Unity.Mathematics;
using UnityEngine;

public class BlackScreen : MonoBehaviour
{
    public FogSettings fogSettings;
    public float fadeDuration = 1f;
    private float m_timeBuffer;
    private bool m_fadeIn;

    private MeshRenderer m_meshRenderer;
    private int m_opacityIndex;
    
    private float m_startingFogDensity;

    public Action onFadeFinished;

    public bool isFading => m_timeBuffer < fadeDuration;
    
    private void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_opacityIndex = Shader.PropertyToID("_Opacity");
    }

    public void StartFade()
    {
        m_timeBuffer = 0f;
        m_startingFogDensity = fogSettings.density;
    }
    
    private void Update()
    {
        if (m_timeBuffer > fadeDuration)
        {
            return;
        }

        m_timeBuffer += Time.deltaTime;

        float interpolator = m_timeBuffer / fadeDuration;
        if (!m_fadeIn)
        {
            interpolator = 1f - interpolator;
        }
        m_meshRenderer.material.SetFloat(m_opacityIndex, interpolator);
        
        fogSettings.density = math.lerp(m_startingFogDensity, 0f, interpolator);

        if (m_timeBuffer > fadeDuration)
        {
            onFadeFinished?.Invoke();
            m_fadeIn = !m_fadeIn;
        }
    }
}
