using UnityEngine;
using Unity.Mathematics;

public class MoveToClosestRiverLocation : MonoBehaviour
{
    public Transform playerTransform; 
    public Terrain terrain;

    private float2 m_terrainOffset;
    private float2 m_terrainScale;
    private float2 m_invTerrainScale;
    
    void Start()
    {
        Vector3 terrainPos = terrain.transform.position;
        m_terrainOffset = new float2(terrainPos.x, terrainPos.z);
        Vector3 terrainSize = terrain.terrainData.size;
        m_terrainScale = new float2(terrainSize.x, terrainSize.z);
        m_invTerrainScale = new float2(1.0f / terrainSize.x, 1.0f / terrainSize.z);
    }
    
    void Update()
    {
        Vector3 playerPos = playerTransform.position;
        float2 riverPosition = GameManager.instance.GetNearestRiverLocation((new float2(playerPos.x, playerPos.z) - m_terrainOffset) * m_invTerrainScale);
        riverPosition = (riverPosition * m_terrainScale) + m_terrainOffset;
        transform.position = new Vector3(riverPosition.x, transform.position.y, riverPosition.y);
    }
}
