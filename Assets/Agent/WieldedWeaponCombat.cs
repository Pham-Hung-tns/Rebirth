using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WieldedWeaponCombat : MonoBehaviour
{
    private GameObject owner; // Tham chiếu đến Player hoặc Enemy cha

    [SerializeField] protected Transform weaponPosition;
    [SerializeField] protected float rotationSpeed = 10f; // Tốc độ xoay súng/mũi đánh] 

    [Header("Juicing")]
    [Tooltip("Feedback phát ra tiếng khi bắt đầu gồng sức (ví dụ: tiếng kéo cung)")]
    [SerializeField] protected FeedbackPlayer chargeFeedback;

    protected Weapon currentWeapon;
    protected int weaponIndex; // 0 - 1
    protected Weapon[] equippedWeapons = new Weapon[2];

    protected SpriteRenderer sp;

    protected float _lastAttackTime;
    protected bool _isCharging;
    protected float _chargeStartTime;

    // Public accessor for owner to be used by weapons/projectiles
    public GameObject Owner => owner ?? (owner = this.gameObject);

    public Transform WeaponPosition { get => weaponPosition; set => weaponPosition = value; }


    // Biến lưu vị trí gốc để biết đường mà đảo ngược
    private Vector3 _defaultLocalPosition;
    protected bool _isFacingRight = true; // Mặc định ban đầu là nhìn về bên phải
    protected virtual void Awake()
    {
        // Lưu lại vị trí setup ban đầu (ví dụ: x=0.5, y=0.2)
        if (weaponPosition != null)
        {
            _defaultLocalPosition = weaponPosition.localPosition;
        }
        // auto-assign owner to the GameObject this component is attached to if not set in inspector
        if (owner == null)
        {
            owner = this.gameObject;
        }
    }


    // vì việc khởi tạo vũ khí sẽ nằm trong file controller nên hàm này sẽ public để các controller có thể gọi
    public virtual void CreateWeapon(Weapon weaponPrefab)
    {
        
    }

    /// <summary>
    /// Reset toàn bộ weapon state về ban đầu — dùng khi pool re-use.
    /// </summary>
    protected void ResetWeaponState()
    {
        if (weaponPosition != null)
        {
            weaponPosition.localPosition = _defaultLocalPosition;
            weaponPosition.localRotation = Quaternion.identity;
            weaponPosition.localScale = Vector3.one;
        }

        _isFacingRight = true;
        _lastAttackTime = 0f;
        _isCharging = false;
        _chargeStartTime = 0f;

        if (chargeFeedback != null)
        {
            chargeFeedback.CompleteFeedbacks();
        }
    }

    protected void FlipWeapon(Vector3 dir)
    {
        // --- 1. XÁC ĐỊNH TRẠNG THÁI (QUAN TRỌNG) ---
        // Nếu x < -0.01f (Rõ ràng đang sang trái) -> Ghi nhận là nhìn Trái
        if (dir.x < -0.01f)
        {
            _isFacingRight = false;
        }
        // Nếu x > 0.01f (Rõ ràng đang sang phải) -> Ghi nhận là nhìn Phải
        else if (dir.x > 0.01f)
        {
            _isFacingRight = true;
        }

        // --- 2. XỬ LÝ VỊ TRÍ & LẬT HÌNH (DỰA TRÊN BIẾN TRẠNG THÁI) ---
        if (!_isFacingRight)
        {
            // --- Đang trạng thái TRÁI ---
            weaponPosition.localScale = new Vector3(1, -1, 1);
            weaponPosition.localPosition = new Vector3(-_defaultLocalPosition.x, _defaultLocalPosition.y, _defaultLocalPosition.z);
        }
        else
        {
            // --- Đang trạng thái PHẢI ---
            weaponPosition.localScale = new Vector3(1, 1, 1);
            weaponPosition.localPosition = _defaultLocalPosition;
        }
    }

    protected void RotateWeaponToAgent(Vector3 dir)
    {
        // 1. Lật vũ khí trước
        FlipWeapon(dir);

        // 2. Xử lý xoay mượt (Rotation)
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        weaponPosition.rotation = Quaternion.Slerp(weaponPosition.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }




    // --- Public Methods cho Input System / AI gọi ---

    // Gọi khi bắt đầu nhấn nút tấn công
    public virtual void StartAttack()
    {
        if (currentWeapon == null || currentWeapon.WeaponData == null) return;

        if (Time.time < _lastAttackTime + currentWeapon.WeaponData.cooldown) return;

        if (currentWeapon.WeaponData.canCharge)
        {
            _isCharging = true;
            _chargeStartTime = Time.time;
            // set animator to charging state
            currentWeapon.ChangeAnimationState(Settings.BOW_CHARGING);

            if (chargeFeedback != null)
            {
                chargeFeedback.PlayFeedbacks();
            }
        }
        else
        {
            // Tấn công ngay lập tức (Melee hoặc Súng thường)
            ExecuteAttack(1f);
        }
    }

    // Gọi khi thả nút tấn công (Dành cho cung tên/Charged weapon)
    public virtual void ReleaseAttack()
    {
        if (currentWeapon == null || currentWeapon.WeaponData == null) return;

        if (_isCharging)
        {
            _isCharging = false;

            if (chargeFeedback != null)
            {
                chargeFeedback.CompleteFeedbacks();
            }

            // Tính toán lực tụ được (từ 0 đến 1)
            float chargeDuration = Mathf.Clamp(Time.time - _chargeStartTime, 0, currentWeapon.WeaponData.maxChargeTime);
            float chargeRatio = chargeDuration / currentWeapon.WeaponData.maxChargeTime;

            // Tính Damage Multiplier dựa trên lực tụ
            float damageMultiplier = Mathf.Lerp(currentWeapon.WeaponData.minChargeDamageMultiplier, currentWeapon.WeaponData.maxChargeDamageMultiplier, chargeRatio);
            // turn off charging animator flag and trigger attack
            currentWeapon.ChangeAnimationState(Settings.BOW_RELEASE);

            ExecuteAttack(damageMultiplier);
        }
    }

    // --- Core Logic ---

    protected void ExecuteAttack(float damageMultiplier)
    {
        _lastAttackTime = Time.time;

        // Delegate actual attack behaviour to current weapon
        currentWeapon.ExecuteAttack(damageMultiplier);
    }
}
