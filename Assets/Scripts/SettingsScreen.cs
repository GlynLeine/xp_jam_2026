using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SettingsScreen : MonoBehaviour
{
    public GameObject[] disableOnSettingScreen;
    public UnityEvent onSettingsChanged;

    public TMP_Dropdown fogResolutionDropdown;
    public TMP_Dropdown fogStepCountDropdown;
    
    [HideInInspector]
    public float fogResolution;
    [HideInInspector]
    public int fogStepCount;

    private bool[] m_wasActive;
    
    public void ToggleSettingsScreen()
    {
        if (gameObject.activeSelf)
        {
            Debug.Assert(m_wasActive.Length == disableOnSettingScreen.Length);
            for (int i = 0; i < disableOnSettingScreen.Length; i++)
            {
                disableOnSettingScreen[i].SetActive(m_wasActive[i]);
            }
        }
        else
        {
            m_wasActive = new bool[disableOnSettingScreen.Length];
            for (int i = 0; i < disableOnSettingScreen.Length; i++)
            {
                m_wasActive[i] = disableOnSettingScreen[i].activeSelf;
                disableOnSettingScreen[i].SetActive(false);
            }
        }

        gameObject.SetActive(!gameObject.activeSelf);
    }
    
    private void Start()
    {
        fogResolutionDropdown.onValueChanged.AddListener(OnSettingsChanged);
        fogStepCountDropdown.onValueChanged.AddListener(OnSettingsChanged);
    }

    private void OnSettingsChanged(int idc)
    {
        float[] resolutionOptions = new[]{ 1f, 1f/2f, 1f/4f, 1f/8f, 1f/16f, 1f/32f };
        fogResolution = resolutionOptions[fogResolutionDropdown.value];
        int[] stepCountOptions = new[]{ 200, 150, 100, 50 };
        fogStepCount = stepCountOptions[fogStepCountDropdown.value];
        
        onSettingsChanged?.Invoke();
    }
}
