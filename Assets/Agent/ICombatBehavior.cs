using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatBehavior
{
    void StartAttack();
    void ReleaseAttack();
    void HandleAiming(Vector2 direction);
    event System.Action OnAttackComplete;
}
