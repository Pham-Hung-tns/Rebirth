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
        _rb.MovePosition(_rb.position + direction.normalized * (currentSpeed * Time.fixedDeltaTime));
    }

    //dung trong update
    public float CalculateSpeed(Vector2 movementInput)
    {
        if (movementInput.magnitude > 0)
        {
            currentSpeed += _data.acceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed -= _data.deceleration * Time.deltaTime;
        }
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

    //public void FacingRightDirection()
    //{
    //    _spriteRenderer.flipX = false;
    //}
    //public void Dash()
    //{
    //    if (usingDash)
    //        return;

    //    usingDash = true;
    //    StartCoroutine(IEDash());
    //}

    //private IEnumerator IEDash()
    //{
    //    ModifyColorForDash(transperency);
    //    currentSpeed = dashSpeed;
    //    yield return new WaitForSeconds(dashTime);
    //    currentSpeed = PlayerConfig.speed;
    //    ModifyColorForDash(1f);
    //    cooldown = true;
    //    StartCoroutine(ControlDash());
    //}
    //IEnumerator ControlDash()
    //{
    //    yield return new WaitForSeconds(dashWaitTime);
    //    usingDash = false;
    //    cooldown = false;
    //}
    //private void ModifyColorForDash(float alpha)
    //{
    //    Color color = spriteRenderer.color;
    //    color.a = alpha;
    //    spriteRenderer.color = color;
    //}
}
