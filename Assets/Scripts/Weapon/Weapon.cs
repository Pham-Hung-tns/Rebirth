using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    protected const string LAYER_PLAYER = "Player";
    protected const string LAYER_ENEMY = "Enemy";

    [SerializeField] protected WeaponDataSO weaponData;

    [Header("Audio")]
    [SerializeField] protected AudioClip attackSFX;
    [SerializeField] protected AudioClip chargeSFX;

    [SerializeField] protected Animator animator;

    private int currentState;
    public CharacterWeapon Character { get; set; }
    public WeaponDataSO WeaponData => weaponData;

    public virtual void DestroyWeapon()
    {

    }

    // New API: execute an attack with an optional damage multiplier
    public virtual void ExecuteAttack(float damageMultiplier = 1f)
    {
        // Default: do nothing. Subclasses should override.
    }
    public void ChangeAnimationState(int newState)
    {
        if (currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }

    // Audio helpers
    public void PlayAttackSFX()
    {
        if (attackSFX == null) return;
        AudioManager.Instance.PlaySFX(attackSFX);
    }

    public void PlayChargeSFX()
    {
        if (chargeSFX == null) return;
        AudioManager.Instance.PlaySFX(chargeSFX);
    }
}
