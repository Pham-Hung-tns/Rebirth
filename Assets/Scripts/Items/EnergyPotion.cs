using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Energy Potion", menuName = "Scriptable Objects/Items/Energy Potion")]
public class EnergyPotion : ItemDataSO
{
    [SerializeField] private float energy;
    public override void PickUp(GameObject player)
    {
        player.GetComponent<PlayerVitality>().RecoverEnergy(Random.Range(10, energy));
    }
}
