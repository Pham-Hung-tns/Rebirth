using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Health Potion", menuName = "Scriptable Objects/Items/Health Potion")]
public class HealthPotion : ItemDataSO
{
    [SerializeField] private float health;
    public override void PickUp(GameObject player)
    {
        player.GetComponent<PlayerVitality>().RecoverHealth(Random.Range(1,health));
    }
}
