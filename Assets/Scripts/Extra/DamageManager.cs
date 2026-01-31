using UnityEngine;


public class DamageManager : Singleton<DamageManager>
{
    [SerializeField] private ShowDamageText dmgTextPrefab;

    protected override void Awake()
    {
        base.Awake();
    }
    public void ShowDmg(int damage, Transform entityPos)
    {
        Vector3 newPos = Vector3.right * (UnityEngine.Random.Range(-0.5f, 0.7f)) + entityPos.position;
        ShowDamageText instance = Instantiate(dmgTextPrefab, newPos, Quaternion.identity, entityPos);
        instance.SetDamageText(damage);
    }
}

