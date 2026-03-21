using UnityEngine;


public class DamageManager : Persistence<DamageManager>
{
    [SerializeField] private ShowDamageText dmgTextPrefab;

    protected override void Awake()
    {
        base.Awake();
    }
    public void ShowDmg(int damage, Transform entityPos)
    {
        Vector3 newPos = Vector3.right * (UnityEngine.Random.Range(-0.5f, 0.7f)) + entityPos.position;
        
        // Dùng ObjPoolManager thay vì Instantiate, và gắn nó làm con của DamageManager thay vì con của EntityPos
        GameObject obj = ObjPoolManager.Instance.GetFromPool(dmgTextPrefab.gameObject, newPos, Quaternion.identity, transform);
        obj.SetActive(true);

        ShowDamageText instance = obj.GetComponent<ShowDamageText>();
        if (instance != null)
        {
            instance.SetDamageText(damage);
        }
    }
}

