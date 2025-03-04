using Mirror;
using UnityEngine;

/// <summary>
/// Represents an inventory item that can be utilized by a player or entity.
/// </summary>
public class InventoryItem : ScriptableObject
{
    public enum InventoryCategory
    {
        Chest,
        Helmet,
        Legs,
        Weapon,
    }

    [Header("Item Data")]
    [SerializeField] protected Sprite _objectSprite;
    [SerializeField] protected GameObject _objectPrefab;
    [SerializeField] protected SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] protected InventoryCategory _inventoryCategory;

    public Sprite ObjectSprite => _objectSprite;
    public GameObject ObjectPrefab => _objectPrefab;
    public SkinnedMeshRenderer SkinnedMeshRenderer => _skinnedMeshRenderer;
    public InventoryCategory ItemCategory => _inventoryCategory;
}

/// <summary>
/// Provides serialization and deserialization methods for <see cref="InventoryItem"/> objects over a network.
/// </summary>
public static class InventoryItemSerializer
{
    private const string NullMarker = "null";

    public static void WriteInventoryItem(this NetworkWriter networkWriter, InventoryItem inventoryItem)
    {
        networkWriter.WriteString(inventoryItem != null ? inventoryItem.name : NullMarker);
    }

    public static InventoryItem ReadInventoryItem(this NetworkReader networkReader)
    {
        string inventoryItem = networkReader.ReadString();

        if (inventoryItem != NullMarker)
        {
            return Resources.Load<InventoryItem>($"Items/{inventoryItem}");
        }
        
        return null;
    }
}
