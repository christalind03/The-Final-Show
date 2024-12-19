using Mirror;
using UnityEngine;

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
