using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LinkDialog : MonoBehaviour
{
    public DayTime dayTime;
    public cherrydev.DialogBehaviour dialogBehaviour;
    private bool m_isEndOfDay;
    
    private void Start()
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (activeSceneIndex > 1)
        {
            if (activeSceneIndex >= 3)
            {
                GameManager.instance.dialogIndex = (activeSceneIndex - 3) * 4;
            }

            startDialogue();
        }
    }

    public void startEndOfDayDialogue()
    {
        m_isEndOfDay = true;
        startDialogue();
    }

    public void endEndOfDayDialogue()
    {
        if (m_isEndOfDay)
        {
            m_isEndOfDay = false;
            dayTime.blackScreen.StartFade();
            dayTime.blackScreen.onFadeFinished = dayTime.StartDay;
        }
    }

    public void startDialogue()
    {
        GameManager.instance.startDialogue(dialogBehaviour);
    }
}
