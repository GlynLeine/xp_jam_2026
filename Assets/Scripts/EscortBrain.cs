using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class EscortBrain : InputDriver
{
    
    public float reactionTime = 0.3f;
    
    [HideInInspector] public AttackInfo attackInfo;
    
    private PlayerController m_player;
    private float m_attackTimeBuffer;
    private float2 m_aimDirection;

    private void Awake()
    {
        m_player = FindAnyObjectByType<PlayerController>();
    }

    private void Update()
    {
        if (!m_player)
        {
            return;
        }
        
        float2 movementInput = float2.zero;
        float3 toPlayer = m_player.transform.position - transform.position;
        float2 toPlayer2D = new float2(toPlayer.x, toPlayer.z);
        float playerDistanceSq = math.lengthsq(toPlayer2D);
        toPlayer2D *= math.rsqrt(playerDistanceSq);

        bool inReach = playerDistanceSq > (attackInfo.aoe.y * attackInfo.aoe.y);
        if (inReach || math.all(math.abs(math.normalize(new float2(transform.forward.x, transform.forward.z)) - toPlayer2D) >= 0.1f))
        {
            movementInput = toPlayer2D;
        }
        
        MoveInput(movementInput);
    }
}
