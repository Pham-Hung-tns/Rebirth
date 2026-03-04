using System;
using System.Collections;
using UnityEngine;

public class PlayerVitality : MonoBehaviour, ITakeDamage
{
    // 1 player nên dùng static oke. Event này dùng để show Panel khi player chết
    public static event Action OnPlayerDeathEvent;

    private PlayerConfig _data;

    private float timer;
    private Coroutine armorCooldownCoroutine;

    // Sử dụng Property để UI có thể lắng nghe sự thay đổi nếu cần
    public float CurrentHealth { get; private set; }
    public float CurrentArmor { get; private set; }
    public float CurrentEnergy { get; private set; }

    public void Initialize(PlayerConfig data)
    {
        _data = data;
        CurrentHealth = _data.MaxHealth;
        CurrentArmor = _data.MaxArmor;
        CurrentEnergy = _data.MaxEnergy;
        NotifyStatsChanged();
    }

    #region Notify UI (giảm duplication)

    /// <summary>
    /// Gửi event cập nhật UI. Gọi một lần duy nhất mỗi khi stats thay đổi.
    /// Trước đây đoạn này bị copy-paste 5 lần.
    /// </summary>
    private void NotifyStatsChanged()
    {
        UIEvents.OnPlayerStatsChanged?.Invoke(new PlayerStatsData
        {
            curHp = CurrentHealth,
            maxHp = _data.MaxHealth,
            curArmor = CurrentArmor,
            maxArmor = _data.MaxArmor,
            curEnergy = CurrentEnergy,
            maxEnergy = _data.MaxEnergy
        });
    }

    #endregion

    #region Player Health Methods

    public void TakeDamage(int amount, GameObject attacker, Vector2 knockbackDir, float knockbackForce)
    {
        // nếu nhận damage, đặt lại thời gian hồi giáp
        ResetArmorCooldown();

        // trừ giáp trước, trừ máu sau
        if (CurrentArmor > 0)
        {
            float remainingDamage = amount - CurrentArmor;
            CurrentArmor = Mathf.Max(CurrentArmor - amount, 0f);
            if (remainingDamage > 0)
            {
                CurrentHealth = Mathf.Max(CurrentHealth - remainingDamage, 0f);
            }
        }
        else
        {
            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
        }

        NotifyStatsChanged();

        // Apply knockback if force > 0
        if (knockbackForce > 0)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        if (CurrentHealth <= 0f)
        {
            OnPlayerDeathEvent?.Invoke();
            UIEvents.OnShowGameOver?.Invoke();
        }
    }

    public void RecoverHealth(float amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, _data.MaxHealth);
        NotifyStatsChanged();
    }

    #endregion

    #region Player Energy Methods

    public void RecoverEnergy(float amount)
    {
        CurrentEnergy = Mathf.Min(CurrentEnergy + amount, _data.MaxEnergy);
        NotifyStatsChanged();
    }

    public bool TryConsumeEnergy(float amount)
    {
        if (CurrentEnergy >= amount)
        {
            CurrentEnergy -= amount;
            NotifyStatsChanged();
            return true;
        }
        return false;
    }

    #endregion

    #region Player Armor Methods

    private void ResetArmorCooldown()
    {
        CancelInvoke();
        timer = _data.timeCooldownArmor;
        if (armorCooldownCoroutine != null)
        {
            StopCoroutine(armorCooldownCoroutine);
        }
        armorCooldownCoroutine = StartCoroutine(ArmorCooldownRoutine());
    }

    private IEnumerator ArmorCooldownRoutine()
    {
        yield return new WaitForSeconds(timer);
        InvokeRepeating(nameof(RecoverArmor), 0.1f, _data.timeRecoverArmor);
    }

    private void RecoverArmor()
    {
        if (CurrentArmor >= _data.MaxArmor)
        {
            CurrentArmor = _data.MaxArmor;
            CancelInvoke();
            return;
        }
        CurrentArmor += 1;
        NotifyStatsChanged();
    }

    #endregion
}
