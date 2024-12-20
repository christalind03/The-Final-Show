using Mirror;
using UnityEngine;

/// <summary>
/// Represents an equippable item that can be worn by a player or entity.
/// </summary>
public class EquippableItem : InventoryItem
{
    public enum EquippableCategory
    {
        Hand,
        Head,
    }

    [Header("Transform Adjustments")]
    [SerializeField] private EquippableCategory _equipmentCategory;
    [SerializeField] private Vector3 _positionOffset;

    public EquippableCategory EquipmentCategory => _equipmentCategory;
    public Vector3 PositionOffset => _positionOffset;
}

/// <summary>
/// Provides serialization and deserialization methods for <see cref="EquippableItem"/> objects over a network.
/// </summary>
public static class EquippableItemSerializer
{
    public static void WriteInventoryItem(this NetworkWriter networkWriter, EquippableItem equippableItem)
    {
        networkWriter.WriteString(equippableItem.name);
    }

    public static EquippableItem ReadInventoryItem(this NetworkReader networkReader)
    {
        return Resources.Load<EquippableItem>($"Items/{networkReader.ReadString()}");
    }
}
