using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBonus : BonusBase
{
    protected override void Update()
    {
        base.Update();
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }
    protected override void GetBonus()
    {
        base.GetBonus();
        player.GetComponent<PlayerVitality>().RecoverEnergy(1);
    }
}
