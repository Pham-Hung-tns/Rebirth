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

    private PlayerControls _gameInput;

    private void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new PlayerControls();
            _gameInput.Player.SetCallbacks(this);
        }
        _gameInput.Player.Enable();
    }

    private void OnDisable()
    {
        _gameInput.Player.Disable();
    }

    // Triển khai interface từ New Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        // Khi thả phím
        if (context.canceled)
        {
            MoveEvent?.Invoke(Vector2.zero);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            AttackEvent?.Invoke(true);

        if(context.phase == InputActionPhase.Canceled)
            AttackEvent?.Invoke(false);
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            SkillEvent?.Invoke(true);

        if (context.phase == InputActionPhase.Canceled)
            SkillEvent?.Invoke(false);
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
