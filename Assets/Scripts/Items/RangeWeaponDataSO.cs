using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Range Weapon", menuName = "Items/Range Weapon")]
public class RangeWeaponDataSO : WeaponDataSO
{

    [Header("Projectile Info")]
    public GameObject projectilePrefab; // Viên đạn
    public float projectileSpeed = 15f;
    public float projectileLifetime = 5f;

    [Header("Shooting Pattern")]
    public int projectileCount = 1; // Số lượng đạn bắn ra 1 lúc (Shotgun = 5)
    [Range(0, 360)] public float spreadAngle = 0f; // Góc tản đạn (Shotgun = 45 độ)
}
