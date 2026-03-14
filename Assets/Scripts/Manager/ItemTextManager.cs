
using UnityEngine;

public class ItemTextManager : Persistence<ItemTextManager>
{

    [SerializeField] private ItemText itemTextPrefab;

    protected override void Awake()
    {
        base.Awake();
    }

    public ItemText ShowName(string name, Color color, Vector3 pos)
    {
        ItemText text = Instantiate(itemTextPrefab);
        text.transform.position = pos;
        text.SetText(name, color);
        return text;
    }
}

