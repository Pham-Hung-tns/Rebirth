using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class CharacterController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Rigidbody2D rigidBody2D;
    [SerializeField] protected Animator animator;


    public Rigidbody2D Rb { get => rigidBody2D; set => rigidBody2D = value; }
    public SpriteRenderer Spr { get => spriteRenderer; set => spriteRenderer = value; }
    public Animator Anim { get => animator; set => animator = value; }

    private int currentState;
    protected virtual void OnMove(Vector2 input)
    {
        // Handle movement input
    }

    protected virtual void OnAttack(bool canAttack)
    {
        // Handle attack input
    }

    protected virtual void OnSkill(bool canUseSkill)
    {
        // Handle skill input
    }

    public void ChangeAnimationState(int newState)
    {
        if (currentState == newState) return;
        animator.CrossFade(newState, 0.25f);
        currentState = newState;
    }

}
