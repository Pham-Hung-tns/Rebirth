using UnityEngine;

[CreateAssetMenu]
public class EnemyConfig : ScriptableObject
{
    public enum AttackType
    {
        Weapon,
        Skill,
        MeleeAttack
    }

    [Header("Vitality")]
    public int Health;

    [Header("Detection")]
    public LayerMask obstacleLayer;
    public LayerMask playerLayer;

    public float rangeCanDetectPlayer;
    public float rangeCanLookAtPlayer;

    [Header("Melee Attack")]
    [Tooltip("Khoảng cách tối thiểu để bắt đầu tấn công đối với melee attack")]
    public float attackRange = 1.5f; // Bán kính vùng đánh
    [Tooltip("Sát thương cho meelee attack")]
    public float damageAmount = 10f; // Sát thương

    [Header("Idle State")]
    public float timeToWander;

    [Header("Wander State")]
    public float timeToIdle;
    public float speed;
    public Vector3 moveRange;

    [Header("Chase State")]
    public float timeToAttack;

    [Header("Attack State")]
    public AttackType attackType = AttackType.Weapon; // Loại tấn công: Weapon hay Skill
    public float attackDelay = 0.5f; // Độ trễ từ lúc theo dõi hướng player đến lúc tấn công

    public Vector2 knockbackDir = Vector2.zero;
    public float knockbackForce = 5f; // Lực đẩy

    [Header("Weapon")]
    public Weapon initialWeapon;
    
    [Header("Skill")]
    public EnemySkill initialSkill;
}



