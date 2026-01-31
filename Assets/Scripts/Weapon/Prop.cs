using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class Prop : MonoBehaviour, ITakeDamage
{
    [SerializeField] float durability;
    private int counter;
    public void TakeDamage(int amount)
    {
        counter++;
        if (counter > durability)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int amount, GameObject attacker, Vector2 knockbackDir, float knockbackForce)
    {
        //TODO: Add knockback effect to prop
        Debug.Log("Prop take damage with knockback");
    }
}
