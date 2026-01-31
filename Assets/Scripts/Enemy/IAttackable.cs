using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    void StartAttack();
    void ReleaseAttack();
    event System.Action OnAttackComplete;
}
