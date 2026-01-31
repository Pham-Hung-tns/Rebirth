using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChaseAction : AIAction
{
    private Transform player;
    private EnemyMovement movement;

    public override void OnEnter()
    {
        // initialize chase
        player = enemyBrain.Player?.transform;
        movement = enemyBrain != null ? enemyBrain.EnemyMovement : null;
        
        // Set flag để EnemyMovement áp dụng fallback movement
        if (movement != null)
            movement.SetChasing(true);
        
        enemyBrain.ChangeAnimationState(Settings.CHASE_STATE);
    }

    public override void OnUpdate()
    {
        if (player == null || enemyBrain == null || movement == null)
            return;

        // Liên tục request path theo vị trí hiện tại của player
        movement.RequestPath(player.position);
        enemyBrain.PatrolPosition = player.position;
    }

    public override void OnExit()
    {   
        // Unset flag khi rời ChaseAction (WanderAction sẽ quay lại pathfinding bình thường)
        if (movement != null)
            movement.SetChasing(false);
    }
}
