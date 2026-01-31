using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class ReachedDestinationDecision : AIDecision
{
    public override bool MakeADecision()
    {
        return Vector3.Distance(enemyBrain.Rb.position, enemyBrain.PatrolPosition) <= 0.5f;
 
    }
}
