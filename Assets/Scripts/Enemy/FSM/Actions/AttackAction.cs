using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : AIAction
{
    private float delayTimer = 0f;

    public override void OnEnter()
    {
        enemyBrain.IsAttack = true;
        enemyBrain.Rb.velocity = Vector2.zero;
        delayTimer = enemyBrain.EnemyConfig.attackDelay;
    }

    public override void OnUpdate()
    {
        delayTimer -= Time.deltaTime;
        
        // Theo dõi hướng player
        if (enemyBrain.Player != null)
        {
            enemyBrain.ChangeDirection(enemyBrain.Player.position);
        }
        
        if (enemyBrain.IsAttack == true && delayTimer <= 0f)
        {
            Debug.Log(enemyBrain.Player);
            // Delay hết, gọi tấn công thông qua attack system (Weapon hoặc Skill)
            enemyBrain.CurrentAttackSystem.StartAttack();
        }
    }

    public override void OnExit()
    {
    }
}
