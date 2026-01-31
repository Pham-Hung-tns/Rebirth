using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectPlayerDecision : AIDecision
{
    private Transform target;
    
    public override bool MakeADecision()
    {
        return DetectPlayerInRange();
    }

    public bool DetectPlayerInRange()
    {
        Collider2D hit = Physics2D.OverlapCircle(enemyBrain.transform.position, _data.rangeCanDetectPlayer, _data.playerLayer);
        if (hit != null)
        {
            enemyBrain.Player = hit.transform;
            return true;
        }
        target = null;
        enemyBrain.Player = null;
        return false;
    }
}
