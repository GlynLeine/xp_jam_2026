using UnityEngine;

[RequireComponent(typeof(TMPro.TMP_Dropdown))]
public class FogStepCountDropdown : MonoBehaviour
{
    void Start()
    {
        int[] stepCountOptions = new[]{ 100, 75, 50, 25 };
        for (int i = 0; i < stepCountOptions.Length; i++)
        {
            if (stepCountOptions[i] == GameManager.instance.fogStepCount)
            {
                GetComponent<TMPro.TMP_Dropdown>().SetValueWithoutNotify(i);
            }
        }
    }
}
