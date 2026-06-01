using System;
using Unity.Mathematics;
using UnityEngine;

public class BlackScreen : MonoBehaviour
{
    public float fadeDuration = 1f;
    private float m_timeBuffer;
    private bool m_fadeIn;

    private MeshRenderer m_meshRenderer;
    private int m_opacityIndex;

    public Action onFadeFinished;

    public bool isFading => m_timeBuffer < fadeDuration;
    
    private void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_opacityIndex = Shader.PropertyToID("_Opacity");
    }

    public void StartFade()
    {
        if (isFading)
        {
            m_meshRenderer.material.SetFloat(m_opacityIndex, m_fadeIn ? 1f : 0f);
        
            onFadeFinished?.Invoke();
            m_fadeIn = !m_fadeIn;
        }
        
        m_timeBuffer = 0f;
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

        if (m_timeBuffer > fadeDuration)
        {
            onFadeFinished?.Invoke();
            m_fadeIn = !m_fadeIn;
        }
    }
}
