using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    protected const string LAYER_PLAYER = "Player";
    protected const string LAYER_ENEMY = "Enemy";

    [SerializeField] protected WeaponDataSO weaponData;

    [SerializeField] protected Animator animator;

    private int currentState;
    public WieldedWeaponCombat Character { get; set; }
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

    // Âm thanh giờ đây được Handle tự động bởi AudioFeedback (Kéo thẻ trên inspector)
}
