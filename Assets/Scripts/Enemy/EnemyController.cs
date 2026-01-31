using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : CharacterController
{
    [SerializeField] private EnemyVitality enemyVitality;
    [SerializeField] private EnemyWeapon enemyWeapon;
    [SerializeField] private EnemySkill enemySkill;
    [SerializeField] private EnemyMeleeAttack enemyMeleeAttack;
    [SerializeField] private EnemyConfig enemyConfig;
    [SerializeField] private EnemyMovement enemyMovement;

    // List of states configured in the Inspector
    [SerializeField] private List<AIState> states = new List<AIState>();

    public AIState currentState;
    private Transform player;
    
    private string currentAnim;
    private float currentTime = 0f;
    private float timeLimit = 0f;
    private Vector3 patrolPosition = Vector3.zero;
    private bool isAttack = false;

    
    public EnemyMovement EnemyMovement { get => enemyMovement; set => enemyMovement = value; }
    public EnemyWeapon EnemyWeapon { get => enemyWeapon; set => enemyWeapon = value; }
    public EnemySkill EnemySkill { get => enemySkill; set => enemySkill = value; }
    public EnemyMeleeAttack EnemyMeleeAttack { get => enemyMeleeAttack; set => enemyMeleeAttack = value; }
    public EnemyConfig EnemyConfig { get => enemyConfig; set => enemyConfig = value; }
    public Vector3 PatrolPosition { get => patrolPosition; set => patrolPosition = value; }
    public float CurrentTime { get => currentTime; set => currentTime = value; }
    public float TimeLimit { get => timeLimit; set => timeLimit = value; }
    public Transform Player { get => player; set => player = value; }
    public bool IsAttack { get => isAttack; set => isAttack = value; }
    
    // Property để access attack system hiện tại
    public IAttackable CurrentAttackSystem
    {
        get
        {
            switch (enemyConfig.attackType)
            {
                case EnemyConfig.AttackType.Skill:
                    return enemySkill as IAttackable;
                case EnemyConfig.AttackType.MeleeAttack:
                    return enemyMeleeAttack as IAttackable;
                case EnemyConfig.AttackType.Weapon:
                default:
                    return enemyWeapon as IAttackable;
            }
        }
    }

    private void Awake()
    {
        enemyMovement.Initialized(this);
        enemyVitality.Initialized(enemyConfig);
    
        // Chỉ khởi tạo component cần thiết dựa vào attackType
        switch (enemyConfig.attackType)
        {
            case EnemyConfig.AttackType.Weapon:
                if (enemyWeapon != null)
                    enemyWeapon.Initialized(this, enemyConfig);
                break;
            case EnemyConfig.AttackType.Skill:
                if (enemySkill != null)
                    enemySkill.Initialized(this, enemyConfig);
                break;
            case EnemyConfig.AttackType.MeleeAttack:
                if (enemyMeleeAttack != null)
                    enemyMeleeAttack.Initialized(this, enemyConfig);
                break;
        }
    }
    private void Start()
    {
        // Initialize all states assigned via the Inspector
        if (states != null)
        {
            for (int i = 0; i < states.Count; i++)
            {
                var s = states[i];
                if (s != null)
                    s.Initialize(this);
            }
        }

        // Ensure current state is initialized even if not in the list
        if (currentState != null)
            currentState.Initialize(this);

        // Ensure initial state's enter lifecycle is invoked if a state was set in the inspector
        if (currentState != null)
        {
            currentState.EnterState();
            CurrentTime = 0f;
        }
    }

    private void Update()
    {
        currentState?.UpdateState();
        CurrentTime += Time.deltaTime;
    }

    public void ChangeToState(AIState nextState)
    {
        // Nếu còn đang tấn công, không cho phép chuyển state
        if (IsAttack)
            return;

        var previous = currentState;
        if (nextState == previous)
            return;

        // Exit previous if any
        previous?.ExitState();

        // If no next provided, stay in previous state (remain)
        if (nextState == null)
        {
            currentState = previous;
            return;
        }

        // Assign and enter new state
        currentState = nextState;
        CurrentTime = 0f; // reset state timer
        currentState.EnterState();
        Debug.Log("Current State: " + currentState);
    }

    // Move enemy to new direction, flip sprite and rotate weapon
    public void ChangeDirection(Vector3 newPosition)
    {
        Vector3 dir = newPosition - rigidBody2D.transform.position;
        if (dir.x < 0)
        {
            Spr.flipX = true;
        }
        else
        {
            Spr.flipX = false;
        }
        if (enemyConfig.initialWeapon != null)
            EnemyWeapon.RotateWeaponToPlayer(dir);
    }


    protected override void OnAttack(bool IsAttack)
    {
        base.OnAttack(IsAttack);
    }

    protected override void OnSkill(bool canUseSkill)
    {
        base.OnSkill(canUseSkill);
    }

    override protected void OnMove(Vector2 input)
    {
        base.OnMove(input);
    }

    // Callback từ EnemyWeapon khi tấn công hoàn thành
    public void OnAttackComplete()
    {
        IsAttack = false;
    }

    // Gizmos
    public void OnDrawGizmos()
    {
        //Detect Player
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, enemyConfig.rangeCanDetectPlayer);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyConfig.rangeCanLookAtPlayer);

        // Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position + Vector3.right * (Spr.flipX ? -1 : 1),
            enemyConfig.attackRange
        );

    }
}
