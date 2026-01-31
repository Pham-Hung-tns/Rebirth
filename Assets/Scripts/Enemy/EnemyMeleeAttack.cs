using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour, IAttackable
{
    private EnemyController enemyController;
    private EnemyConfig enemyConfig;
    
    public event System.Action OnAttackComplete;

    public void Initialized(EnemyController controller, EnemyConfig enemyConfig)
    {
        enemyController = controller;
        this.enemyConfig = enemyConfig;
    }

    public void StartAttack()
    {
        enemyController.ChangeAnimationState(Settings.ATTACK_STATE);
        // Đơn giản, chỉ setup animation
        // Animation event sẽ call OnAnimationAttackEnd()
    }

    public void ReleaseAttack()
    {
        // Không cần implement
    }

    /// <summary>
    /// Gọi từ Animation Event để trigger damage
    /// Đặt tại frame khi enemy contact với player
    /// </summary>
    public void OnAnimationDamage()
    {
        DetectAndDamageNearby();
    }

    /// <summary>
    /// Gọi từ Animation Event khi animation tấn công kết thúc
    /// Đặt trong frame cuối cùng của attack animation
    /// </summary>
    public void OnAnimationAttackEnd()
    {
        NotifyAttackComplete();
    }

    private void DetectAndDamageNearby()
    {
        // // Phát hiện target trong vùng tấn công
        // RaycastHit2D[] targets = Physics2D.CircleCastAll(enemyController.transform.position + Vector3.right * (enemyController.Spr.flipX ? -1 : 1), enemyConfig.attackRange, Vector2.one, enemyConfig.attackRange, enemyConfig.playerLayer);

        // foreach (RaycastHit2D target in targets)
        // {
        //     if (target.collider.gameObject == enemyController.gameObject) continue;
        //     ITakeDamage damageable = target.GetComponent<ITakeDamage>();
        //     if (damageable != null)
        //     {
        //         // Tính hướng knockback
        //         Vector2 knockDir = (target.transform.position - enemyController.transform.position).normalized;
        //         damageable.TakeDamage(enemyConfig.damageAmount, enemyController.gameObject, enemyConfig.knockbackDir, enemyConfig.knockbackForce);
        //     }
        // }
    }

    private void NotifyAttackComplete()
    {
        if (enemyController != null)
        {
            enemyController.OnAttackComplete();
        }
        OnAttackComplete?.Invoke();
    }
}
