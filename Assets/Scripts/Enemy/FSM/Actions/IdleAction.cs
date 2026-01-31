using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleAction : AIAction
{
    public override void OnEnter()
    {
        if (enemyBrain != null)
        {
            enemyBrain.Rb.velocity = Vector2.zero;
            enemyBrain.TimeLimit = enemyBrain.EnemyConfig.timeToIdle;
            enemyBrain.ChangeAnimationState(Settings.IDLE_STATE);
        }
    }

    public override void OnUpdate() 
    {

    }

    public override void OnExit() 
    {

    }
}
