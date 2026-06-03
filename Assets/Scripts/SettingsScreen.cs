using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SettingsScreen : MonoBehaviour
{
    public GameObject[] disableOnSettingScreen;
    public Action onSettingsChanged;

    public TMP_Dropdown fogStepCountDropdown;
    
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
        fogStepCountDropdown.onValueChanged.AddListener(OnSettingsChanged);
    }

    private void OnSettingsChanged(int idc)
    {
        int[] stepCountOptions = new[]{ 100, 75, 50, 25 };
        GameManager.instance.fogStepCount = stepCountOptions[fogStepCountDropdown.value];
        
        onSettingsChanged?.Invoke();
    }
}
