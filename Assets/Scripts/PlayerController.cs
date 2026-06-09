using UnityEngine;
using Unity.Mathematics;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : GameCharacterController
{
    [Header("Player Specific")] 
    public BlackScreen blackScreen;
    public Transform cameraTarget;
    public Transform aimVisual;
    public MeshRenderer aimRenderer;
    public Transform aimSelect;
    public bool unlockAllWeapons;
    public bool hasAggro = false;
    public float combatMusicFadeDuration = 1f;
    private float m_combatMusicFadeTime;

    public GameObject[] weaponVisuals;
    private int m_activeWeaponVisual;

    public Destination[] destinations;

    public MaskPedestal interactingPedestal { get; set; }
    
    private int m_shaderIDPlayerPosition;
    private int m_shaderIDPlayerWeapon;
    private int m_shaderIDPlayerWeaponFill;
    private Quaternion m_cameraRotation;
    
    private void Awake()
    {
        Shader.SetGlobalFloat("_EnableDither", 1f);
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.combatMusicScalar = 0f;
        }

        Shader.SetGlobalFloat("_EnableDither", 0f);
    }

    protected override void OnStart()
    {
        GameManager.instance.nextScene = SceneManager.GetActiveScene().buildIndex;
        
        m_rng.InitState();
        
        m_cameraRotation = cameraTarget.rotation;

        m_shaderIDPlayerPosition = Shader.PropertyToID("_Player_Position");
        m_shaderIDPlayerWeapon = Shader.PropertyToID("_CurrentWeaponColor");
        m_shaderIDPlayerWeaponFill = Shader.PropertyToID("_CurrentWeaponFill");
        
        Debug.Assert(attacks.Length == 4);
        Debug.Assert(weaponVisuals.Length == 4);

        for (int i = 0; i < 4; ++i)
        {
            attacks[i].selectionDirection = math.normalize(attacks[i].selectionDirection);
        }
        
        if (unlockAllWeapons)
        {
            for (int i = 0; i < 4; ++i)
            {
                UnlockWeapon(i);
            }

            m_attackIndex = 0;
        }
        
        for (int i = 0; i < 4; ++i)
        {
            weaponVisuals[i].SetActive(false);
        }
        weaponVisuals[m_attackIndex].SetActive(true);
        m_activeWeaponVisual = m_attackIndex;

        
        Debug.Assert(destinations.Length == 2);

        for (int i = 0; i < 2; ++i)
        {
            if (destinations[i].isStart)
            {
                transform.position = destinations[i].spawnLocation.position;
            }
        }
    }

    protected override float3 GetAimInput()
    {
        float3 result = float3.zero;
        
        if (math.lengthsq(m_input.aimInput) > math.EPSILON)
        {
            if (m_input.isCurrentDeviceMouse)
            {
                result = Camera.main.ScreenToViewportPoint(m_input.aimInput);
                result = math.normalize(new float3(result.x - 0.5f, 0f, result.y - 0.5f));
            }
            else
            {
                result = math.normalize(new float3(m_input.aimInput.x, 0.0f, m_input.aimInput.y));
            }

            if (!m_isAttacking)
            {
                result.y = math.atan2(result.x, result.z) +
                         math.radians(Camera.main.transform.eulerAngles.y);
            }
        }

        return result;
    }

    protected override void OnAim()
    {
        if (Camera.main == null)
        {
            return;
        }
        
        aimVisual.forward = m_aimDirection;
        aimSelect.forward = math.mul(quaternion.Euler(0.0f, math.radians(Camera.main.transform.eulerAngles.y), 0.0f), math.forward());
    }

    private void UnlockWeapon(int index)
    {
        attacks[index].unlocked = true;
        attacks[index].selectionVisual.SetActive(true);
        attacks[index].timeBuffer = attacks[index].duration;
    }
    
    protected override bool OnHandleAttacking(ref float3 movement, ref bool doMovement)
    {
        if (!m_isAttacking && m_input.changeMask && math.lengthsq(m_aimInput) > math.EPSILON)
        {
            float closest = 0f;
            int closestIndex = -1;
            for (int i = 0; i < 4; ++i)
            {
                if (!attacks[i].unlocked)
                {
                    continue;
                }
                
                float distance = math.dot(m_aimInput, attacks[i].selectionDirection);
                if (distance > closest)
                {
                    closest = distance;
                    closestIndex = i;
                }
            }
        
            m_attackIndex = closestIndex;
            aimSelect.gameObject.SetActive(true);
        }
        else
        {
            aimSelect.gameObject.SetActive(false);
        }

        if (doMovement && interactingPedestal is not null && !attacks[interactingPedestal.maskIndex].unlocked)
        {
            if (m_input.attack)
            {
                UnlockWeapon(interactingPedestal.maskIndex);
                m_attackIndex = interactingPedestal.maskIndex;
            }

            return false;
        }

        return true;
    }

    protected override float GetTargetRotation()
    {
        float3 inputDirection = math.normalize(new float3(m_input.move.x, 0.0f, m_input.move.y));
        return math.atan2(inputDirection.x, inputDirection.z) + math.radians(Camera.main.transform.eulerAngles.y);
    }

    private void LateUpdate()
    {
        if (m_displayHealth <= 0.1f && m_health <= 0f)
        {
            return;
        }

        if (m_attackIndex != m_activeWeaponVisual)
        { 
            for (int i = 0; i < 4; ++i)
            {
                weaponVisuals[i].SetActive(false);
            }
            weaponVisuals[m_attackIndex].SetActive(true);
            m_activeWeaponVisual = m_attackIndex;
        }
        
        if (combatMusicFadeDuration != 0f)
        {
            m_combatMusicFadeTime += hasAggro ? Time.deltaTime : -Time.deltaTime;
            m_combatMusicFadeTime = Mathf.Clamp(m_combatMusicFadeTime, 0.0f, combatMusicFadeDuration);

            GameManager.instance.combatMusicScalar = m_combatMusicFadeTime / combatMusicFadeDuration;
        }
        else
        {
            GameManager.instance.combatMusicScalar = hasAggro ? 1f : 0f;
        }
        hasAggro = false;

        cameraTarget.rotation = m_cameraRotation;
        Shader.SetGlobalVector(m_shaderIDPlayerPosition, cameraTarget.position);
        
        aimRenderer.material.SetColor(m_shaderIDPlayerWeapon, m_attackIndex >= 0 ? attacks[m_attackIndex].color : Color.white);
        aimRenderer.material.SetFloat(m_shaderIDPlayerWeaponFill, m_attackIndex >= 0 ? attacks[m_attackIndex].timeBuffer / (attacks[m_attackIndex].duration + attacks[m_attackIndex].cooldown) : 0f);
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
        blackScreen.FadeIn(() => GameManager.instance.StartLoadingScene(2));
    }
}