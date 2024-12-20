using Mirror;
using UnityEngine;

/// <summary>
/// Represents an inventory item that can be utilized by a player or entity.
/// </summary>
public class InventoryItem : ScriptableObject
{
    [Header("Item Data")]
    [SerializeField] protected GameObject _objectPrefab;

    public GameObject ObjectPrefab => _objectPrefab;
}

/// <summary>
/// Provides serialization and deserialization methods for <see cref="InventoryItem"/> objects over a network.
/// </summary>
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
