using Mirror;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Item", menuName = "Inventory Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Inventory Item Data")]
    [SerializeField] private GameObject _objectPrefab;

    public GameObject ObjectPrefab { get { return _objectPrefab; } }
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
