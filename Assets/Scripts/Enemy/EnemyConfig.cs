using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig_", menuName = "Scriptable Objects/Enemy/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    public enum AttackType
    {
        Weapon,
        Skill,
        MeleeAttack
    }

    [Header("Prefab")]
    [Tooltip("Prefab GameObject của enemy này (dùng cho spawner)")]
    public GameObject enemyPrefab;

    [Header("Vitality")]
    public int Health;

    [Header("Health Per Level")]
    [Tooltip("Cấu hình HP theo từng dungeon level, override field Health bên trên")]
    public EnemyHealthDetails[] healthByLevel;

    [Header("Detection")]
    public LayerMask obstacleLayer;
    public LayerMask playerLayer;

    public float rangeCanDetectPlayer;
    public float rangeCanLookAtPlayer;

    [Header("Melee Attack")]
    [Tooltip("Khoảng cách tối thiểu để bắt đầu tấn công đối với melee attack")]
    public float attackRange = 1.5f;
    [Tooltip("Sát thương cho meelee attack")]
    public float damageAmount = 10f;

    [Header("Idle State")]
    public float timeToWander;

    [Header("Wander State")]
    public float timeToIdle;
    public float speed;
    public Vector3 moveRange;

    [Header("Chase State")]
    public float timeToAttack;

    [Header("Attack State")]
    public AttackType attackType = AttackType.Weapon;
    public float attackDelay = 0.5f;

    public Vector2 knockbackDir = Vector2.zero;
    public float knockbackForce = 5f;

    [Header("Weapon")]
    public Weapon initialWeapon;
    
    [Header("Skill")]
    public CastSkillCombat initialSkill;
}

[Serializable]
public class EnemyHealthDetails
{
    public DungeonLevelSO dungeonLevel;
    public int health = 10;
}
