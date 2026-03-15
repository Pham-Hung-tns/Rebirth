using System;
using System.Collections;
using UnityEngine;

public class EnemyVitality : MonoBehaviour, ITakeDamage, IPoolable
{
    public static event Action<Transform> OnEnemyKilledEvent;
    public static event Action OnChangeState;
    private float enemyHealth;
    private float initialHealth; // Lưu HP ban đầu để reset khi pool spawn
    private Coroutine coroutine;
    private bool isDead; // Guard chống fire OnEnemyKilledEvent nhiều lần

    public float Health { get => enemyHealth; set => enemyHealth = value; }

    public void Initialized(EnemyConfig config)
    {
        enemyHealth = config.Health;
        initialHealth = config.Health;
    }

    private IEnumerator IEChangeColor()
    {
        //spr.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        //spr.color = initialColor;
    }

    public void TakeDamage(int amount, GameObject attacker, Vector2 knockbackDir, float knockbackForce)
    {
        // Implement knockback
        if (knockbackForce > 0)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        AudioManager.Instance.PlaySFX("Enemy_Damage");
        enemyHealth -= amount;
        DamageManager.Instance.ShowDmg(amount, transform);
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        coroutine = StartCoroutine(IEChangeColor());

        if(enemyHealth <= 0 && !isDead)
        {
            isDead = true; // Đảm bảo chỉ fire 1 lần duy nhất
            OnEnemyKilledEvent?.Invoke(transform);
            OnChangeState?.Invoke();

            // Trả về pool thay vì Destroy
            if (ObjPoolManager.Instance != null)
            {
                ObjPoolManager.Instance.ReturnToPool(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    #region IPoolable

    /// <summary>
    /// Reset HP và state khi được lấy ra từ pool.
    /// </summary>
    public void OnPoolSpawn()
    {
        enemyHealth = initialHealth;
        isDead = false;

        // Stop coroutine đang chạy từ lần sử dụng trước
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        // Reset velocity nếu có rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// Cleanup khi trả về pool.
    /// </summary>
    public void OnPoolDespawn()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    #endregion
}
