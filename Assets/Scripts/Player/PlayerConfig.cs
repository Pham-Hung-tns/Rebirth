using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerConfig", menuName = "Scriptable Objects/Player/Player Config")]
public class PlayerConfig : ScriptableObject
{
    [Header("Information")]
    public int Level;
    public string Name;
    public Sprite Icon;
    public string Story;

    [Space(5)]
    [Header("Movement Stats")]
    public float speed;
    public float acceleration = 50f, deceleration = 50f;

    [Space(5)]
    [Header("Health Stats")]
    public float currentHealth;
    public float MaxHealth;

    [Space(5)]
    [Header("Armor Stats")]
    public float currentArmor;
    public float MaxArmor;
    public float timeCooldownArmor;
    public float timeRecoverArmor = 2f;

    [Space(5)]
    [Header("Energy Stats")]
    public float currentEnergy;
    public float MaxEnergy;

    [Space(5)]
    [Header("Attack Stats")]    
    public float CriticalChance;
    public float CriticalDamage;
    public float rangeDetect;


    [Space(5)]
    [Header("Extra")]
    public bool unlock;
    public int unlockCost;
    public int upgradeCost;
    public int upgradeCostPercent;

    [Space(5)]
    [Header("Prefab")]
    public GameObject playerPrefab;
    public Weapon initialWeapon;

    [Space(5)]
    [Header("Skill Tree")]
    public TechTree skillTree;

    public List<TechNode> GetSkillNodes()
    {
        if (skillTree == null || skillTree.tree == null) return new List<TechNode>();
        return skillTree.tree;
    }
}
