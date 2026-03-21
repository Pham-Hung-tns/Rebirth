using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerConfig _data;
    private SpriteRenderer _spriteRenderer;

    private float currentSpeed = 0f;
    public void Initialize(Rigidbody2D rb, SpriteRenderer spriteRenderer, PlayerConfig data)
    {
        _rb = rb;
        _data = data;
        _spriteRenderer = spriteRenderer;
    }

    // dùng trong fixedupdate
    public void HandleMovement(Vector2 direction)
    {
        _rb.MovePosition(_rb.position + direction.normalized * (_data.speed * Time.fixedDeltaTime));
    }

    //dung trong update
    public float CalculateSpeed(Vector2 movementInput)
    {
        // phần này sử dụng acceleration và deceleration. Tạm thời chưa sử dụng nên note lại
        // if (movementInput.magnitude > 0)
        // {
        //     currentSpeed += Time.deltaTime;
        // }
        // else
        // {
        //     currentSpeed -= Time.deltaTime;
        // }
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, _data.speed);
        return currentSpeed;
    }

    public void ResetSpeed()
    {
        currentSpeed = 0f;
    }

    public void RotationPlayer(Vector2 direction)
    {
        var result = Vector3.Cross(Vector2.up, direction.normalized);
        if (result.z > 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (result.z < 0)
        {
            _spriteRenderer.flipX = false;
        }
    }

    // ── Animation Events ──────────────────────────────────────────────────────

    /// <summary>
    /// Gọi hàm này từ Animation Event trên clip chạy của player.
    /// Đặt event tại frame khi chân chạm đất để sync âm thanh chính xác.
    /// </summary>
    public void OnFootstepAnimEvent()
    {
        AudioManager.Instance?.PlaySFX(SFXClip.PlayerFootstep);
    }
}
