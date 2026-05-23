using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int maxStackSize;
    public GameObject itemPrefab;
    public GameObject handItemPrefab;
}
