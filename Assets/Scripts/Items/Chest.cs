using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Transform itemPos;

    [Header("Item")]
    [SerializeField] private bool usePredefinedItemFromChest; // if true, use predefined item
    [SerializeField] private GameObject predefinedItem; // predefined item

    private ChestItem chestItemData;
    private Animator animator;
    private bool openChest;

    /// <summary>
    /// Thiết lập ChestItem data cho chest này (gọi ngay sau Instantiate)
    /// </summary>
    public void SetChestItemData(ChestItem data)
    {
        chestItemData = data;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (openChest) return;
        if (collision.CompareTag(Settings.playerTag))
        {
            //TODO: Add sound effect for opening chest
            //AudioManager.Instance.PlaySFX("Chest_Open");
            ShowItem();
            animator.SetTrigger("Open");
        }
    }

    private void ShowItem()
    {
        GameObject itemPrefab = null;

        if (usePredefinedItemFromChest)
        {
            itemPrefab = predefinedItem;
        }
        else if (chestItemData != null)
        {
            PickableItem randomItem = chestItemData.GetRandomItem();
            if (randomItem != null)
                itemPrefab = randomItem.gameObject;
        }

        if (itemPrefab != null)
        {
            GameObject spawnedItem;
            if (ObjPoolManager.Instance != null)
            {
                spawnedItem = ObjPoolManager.Instance.GetFromPool(itemPrefab, transform.position, Quaternion.identity, itemPos.parent);
            }
            else
            {
                spawnedItem = Instantiate(itemPrefab, transform.position, Quaternion.identity, itemPos.parent);
            }
            spawnedItem.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Chest: Không có item để spawn!");
        }

        openChest = true;
    }
}
