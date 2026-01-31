using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectObstacleDecision : AIDecision
{
    private Transform target;
    
    public override bool MakeADecision()
    {
        return DetectObstacle();
    }

    // Detect obstacle which is between enemy and player
    public bool DetectObstacle()
    {
        if (enemyBrain.Player == null)
            return false;

        target = enemyBrain.Player;

        if ((target.transform.position - enemyBrain.transform.position).magnitude > _data.rangeCanLookAtPlayer)
        {
            enemyBrain.Player = null;
            return false;
        }

        Vector3 direction = (target.transform.position - enemyBrain.transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(enemyBrain.transform.position, direction, _data.rangeCanLookAtPlayer, _data.obstacleLayer);
        
        if (hit.collider == null)
        {
            enemyBrain.Player = target;
            return true; // No obstacle, can see player
        }
        else
        {
            enemyBrain.Player = null;
            return false; // Obstacle blocking view
        }
    }
}
