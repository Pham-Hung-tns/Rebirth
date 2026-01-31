using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : CharacterWeapon, IAttackable
{
    private EnemyController enemyController;
    
    public event System.Action OnAttackComplete;

    public void Initialized(EnemyController controller, EnemyConfig enemyConfig)
    {
        enemyController = controller;
        if (enemyConfig?.initialWeapon != null)
        {
            CreateWeapon(enemyConfig.initialWeapon);
        }
    }
  
    public override void CreateWeapon(Weapon initalWeapon)
    {
        currentWeapon = Instantiate(initalWeapon, weaponPosition.position, Quaternion.identity, 
            weaponPosition);
        equippedWeapons[weaponIndex] = currentWeapon;
        equippedWeapons[weaponIndex].Character = this;
    }

    public override void StartAttack()
    {
        base.StartAttack();
        
        // Nếu là charged weapon, dùng coroutine để đợi charge complete
        if (currentWeapon != null && currentWeapon.WeaponData.canCharge)
        {
            StartCoroutine(WaitForChargeComplete(currentWeapon.WeaponData.maxChargeTime));
        }
        else
        {
            NotifyAttackComplete();
        }
    }

    private IEnumerator WaitForChargeComplete(float chargeTime)
    {
        yield return new WaitForSeconds(chargeTime);
        ReleaseAttack();
        NotifyAttackComplete();
    }

    private void NotifyAttackComplete()
    {
        if (enemyController != null)
        {
            enemyController.OnAttackComplete();
        }
        OnAttackComplete?.Invoke();
    }

    public void RotateWeaponToPlayer(Vector3 dir)
    {
        RotateWeaponToAgent(dir);
    }

    public void DestroyWeapon()
    {

    }
}
