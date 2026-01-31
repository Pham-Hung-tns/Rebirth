using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockTargetAction : AIAction
{
    private Transform target;
    public override void OnEnter()
    {
        // setup attack (e.g. reset timers)
        target = enemyBrain.Player;
        enemyBrain.TimeLimit = enemyBrain.EnemyConfig.timeToAttack;
        enemyBrain.Rb.velocity = Vector2.zero;

        enemyBrain.ChangeAnimationState(Settings.IDLE_STATE);
    }

    public override void OnUpdate()
    {
        enemyBrain.ChangeDirection(target.position);
    }

    public override void OnExit()
    {
        enemyBrain.Player = target;
    }
}
