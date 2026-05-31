using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EscortBrain))]
public class EscortController : GameCharacterController
{
    protected override void OnStart()
    {
        Debug.Assert((m_input as EscortBrain) is not null);
        
        EscortBrain brain = m_input as EscortBrain;
    }

    protected override void OnDeath()
    {
        Destroy(gameObject);
    }
}