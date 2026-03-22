using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowDamageText : MonoBehaviour, IPoolable
{
    [SerializeField] private TextMeshProUGUI dmgText;
    [SerializeField] private float duration = 1.2f;

    private int currentDamage;
    private float animTimer = 0f;

    public void OnPoolSpawn()
    {
        // Chuẩn bị khi tái sử dụng từ Pool
    }

    public void OnPoolDespawn()
    {
        // Ngắt hết mọi Coroutine chạy dở dang nếu object bị gom về pool đột ngột
        StopAllCoroutines();
    }

    public void SetDamageText(int value)
    {
        currentDamage = value;
        dmgText.text = currentDamage.ToString();
        StopAllCoroutines(); // Đảm bảo không đè Coroutine kịch bản cũ
        StartCoroutine(AnimateText());
    }

    public void AddDamage(int additionalValue)
    {
        currentDamage += additionalValue;
        dmgText.text = currentDamage.ToString();

        // Combo-hit: Đẩy lùi thời gian animation lại một chút (max 0.3s) 
        // để số giật nảy to lên lại và lâu biến mất hơn
        animTimer = Mathf.Max(0f, animTimer - 0.3f);
    }

    private IEnumerator AnimateText()
    {
        animTimer = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3.up * 1.5f); // Bay lên 1.5 units
        
        Color startColor = dmgText.color;
        startColor.a = 1f; // Chắn chắn đục hoàn toàn
        dmgText.color = startColor;
        
        transform.localScale = Vector3.zero;

        while (animTimer < duration)
        {
            animTimer += Time.deltaTime;
            float ratio = animTimer / duration;

            // 1. Bay chầm chậm lên trên
            transform.position = Vector3.Lerp(startPos, endPos, ratio);

            // 2. Hiệu ứng Scale: Phóng to nảy lên (0 -> 1.5) rồi thu gỏ dần (1.5 -> 1)
            if (ratio < 0.2f)
            {
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.5f, ratio / 0.2f);
            }
            else
            {
                transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, (ratio - 0.2f) / 0.8f);
            }

            // 3. Mờ dần trong nửa thời gian sau cuối bản animation
            if (ratio > 0.5f)
            {
                startColor.a = Mathf.Lerp(1f, 0f, (ratio - 0.5f) / 0.5f);
                dmgText.color = startColor;
            }
            else
            {
                startColor.a = 1f; // Chắc chắn đục nếu ratio bị lùi về < 0.5 do AddDamage
                dmgText.color = startColor;
            }

            yield return null;
        }

        // Tự biến mất khi animation kết thúc: Gọi Pool manager gom về
        ObjPoolManager.Instance.ReturnToPool(gameObject);
    }
}
