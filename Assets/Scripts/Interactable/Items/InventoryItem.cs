using Mirror;
using UnityEngine;

public class InventoryItem : ScriptableObject
{
    [Header("Item Data")]
    [SerializeField] protected GameObject _objectPrefab;

    public GameObject ObjectPrefab => _objectPrefab;
}

public static class InventoryItemSerializer
{
    public static void WriteInventoryItem(this NetworkWriter networkWriter, InventoryItem inventoryItem)
    {
        networkWriter.WriteString(inventoryItem.name);
    }

    public static InventoryItem ReadInventoryItem(this NetworkReader networkReader)
    {
        return Resources.Load<InventoryItem>($"Items/{networkReader.ReadString()}");
    }
}
