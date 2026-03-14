using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/Items")]
public class ItemDataSO: ScriptableObject
{
    [Header("Config")]
    public string Name;
    public Sprite icon;

    public virtual void PickUp(GameObject player)
    {

    }
}
