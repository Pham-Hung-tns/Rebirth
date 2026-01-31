using System;
using System.Collections;
using UnityEngine;

public class PlayerWeapon : CharacterWeapon
{
    public static event Action<Weapon> OnShowUIWeaponEvent;
    
    private PlayerConfig playerConfig;
    private PlayerVitality playerVitality;

    private Coroutine weaponCoroutine;
    private ItemText weaponNameText;

    private SpriteRenderer spriteRenderer;
    private DetectionEnemy detection;

    private Vector2 movementDirection;
    public Vector2 MovementDirection { get => movementDirection; set => movementDirection = value; }
    //public Weapon InitialWeapon { get => initialWeapon; set => initialWeapon = value; }


    public void Initialize(PlayerConfig _data, SpriteRenderer _sp, PlayerVitality _vitality, DetectionEnemy _detection)
    {
        playerConfig = _data;
        spriteRenderer = _sp;
        playerVitality = _vitality;
        detection = _detection;
    }

    public override void CreateWeapon(Weapon weaponPrefab)
    {
        currentWeapon = Instantiate(weaponPrefab, weaponPosition.position,
            Quaternion.identity, weaponPosition);
        equippedWeapons[weaponIndex] = currentWeapon;
        equippedWeapons[weaponIndex].Character = this;
        //ShowCurrentWeaponName();
        OnShowUIWeaponEvent?.Invoke(currentWeapon);
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (equippedWeapons[0] == null)
        {
            CreateWeapon(weapon);
            ResetWeaponForChange();
            return;
        }

        if (equippedWeapons[1] == null)
        {
            weaponIndex++;
            equippedWeapons[0].gameObject.SetActive(false);
            CreateWeapon(weapon);
            ResetWeaponForChange();
            return;
        }

        // Destroy current weapon
        currentWeapon.DestroyWeapon();
        equippedWeapons[weaponIndex] = null;

        // Create new weapon
        CreateWeapon(weapon);
        ResetWeaponForChange();
    }

    private void ChangeWeapon()
    {
        if (equippedWeapons[1] == null) return;
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            equippedWeapons[i].gameObject.SetActive(false);
        }

        weaponIndex = 1 - weaponIndex;
        currentWeapon = equippedWeapons[weaponIndex];
        currentWeapon.gameObject.SetActive(true);
        ResetWeaponForChange();
        ShowCurrentWeaponName();
        OnShowUIWeaponEvent?.Invoke(currentWeapon);
    }


    //public void StopShooting()
    //{
    //    CancelInvoke(nameof(Shoot));
    //    CancelInvoke(nameof(AutoFire));
    //    return; 
    //}

    //public void Shoot()
    //{
    //    Debug.Log("Shoot weapon");
    //    //if (currentWeapon == null)
    //    //    return;
    //    //if (CanUseWeapon())
    //    //{
    //    //    currentWeapon.UseWeapon();
    //    //    playerVitality.TryConsumeEnergy(currentWeapon.WeaponData.energy);
    //    //}
    //    //else
    //    //    return;
    //}

    public int GetDamageCritical()
    {
        float damage = currentWeapon.WeaponData.damage;
        float per = UnityEngine.Random.Range(0, 100f);
        if (per <= playerConfig.CriticalChance)
        {
            return Mathf.RoundToInt(damage + (playerConfig.CriticalDamage / 100) * damage);
        }
        return Mathf.RoundToInt(damage);
    }

    public void RotateWeapon()
    {
        if (movementDirection != Vector2.zero && currentWeapon != null)
        {
            RotateWeaponToAgent(movementDirection);
        }

        if (detection != null && detection.EnemyTarget != null)
        {
            Vector3 dirToEnemy = detection.EnemyTarget.transform.position - transform.position;
            RotateWeaponToAgent(dirToEnemy);
        }

    }
    private void ShowCurrentWeaponName()
    {
        if (weaponCoroutine != null)
        {
            StopCoroutine(weaponCoroutine);
        }

        if (weaponNameText != null && weaponNameText.gameObject.activeInHierarchy)
        {
            Destroy(weaponNameText.gameObject);
        }

        weaponCoroutine = StartCoroutine(IEShowName());
    }

    private IEnumerator IEShowName()
    {
        Vector3 textPos = transform.position + Vector3.up;
        Color weaponNameColor = GameManager.Instance.
            ChooseColorForWeapon(currentWeapon.WeaponData);
        weaponNameText = ItemTextManager.Instance
            .ShowName(currentWeapon.WeaponData.name, weaponNameColor,
                textPos);
        weaponNameText.transform.SetParent(transform);
        yield return new WaitForSeconds(2f);
        Destroy(weaponNameText.gameObject);
    }
    public bool CanUseWeapon()
    {
        if (currentWeapon.WeaponData.weaponType == WeaponDataSO.WeaponType.Gun && playerVitality.TryConsumeEnergy(currentWeapon.WeaponData.energy))
            return true;
        if (currentWeapon.WeaponData.weaponType == WeaponDataSO.WeaponType.Melee)
            return true;

        return false;

    }
    private void ResetWeaponForChange()
    {
        Transform weaponTransform = currentWeapon.transform;
        weaponTransform.rotation = Quaternion.identity;
        //weaponTransform.localScale = Vector3.one;
        weaponPosition.rotation = Quaternion.identity;
        //weaponPosition.localScale = Vector3.one;
        spriteRenderer.flipX = false;
    }

    private void OnEnable()
    {
        // triển khai thêm interaction cho player (Sau khi thêm UI)
        //actions.Enable();
        //actions.Interaction.ChangeItem.performed += ctx => ChangeWeapon();
    }
    private void OnDisable()
    {
        //actions.Disable();
        //actions.Interaction.ChangeItem.performed += ctx => ChangeWeapon();
    }

    // New: override StartAttack to support auto-fire for non-charge guns
    public override void StartAttack()
    {
        if (currentWeapon == null || currentWeapon.WeaponData == null) return;

        // If weapon supports charging, fall back to base (charge flow)
        if (currentWeapon.WeaponData is RangeWeaponDataSO rangeData && rangeData.canCharge)
        {
            base.StartAttack();
            return;
        }

        // For guns (non-charge), start auto-fire using InvokeRepeating with cooldown interval
        if (currentWeapon.WeaponData.weaponType == WeaponDataSO.WeaponType.Gun)
        {
            // Fire immediately and then repeat at cooldown interval
            AutoFire();
            float interval = Mathf.Max(0.01f, currentWeapon.WeaponData.cooldown);
            if (!IsInvoking(nameof(AutoFire)))
                InvokeRepeating(nameof(AutoFire), interval, interval);
            return;
        }

        // Default: use base behavior for melee or other types
        base.StartAttack();
    }

    // New: override ReleaseAttack to stop auto-fire and handle charge release
    public override void ReleaseAttack()
    {
        if (currentWeapon == null || currentWeapon.WeaponData == null)
            return;

        // If weapon supports charging, let base handle release (calculate multiplier)
        if (currentWeapon.WeaponData is RangeWeaponDataSO rangeData && rangeData.canCharge)
        {
            base.ReleaseAttack();
            return;
        }

        // For non-charge guns, stop auto-fire
        if (currentWeapon.WeaponData.weaponType == WeaponDataSO.WeaponType.Gun)
        {
            CancelInvoke(nameof(AutoFire));
            return;
        }

        // Default
        base.ReleaseAttack();
    }

    private void AutoFire()
    {
        if (currentWeapon == null || currentWeapon.WeaponData == null)
        {
            CancelInvoke(nameof(AutoFire));
            return;
        }

        // For guns, ensure we have energy to fire each shot
        if (currentWeapon.WeaponData.weaponType == WeaponDataSO.WeaponType.Gun)
        {
            if (!CanUseWeapon())
            {
                // If cannot use (no energy), stop auto-fire
                CancelInvoke(nameof(AutoFire));
                return;
            }
        }

        // Execute attack with multiplier 1
        currentWeapon.ExecuteAttack(1f);
        _lastAttackTime = Time.time;
    }
}
