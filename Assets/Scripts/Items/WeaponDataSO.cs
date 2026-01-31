using UnityEngine;


public class WeaponDataSO : ItemDataSO
{
    public enum WeaponType
    {
        Melee,
        Gun,
        Staff
    }

    public enum WeaponRarity
    {
        Normal,
        Rare,
        Epic, // su thi
        Legend
    }

    [Header("Stat")]
    public int damage;
    public int energy;

    public WeaponType weaponType;
    public WeaponRarity weaponRarity;
    public Transform firePoint; // Điểm bắn
    public float cooldown;
    public Weapon weapon;

    [Header("Charge Mechanics")]
    public bool canCharge;
    public float maxChargeTime = 2f; // Thời gian tụ lực tối đa
    public float minChargeDamageMultiplier = 0.5f; // Sát thương khi chưa tụ lực
    public float maxChargeDamageMultiplier = 2.0f; // Sát thương khi tụ lực full
    public override void PickUp()
    {
        LevelManager.Instance.SelectedPlayer.GetComponent<PlayerWeapon>().EquipWeapon(weapon);
    }
}
