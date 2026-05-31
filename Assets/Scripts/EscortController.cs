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

    protected override void OnDeath()
    {
        GameManager.instance.succeededSeason = false;
        blackScreen.onFadeFinished = () => SceneManager.LoadScene(2);
        blackScreen.StartFade();
    }
}