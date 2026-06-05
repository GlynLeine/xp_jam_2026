using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerCharacterInput : InputDriver
{
    public cherrydev.DialogBehaviour dialog;
    public bool dialogOnly;
    public bool cursorLocked { get; private set; } = false;
    public bool cursorInputLocked { get; private set; } = true;
    public GameObject pauseMenu;
    
    private PlayerInput m_playerInput;

    public override bool isCurrentDeviceMouse => m_playerInput.currentControlScheme == "Keyboard&Mouse";

    private void Awake()
    {
        m_playerInput = GetComponent<PlayerInput>();
    }

    public void OnMove(InputValue value)
    {
        if (dialog.isDialogActive || dialogOnly || GameManager.instance.isPaused)
        {
            return;
        }
        MoveInput(value.Get<Vector2>());
    }

    public void OnAim(InputValue value)
    {
        if (dialog.isDialogActive || dialogOnly || GameManager.instance.isPaused)
        {
            return;
        }
        if(cursorInputLocked)
        {
            AimInput(value.Get<Vector2>());
        }
    }

    public void OnChangeMask(InputValue value)
    {
        if (dialog.isDialogActive || dialogOnly || GameManager.instance.isPaused)
        {
            return;
        }
        ChangeMaskInput(value.isPressed);
    }
    
    public void OnDodge(InputValue value)
    {
        if (dialog.isDialogActive || dialogOnly || GameManager.instance.isPaused)
        {
            return;
        }
        DodgeInput(value.isPressed);
    }

    public void OnAttack(InputValue value)
    {
        if (dialog.isDialogActive || dialogOnly || GameManager.instance.isPaused)
        {
            if (value.isPressed)
            {
                dialog.moveToNext();
            }

            return;
        }

        AttackInput(value.isPressed);
    }

    public void OnPause(InputValue value)
    {
        if (dialog.isDialogActive || dialogOnly)
        {
            return;
        }
        
        if (value.isPressed)
        {
            TogglePauseGame();
        }
    }

    public void TogglePauseGame()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        GameManager.instance.isPaused = !GameManager.instance.isPaused;
            
        AttackInput(false);
        DodgeInput(false);
        ChangeMaskInput(false);
        MoveInput(Vector2.zero);
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    public void SetCursorState(bool newState)
    {
        cursorLocked = newState;
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public void LockCursorInput(bool locked)
    {
        cursorInputLocked = locked;
    }
}
