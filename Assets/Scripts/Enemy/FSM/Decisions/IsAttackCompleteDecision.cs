using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsAttackCompleteDecision : AIDecision
{
    public override bool MakeADecision()
    {
        // Trả về true khi IsAttack = false (tấn công hoàn thành)
        return !enemyBrain.IsAttack;
    }
}
