using Unity.Mathematics;
using UnityEngine;

public class EnemyBrain : InputDriver
{
    public enum WeaponType
    {
        Gauntlets,
        Spear,
        Scythe,
        Rifle,
    }
    
    public WeaponType weaponType = WeaponType.Gauntlets;
    public float attackPreviewDuration = 0.3f;
    public float reactionTime = 0.3f;
    
    public float visionRange = 15f;
    
    [HideInInspector] public AttackInfo attackInfo;
    
    private PlayerController m_player;
    private float m_attackTimeBuffer;
    private float2 m_aimDirection;
    private bool m_playerDetected;
    private float m_detectionTimeBuffer;

    private void Awake()
    {
        m_player = FindAnyObjectByType<PlayerController>();
    }

    struct PlayerDetectionInfo
    {
        public float3 toPlayer;
        public float2 toPlayer2D;
        public float playerDistanceSq;
        public bool playerDetected;
    }
    
    private PlayerDetectionInfo DetectPlayer()
    {
        PlayerDetectionInfo detectionInfo = new PlayerDetectionInfo();
        if (!m_player)
        {
            detectionInfo.playerDetected = false;
            return detectionInfo;
        }
        
        detectionInfo.toPlayer = m_player.transform.position - transform.position;
        detectionInfo.toPlayer2D = new float2(detectionInfo.toPlayer.x, detectionInfo.toPlayer.z);
        detectionInfo.playerDistanceSq = math.lengthsq(detectionInfo.toPlayer2D);
        detectionInfo.toPlayer2D *= math.rsqrt(detectionInfo.playerDistanceSq);

        if (detectionInfo.playerDistanceSq < visionRange * visionRange)
        {
            m_playerDetected = true;
        }

        if (m_playerDetected)
        {
            m_detectionTimeBuffer += Time.deltaTime;
        }
        else
        {
            m_detectionTimeBuffer = 0f;
        }
        
        detectionInfo.playerDetected = m_playerDetected && m_detectionTimeBuffer > reactionTime;
        return detectionInfo;
    }

    private void Fight(PlayerDetectionInfo detectionInfo, ref float2 movementInput)
    {
        float totalAttackDuration = reactionTime + attackPreviewDuration + attackInfo.cooldown + attackInfo.duration;
        float attackDuration = attackInfo.cooldown + attackInfo.duration;
        float attackPreviewStartTime = attackDuration + reactionTime;
        float attackPreviewEndTime = attackPreviewStartTime + attackPreviewDuration;

        AttackInput(m_attackTimeBuffer > attackPreviewStartTime && m_attackTimeBuffer < attackPreviewEndTime);
        if (m_attackTimeBuffer > attackInfo.duration && m_attackTimeBuffer < attackPreviewStartTime)
        {
            m_aimDirection = detectionInfo.toPlayer2D;
        }
        
        bool inReach = detectionInfo.playerDistanceSq > (attackInfo.aoe.y * attackInfo.aoe.y);
        if (inReach || math.all(math.abs(math.normalize(new float2(transform.forward.x, transform.forward.z)) - detectionInfo.toPlayer2D) >= 0.1f))
        {
            movementInput = detectionInfo.toPlayer2D;
        }
        
        if(inReach)
        {
            if (m_attackTimeBuffer > attackDuration)
            {
                m_attackTimeBuffer = attackDuration;
            }
        }
        else
        {
            if (m_attackTimeBuffer < totalAttackDuration)
            {
                m_attackTimeBuffer += Time.deltaTime;
            }
            
            if (m_attackTimeBuffer >= totalAttackDuration)
            {
                m_attackTimeBuffer = 0f;
            }
        }
    }

    void Wander(ref float2 movementInput)
    {
        
    }
    
    private void Update()
    {
        float2 movementInput = float2.zero;

        PlayerDetectionInfo detectionInfo = DetectPlayer();

        if (detectionInfo.playerDetected)
        {
            Fight(detectionInfo, ref movementInput);
        }
        else
        {
            Wander(ref movementInput);
        }
        
        AimInput(m_aimDirection);
        MoveInput(movementInput);
    }
}
