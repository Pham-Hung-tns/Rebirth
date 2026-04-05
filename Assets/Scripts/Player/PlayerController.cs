using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerVitality))]
//[RequireComponent(typeof(PlayerWeapon))]
//[RequireComponent(typeof(PlayerSkill))]

public class PlayerController : CharacterController
{
    [Header("Dependencies")]
    [SerializeField] private InputReader inputReader; // Kéo file SO vào đây
    [SerializeField] private PlayerConfig playerData; // Kéo file Data vào đây

    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerVitality _vitality;
    private ICombatBehavior _combat;
    [SerializeField] private DetectionEnemy _detection;
    //private PlayerSkill _skill;

    private Vector2 _moveInput;
    private NPCInteractable _currentInteractable; // Thêm biến lưu NPC đang ở gần

    // ── Audio State ────────────────────────────────────────────────────────────
    /// <summary>Flag để kiểm soát khi nào cho phép phát âm thanh di chuyển.
    /// Chỉ được phát khi player được đặt vào màn chơi mới thành công.</summary>
    private bool isPlayerPlacedInLevel = false;

    public PlayerConfig PlayerData { get => playerData; set => playerData = value; }
    public DetectionEnemy Detection => _detection;
    public bool IsPlayerPlacedInLevel => isPlayerPlacedInLevel;

    private void Awake()
    {
        // Dependency Injection: Đẩy dữ liệu vào các module con
        _movement.Initialize(rigidBody2D, Spr, PlayerData);
        _vitality.Initialize(PlayerData);
        _combat = GetComponent<ICombatBehavior>();
        // _skill.Initialize(...);
    }

    private void Start()
    {
        // Weapon / Combat initialization happens internally in their own Start methods
    }

    private void Update()
    {
        // Controller ra lệnh cho Movement module di chuyển mỗi khung hình
        //_movement.CalculateSpeed(_moveInput);
        // Nếu có enemy trong tầm, ưu tiên quay mặt về phía enemy thay vì dựa vào input di chuyển
        Vector2 facingDirection = _moveInput;
        if (_detection != null && _detection.EnemyTarget != null)
        {
            Vector3 enemyPos = _detection.EnemyTarget.transform.position;
            facingDirection = (enemyPos - transform.position);
        }

        _movement.RotationPlayer(facingDirection);

        _combat?.HandleAiming(facingDirection);
    }

    private void FixedUpdate()
    {
        _movement.HandleMovement(_moveInput);
    }

    // --- Event Handlers ---

    protected override void OnMove(Vector2 direction)
    {
        _moveInput = direction;
        ChangeAnimationState(_moveInput != Vector2.zero ? Settings.PLAYER_RUN : Settings.PLAYER_IDLE);
    }

    

    protected override void OnAttack(bool canAttack)
    {
        // Delegate attack input to IAttackable component
        if (canAttack)
        {
            _combat?.StartAttack();
        }
        else
        {
            _combat?.ReleaseAttack();
        }
    }

    protected override void OnSkill(bool canUseSkill)
    {
        ChangeAnimationState(Settings.PLAYER_SKILL);
    }

    // Gán NPC đang ở gần
    public void SetInteractable(NPCInteractable interactable)
    {
        _currentInteractable = interactable;
    }

    // Bắt sự kiện Interact từ InputReader
    private void OnInteract()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.Interact();
        }
    }


    
    private void OnEnable()
    {
        // Reset input và speed để tránh player tự di chuyển sau portal transition
        _moveInput = Vector2.zero;
        _movement.ResetSpeed();

        // Đăng ký nhận lệnh từ Input Reader
        inputReader.MoveEvent += OnMove;
        inputReader.AttackEvent += OnAttack;
        inputReader.SkillEvent += OnSkill;
        inputReader.InteractEvent += OnInteract;
    }

    private void OnDisable()
    {
        inputReader.MoveEvent -= OnMove;
        inputReader.AttackEvent -= OnAttack;
        inputReader.SkillEvent -= OnSkill;
        inputReader.InteractEvent -= OnInteract;
    }

    // ── Audio Control ──────────────────────────────────────────────────────────

    /// <summary>Đánh dấu rằng player đã được đặt vào màn chơi mới.
    /// Sau khi gọi hàm này, âm thanh di chuyển (tiếng bước chân) mới được phát.</summary>
    public void SetPlayerPlacedInLevel(bool placed)
    {
        isPlayerPlacedInLevel = placed;
    }
}

