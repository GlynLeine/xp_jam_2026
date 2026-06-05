using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EnemyBrain))]
public class EnemyController : GameCharacterController
{
    protected override void OnStart()
    {
        Debug.Assert(attacks.Length >= 1);
        Debug.Assert((m_input as EnemyBrain) is not null);
        
        EnemyBrain brain = m_input as EnemyBrain;
        
        m_attackIndex = (int)brain!.weaponType;
        brain.attackInfo = attacks[m_attackIndex];
    }

    protected override void OnDeath()
    {
        if (m_displayHealth > 0.1f)
        {
            return;
        }
        
        Destroy(gameObject);
    }
}