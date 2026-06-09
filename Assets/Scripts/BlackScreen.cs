using System;
using Unity.Mathematics;
using UnityEngine;

public class BlackScreen : MonoBehaviour
{
    public float fadeDuration = 1f;
    private float m_timeBuffer;
    private bool m_fadeIn;

    private MeshRenderer m_meshRenderer;
    private int m_opacityProperyID;

    private Action m_onFadeFinished;

    private Action m_fadeInQueue;
    private Action m_fadeOutQueue;
    
    public bool isFading => m_timeBuffer < fadeDuration;
    
    private void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_opacityProperyID = Shader.PropertyToID("_Opacity");
    }

    public void FadeIn(Action onFadeFinished)
    {
        if (isFading)
        {
            if (m_fadeInQueue != null)
            {
                m_fadeInQueue = onFadeFinished;
            }
            return;
        }
        
        m_onFadeFinished = onFadeFinished;
        m_fadeIn = true;
        m_timeBuffer = 0f;
    }
    
    public void FadeOut(Action onFadeFinished)
    {
        if (isFading)
        {
            if (m_fadeOutQueue != null)
            {
                m_fadeOutQueue = onFadeFinished;
            }
            return;
        }
        
        m_onFadeFinished = onFadeFinished;
        m_fadeIn = false;
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
        m_meshRenderer.material.SetFloat(m_opacityProperyID, interpolator);

        if (m_timeBuffer > fadeDuration)
        {
            m_onFadeFinished?.Invoke();

            if (m_fadeIn)
            {
                if (m_fadeOutQueue != null)
                {
                    FadeOut(m_fadeOutQueue);
                    m_fadeOutQueue = null;
                    return;
                }
            }
            else
            {
                if (m_fadeInQueue != null)
                {
                    FadeIn(m_fadeInQueue);
                    m_fadeInQueue = null;
                    return;
                }
            }
            
            m_fadeIn = !m_fadeIn;
        }
    }
}
