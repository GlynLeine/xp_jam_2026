using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EscortBrain))]
public class EscortController : GameCharacterController
{
    public BlackScreen blackScreen;
    
    protected override void OnStart()
    {
        Debug.Assert((m_input as EscortBrain) is not null);
        
        EscortBrain brain = m_input as EscortBrain;
    }

    bool m_isAlreadyDead = false;
    protected override void OnDeath()
    {
        if (m_displayHealth > 0.1f)
        {
            return;
        }

        if (m_isAlreadyDead)
        {
            return;
        }
        m_isAlreadyDead = true;
        
        GameManager.instance.succeededSeason = false;
        GameManager.instance.nextScene = SceneManager.GetActiveScene().buildIndex;
        GameManager.instance.isPaused = true;
        blackScreen.FadeIn(() => GameManager.instance.StartLoadingScene(2));
    }
}