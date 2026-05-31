using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LinkDialog : MonoBehaviour
{
    public DayTime dayTime;
    public cherrydev.DialogBehaviour dialogBehaviour;
    private bool m_isEndOfDay;
    private bool m_isDenyGame;
    
    private void Start()
    {
        dialogBehaviour.ExternalFunctionsHandler.BindExternalFunction("DenyGame", denyGame);
        
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

    public void denyGame()
    {
        GameManager.instance.succeededSeason = false;
        GameManager.instance.nextScene = SceneManager.GetActiveScene().buildIndex;
        m_isDenyGame = true;
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

        if (m_isDenyGame)
        {
            m_isDenyGame = false;
            dayTime.blackScreen.onFadeFinished = () => SceneManager.LoadScene(2);
            dayTime.blackScreen.StartFade();
        }
    }

    public void startDialogue()
    {
        GameManager.instance.startDialogue(dialogBehaviour);
    }
}
