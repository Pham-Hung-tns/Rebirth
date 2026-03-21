using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static PlayerControls;

[CreateAssetMenu(fileName = "New Input", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    // Các Event để các script khác đăng ký lắng nghe
    public event UnityAction<Vector2> MoveEvent;
    public event UnityAction<bool> AttackEvent;
    public event UnityAction<bool> SkillEvent;
    public event UnityAction PickupEvent;
    public event UnityAction ChangeItemEvent;
    public event UnityAction InteractEvent;

    // --- Flag khóa Input tấn công/di chuyển khi mở UI ---
    public bool EnableCombatInput { get; set; } = true;

    private PlayerControls _gameInput;

    private void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new PlayerControls();
            _gameInput.Player.SetCallbacks(this);
            _gameInput.Player.Enable();
        }
        
        UIEvents.OnUIStateChanged += SetCombatInputState;
    }

    private void OnDisable()
    {
        if (_gameInput != null)
        {
            _gameInput.Player.Disable();
        }

        UIEvents.OnUIStateChanged -= SetCombatInputState;
    }

    private void SetCombatInputState(bool isUIOpen)
    {
        EnableCombatInput = !isUIOpen;
    }

    // Triển khai interface từ New Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!EnableCombatInput)
        {
            // Bắt buộc vector di chuyển về 0 để nhân vật dừng lại
            MoveEvent?.Invoke(Vector2.zero);
            return;
        }

        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!EnableCombatInput) return;

        if (context.phase == InputActionPhase.Performed)
            AttackEvent?.Invoke(true);
        else if (context.phase == InputActionPhase.Canceled)
            AttackEvent?.Invoke(false);
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        if (!EnableCombatInput) return;

        if (context.phase == InputActionPhase.Performed)
            SkillEvent?.Invoke(true); // Assuming SkillEvent expects a bool, based on its declaration
    }

    public void OnPickItem(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            PickupEvent?.Invoke();
    }

    public void OnChangeItem(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            ChangeItemEvent?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            InteractEvent?.Invoke();
    }

    // Allow external callers (eg. UI controllers) to enable/disable the gameplay action map
    public void SetGameplayEnabled(bool enabled)
    {
        if (_gameInput == null)
        {
            _gameInput = new PlayerControls();
            _gameInput.Player.SetCallbacks(this);
        }

        if (enabled) _gameInput.Player.Enable();
        else _gameInput.Player.Disable();
    }
}
