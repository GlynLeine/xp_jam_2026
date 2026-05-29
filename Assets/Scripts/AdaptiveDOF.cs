using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdaptiveDOF : MonoBehaviour
{
    private Volume m_volume;
    private DepthOfField m_depthOfField;
    private float m_defaultFocusDistance;

    private void Start()
    {
        m_volume = GetComponent<Volume>();
        m_volume.profile.TryGet(out m_depthOfField);
        m_defaultFocusDistance = m_depthOfField.focusDistance.value;
    }

    void Update()
    {
        Ray cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        float focusDistance = m_defaultFocusDistance;
        if (Physics.Raycast(cameraRay, out RaycastHit hit, m_defaultFocusDistance))
        {
            focusDistance = math.clamp(hit.distance, 0.4f, m_defaultFocusDistance);
        }

        m_depthOfField.focusDistance.value = focusDistance;
    }
}
