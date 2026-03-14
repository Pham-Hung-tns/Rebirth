
using UnityEngine;

[CreateAssetMenu(fileName = "New Chest Item", menuName = "Scriptable Objects/Items/Chest Item")]
public class ChestItem : ScriptableObject
{
    [System.Serializable]
    public class ChestItemRatio
    {
        [Tooltip("Prefab item có thể rơi ra từ chest")]
        public PickableItem item;

        [Tooltip("Tỉ lệ xuất hiện (weight) — số càng cao càng dễ xuất hiện")]
        public int ratio = 1;
    }

    [Tooltip("Danh sách item với tỉ lệ xuất hiện trong chest ở level này")]
    public ChestItemRatio[] availableItems;

    /// <summary>
    /// Random item dựa trên weighted ratio
    /// </summary>
    public PickableItem GetRandomItem()
    {
        if (availableItems == null || availableItems.Length == 0)
            return null;

        int totalWeight = 0;
        foreach (var entry in availableItems)
            totalWeight += Mathf.Max(0, entry.ratio);

        if (totalWeight <= 0)
            return null;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in availableItems)
        {
            cumulative += Mathf.Max(0, entry.ratio);
            if (roll < cumulative)
                return entry.item;
        }

        return availableItems[0].item;
    }
}

