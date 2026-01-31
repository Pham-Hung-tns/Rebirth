using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITakeDamage
{
    // amount: sat thuong
    // attacker: nguoi tan cong
    // knockback_Dir: huong knockback
    // knockback_Force: luc knockback
    void TakeDamage(int amount, GameObject attacker, Vector2 knockbackDir, float knockbackForce);
}
