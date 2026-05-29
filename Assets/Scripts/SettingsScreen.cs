using System;
using TMPro;
using UnityEngine;

public class SettingsScreen : MonoBehaviour
{
    public Action onSettingsChanged;

    public TMP_Dropdown fogResolutionDropdown;
    public TMP_Dropdown fogStepCountDropdown;
    
    [HideInInspector]
    public float fogResolution;
    [HideInInspector]
    public int fogStepCount;

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
