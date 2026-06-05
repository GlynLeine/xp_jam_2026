using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class FlowerLilyPad : MonoBehaviour
{
    public Mesh springMesh;
    public Mesh summerMesh;
    private MeshFilter m_meshFilter;

    private int m_season;
    private int m_seasonId;
    private void Awake()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_seasonId = Shader.PropertyToID("_Season");
    }

    private void OnValidate()
    {
        Update();
    }

    private void Update()
    {
        int season = Mathf.RoundToInt(Shader.GetGlobalFloat(m_seasonId));
        if (season != m_season)
        {
            m_season = season;
            OnSeasonChange();
        }
    }

    private void OnSeasonChange()
    {
        switch (m_season)
        {
            case 0:
                if (springMesh is not null)
                {
                    m_meshFilter.mesh = springMesh;
                }
                break;
            default:
                if (summerMesh is not null)
                {
                    m_meshFilter.mesh = summerMesh;
                }
                break;
        }
    }
}
