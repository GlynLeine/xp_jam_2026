using UnityEngine;

public class WinterFootStepAudio : MonoBehaviour
{
    public GameCharacterController characterController;
    
    private DayTime m_dayTime;
    void Start()
    {
        m_dayTime = FindAnyObjectByType<DayTime>();
    }

    void Update()
    {
        if (m_dayTime.season == 3)
        {
            characterController.footStepEventEmitter = GetComponent<FMODUnity.StudioEventEmitter>();
        }

        enabled = false;
    }
}
