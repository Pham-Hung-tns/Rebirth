using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerVitality))]
[RequireComponent(typeof(PlayerWeapon))]
//[RequireComponent(typeof(PlayerSkill))]

public class PlayerController : CharacterController
{
    [Header("Dependencies")]
    [SerializeField] private InputReader inputReader; // Kéo file SO vào đây
    [SerializeField] private PlayerConfig playerData; // Kéo file Data vào đây

    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerVitality _vitality;
    [SerializeField] private PlayerWeapon _combat;
    [SerializeField] private DetectionEnemy _detection;
    //private PlayerSkill _skill;

    private Vector2 _moveInput;

    public PlayerConfig PlayerData { get => playerData; set => playerData = value; }

    private void Awake()
    {
        // Dependency Injection: Đẩy dữ liệu vào các module con
        _movement.Initialize(rigidBody2D, Spr, PlayerData);
        _vitality.Initialize(PlayerData);
        _combat.Initialize(PlayerData, Spr, _vitality, _detection);
        // _skill.Initialize(...);
    }

    private void Start()
    {
        if(PlayerData.initialWeapon != null)
            _combat.CreateWeapon(PlayerData.initialWeapon);
    }

    private void Update()
    {
        // Controller ra lệnh cho Movement module di chuyển mỗi khung hình
        _movement.CalculateSpeed(_moveInput);
        // Nếu có enemy trong tầm, ưu tiên quay mặt về phía enemy thay vì dựa vào input di chuyển
        Vector2 facingDirection = _moveInput;
        if (_detection != null && _detection.EnemyTarget != null)
        {
            Vector3 enemyPos = _detection.EnemyTarget.transform.position;
            facingDirection = (enemyPos - transform.position);
        }

        _movement.RotationPlayer(facingDirection);

        _combat.RotateWeapon();
    }

    private void FixedUpdate()
    {
        _movement.HandleMovement(_moveInput);
    }

    // --- Event Handlers ---

    protected override void OnMove(Vector2 direction)
    {
        _moveInput = direction;
        _combat.MovementDirection = direction;
        ChangeAnimationState(_moveInput != Vector2.zero ? Settings.PLAYER_RUN : Settings.PLAYER_IDLE);
    }

    protected override void OnAttack(bool canAttack)
    {
        // Delegate attack input to PlayerWeapon; weapon classes manage their own animations
        if (canAttack == true)
        {
            _combat.StartAttack();
        }
        else
        {
            _combat.ReleaseAttack();
        }
    }

    protected override void OnSkill(bool canUseSkill)
    {
        ChangeAnimationState(Settings.PLAYER_SKILL);
    }


    
    private void OnEnable()
    {
        // Đăng ký nhận lệnh từ Input Reader
        inputReader.MoveEvent += OnMove;
        inputReader.AttackEvent += OnAttack;
        inputReader.SkillEvent += OnSkill;
    }

    private void OnDisable()
    {
        inputReader.MoveEvent -= OnMove;
        inputReader.AttackEvent -= OnAttack;
        inputReader.SkillEvent -= OnSkill;
    }
}
