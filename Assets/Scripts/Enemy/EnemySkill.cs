using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkill : MonoBehaviour, IAttackable
{
    private EnemyController enemyController;
    [SerializeField] private EnemySkill skillPrefab;
    
    public event System.Action OnAttackComplete;

    public void Initialized(EnemyController controller, EnemyConfig enemyConfig)
    {
        enemyController = controller;
        if (enemyConfig?.initialSkill != null)
        {
            skillPrefab = enemyConfig.initialSkill;
        }
    }

    public void StartAttack()
    {
        if (skillPrefab == null) return;
        
        // Instantiate và execute skill
        EnemySkill skillInstance = Instantiate(skillPrefab, transform.position, Quaternion.identity);
        skillInstance.ExecuteSkill();
        
        // Notify complete (có thể customize để đợi animation)
        StartCoroutine(WaitForSkillComplete(skillPrefab.skillDuration));
    }

    public void ReleaseAttack()
    {
        // Nếu skill có mechanics charge thì implement ở đây
    }

    private IEnumerator WaitForSkillComplete(float duration)
    {
        yield return new WaitForSeconds(duration);
        NotifySkillComplete();
    }

    private void NotifySkillComplete()
    {
        if (enemyController != null)
        {
            enemyController.OnAttackComplete();
        }
        OnAttackComplete?.Invoke();
    }

    public void ExecuteSkill()
    {
        // Logic tấn công của skill
        // Ví dụ: spawn projectile, vẽ AoE damage, v.v.
    }

    [SerializeField] public float skillDuration = 1f; // Thời lượng skill (animation duration)
}
