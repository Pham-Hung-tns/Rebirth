using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [SerializeField] private float radiusAttack;
    [SerializeField] private Transform positionAttack;
    [SerializeField] private float knockBackSpeed;
    [SerializeField] private float knockBackDelay;

    public override void DestroyWeapon()
    {
        Destroy(gameObject);
    }

    public void DetectedCharacter()
    {
        Collider2D[] Characters = null;
        if (Character is PlayerWeapon player)
        {
            Characters =  Physics2D.OverlapCircleAll(positionAttack.position, radiusAttack, LayerMask.GetMask(LAYER_ENEMY));
        }
        else
        {
            Characters = Physics2D.OverlapCircleAll(positionAttack.position, radiusAttack, LayerMask.GetMask(LAYER_PLAYER));
        }

        if (Characters.Length > 0) {
            StopAllCoroutines();
            foreach (Collider2D collider in Characters)
            {
                ITakeDamage obj = collider.GetComponent<ITakeDamage>();
                if (obj != null)
                {
                    Vector3 knockBack = (collider.transform.position - Character.Owner.transform.position).normalized;
                    Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.AddForce(knockBack * knockBackSpeed, ForceMode2D.Force);
                        StartCoroutine(IEStopKnockBack(knockBackDelay, rb));
                    }

                    if (weaponData is MeleeWeaponDataSO meleeData)
                    {
                        int damage = Mathf.RoundToInt(meleeData.damage);
                        if (Character is PlayerWeapon playerWeapon)
                        {
                            damage = playerWeapon.GetDamageCritical();
                        }

                        obj.TakeDamage(damage, Character.Owner, new Vector2(knockBack.x, knockBack.y), meleeData.knockbackForce);

                        if (meleeData.hasEnergyWave && meleeData.energyWavePrefab)
                        {
                            SpawnEnergyWave(meleeData.energyWavePrefab, positionAttack.position, positionAttack.rotation, meleeData.waveSpeed, damage);
                        }
                    }
                }
            }
        }
    }

    private IEnumerator IEStopKnockBack(float delay, Rigidbody2D rb)
    {
        yield return new WaitForSeconds(delay);
        rb.velocity = Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(positionAttack.position, radiusAttack);
    }

    // New: ExecuteAttack invoked from CharacterWeapon
    public override void ExecuteAttack(float damageMultiplier = 1f)
    {
        // Use weaponData if present
        if (!(weaponData is MeleeWeaponDataSO meleeData))
            return;

        // Play attack sound
        PlayAttackSFX();

        // Determine damage (consider character critical for player)
        int baseDamage = Mathf.RoundToInt(meleeData.damage * damageMultiplier);
        if (Character is PlayerWeapon player)
        {
            baseDamage = Mathf.RoundToInt(player.GetDamageCritical() * damageMultiplier);
        }

        // Detect targets
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(positionAttack.position, meleeData.attackRange);

        foreach (Collider2D hit in hitEnemies)
        {
            if (hit.gameObject == Character.Owner) continue;

            ITakeDamage damageable = hit.GetComponent<ITakeDamage>();
            if (damageable != null)
            {
                Vector2 knockbackDir = (hit.transform.position - Character.Owner.transform.position).normalized;
                damageable.TakeDamage(baseDamage, Character.Owner, knockbackDir, meleeData.knockbackForce);

                if (meleeData.hasEnergyWave && meleeData.energyWavePrefab)
                {
                    SpawnEnergyWave(meleeData.energyWavePrefab, positionAttack.position, positionAttack.rotation, meleeData.waveSpeed, baseDamage);
                }
            }
        }
    }

    private void SpawnEnergyWave(GameObject prefab, Vector3 pos, Quaternion rot, float speed, int damage)
    {
        GameObject projObj = Instantiate(prefab, pos, rot);
        Projectile proj = projObj.GetComponent<Projectile>();
        if (proj)
        {
            proj.Initialize(Character.Owner, speed, Mathf.RoundToInt(WeaponData.damage), projObj.transform.right, 0f);
        }
    }
}
