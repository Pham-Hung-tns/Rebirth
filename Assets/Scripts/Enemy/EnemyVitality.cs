using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVitality : MonoBehaviour, ITakeDamage
{

    public static event Action<Transform> OnEnemyKilledEvent;
    public static event Action OnChangeState;
    private float enemyHealth;
    private Coroutine coroutine;

    public float Health { get => enemyHealth; set => enemyHealth = value; }

    // Start is called before the first frame update
    public void Initialized(EnemyConfig config)
    {
        enemyHealth = config.Health;
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

        Debug.Log("Enemy Take Damage with knockback");
        AudioManager.Instance.PlaySFX("Enemy_Damage");
        enemyHealth -= amount;
        DamageManager.Instance.ShowDmg(amount, transform);
        if(coroutine != null)
        {
            coroutine = null;
        }
        coroutine =  StartCoroutine(IEChangeColor());

        if(enemyHealth <= 0)
        {
            OnEnemyKilledEvent?.Invoke(transform);
            OnChangeState?.Invoke();
            Destroy(gameObject);
        }
    }
}
