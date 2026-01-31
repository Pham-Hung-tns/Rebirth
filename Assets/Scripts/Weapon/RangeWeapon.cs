using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeWeapon : Weapon
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] protected Transform shootTrans;

    public override void DestroyWeapon()
    {
        Destroy(gameObject);
    }

    public override void ExecuteAttack(float damageMultiplier = 1f)
    {
        if (!(weaponData is RangeWeaponDataSO rangeData)) return;

        // Play attack sound
        PlayAttackSFX();

        float startAngle = -rangeData.spreadAngle / 2f;
        float angleStep = rangeData.projectileCount > 1 ? rangeData.spreadAngle / (rangeData.projectileCount - 1) : 0f;

        for (int i = 0; i < rangeData.projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = shootTrans.rotation * Quaternion.Euler(0, 0, currentAngle);

            // Use pool if available
            Projectile bullet = null;
            if (ObjPoolManager.Instance != null && projectilePrefab != null)
            {
                bullet = ObjPoolManager.Instance.Initialization(projectilePrefab);
                bullet.transform.position = shootTrans.position;
                bullet.transform.rotation = rotation;
                bullet.Direction = bullet.transform.right;
                bullet.gameObject.SetActive(true);
            }
            else if (projectilePrefab != null)
            {
                Projectile p = Instantiate(projectilePrefab, shootTrans.position, rotation);
                p.Direction = p.transform.right;
                bullet = p;
            }

            if (bullet != null)
            {
                int baseDamage = Mathf.RoundToInt(rangeData.damage * damageMultiplier);
                if (Character is PlayerWeapon player)
                {
                    baseDamage = Mathf.RoundToInt(player.GetDamageCritical() * damageMultiplier);
                }

                // Get knockback info from WeaponDataSO
                Vector2 knockbackDir = bullet.Direction;
                float knockbackForce = 0f; // Range weapons không có knockback mặc định
                
                // Initialize projectile with damage and knockback info from WeaponDataSO
                bullet.Initialize(Character.Owner, rangeData.projectileSpeed, baseDamage, knockbackDir, knockbackForce);
            }
        }
    }

    public void StartCharge()
    {
        PlayChargeSFX();
    }

    public void StopCharge()
    {
        // optional: stop charge SFX if using looping audio
    }
}
