using UnityEngine;

public class IceWater : MonoBehaviour
{
    public Material iceMaterial;
    private MeshRenderer m_meshRenderer;
    private BoxCollider m_collider;
    private DayTime m_dayTime;

    private float m_colliderHeight;
    private Material m_material;
    
    void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_collider = GetComponent<BoxCollider>();
        m_dayTime = FindAnyObjectByType<DayTime>();

        m_colliderHeight = m_collider.center.y;
        m_material = m_meshRenderer.material;
    }

    void Update()
    {
        if (m_dayTime.season == 3)
        {
            Vector3 colliderCenter = m_collider.center;
            colliderCenter.y = m_colliderHeight + 0.6f;
            m_collider.center = colliderCenter;
            m_meshRenderer.material = iceMaterial;
        }
        else
        {
            Vector3 colliderCenter = m_collider.center;
            colliderCenter.y = m_colliderHeight;
            m_collider.center = colliderCenter;
            m_meshRenderer.material = m_material;
        }
    }
}
