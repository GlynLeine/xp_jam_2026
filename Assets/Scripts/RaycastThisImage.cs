using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RaycastThisImage : MonoBehaviour
{
    private Canvas m_canvas;
    
    private void OnEnable()
    {
        Transform check = transform.parent;
        while (check is not null && m_canvas is null)
        {
            m_canvas = check.GetComponent<Canvas>();
            check = check.parent;
        }
        
        GraphicRegistry.RegisterRaycastGraphicForCanvas(m_canvas, GetComponent<Image>());
    }

    private void OnDisable()
    {
        GraphicRegistry.UnregisterRaycastGraphicForCanvas(m_canvas, GetComponent<Image>());
    }
}
