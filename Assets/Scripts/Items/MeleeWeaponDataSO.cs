using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Scriptable Objects/Items/Melee Weapon")]
public class MeleeWeaponDataSO : WeaponDataSO
{
    [Header("Melee Specific")]
    public float attackRange = 1.5f; // Bán kính vùng đánh
    public float knockbackForce = 5f; // Lực đẩy lùi

    [Header("Energy Wave (Optional)")]
    public bool hasEnergyWave;
    public GameObject energyWavePrefab; // Prefab sóng năng lượng (như Projectile)
    public float waveSpeed = 10f;
}
