using System.Collections.Generic;
using UnityEngine;


public class DamageManager : Persistence<DamageManager>
{
    [SerializeField] private ShowDamageText dmgTextPrefab;
    private Dictionary<Transform, ShowDamageText> activeTexts = new Dictionary<Transform, ShowDamageText>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void ShowDmg(int damage, Transform entityPos)
    {
        // 1. Nếu trên đầu quái này ĐANG CÓ sẵn 1 số sát thương nhảy lên -> Cộng dồn
        if (activeTexts.TryGetValue(entityPos, out ShowDamageText existingText) 
            && existingText != null 
            && existingText.gameObject.activeInHierarchy)
        {
            existingText.AddDamage(damage);
        }
        else
        {
            // 2. Nếu chưa có (hoặc text cũ đã bay mất) -> Khởi tạo mới
            Vector3 newPos = Vector3.right * (UnityEngine.Random.Range(-0.5f, 0.7f)) + entityPos.position;
            
            GameObject obj = ObjPoolManager.Instance.GetFromPool(dmgTextPrefab.gameObject, newPos, Quaternion.identity, transform);
            obj.SetActive(true);

            ShowDamageText instance = obj.GetComponent<ShowDamageText>();
            if (instance != null)
            {
                instance.SetDamageText(damage);
                activeTexts[entityPos] = instance;
            }
        }
    }
}

