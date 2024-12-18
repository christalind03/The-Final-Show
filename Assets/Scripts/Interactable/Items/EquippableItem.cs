using Mirror;
using UnityEngine;

public class EquippableItem : InventoryItem
{
    public enum EquippableCategory
    {
        Head,
        Chest,
        Legs,
    }

    [Header("Transform Adjustments")]
    [SerializeField] private EquippableCategory _category;
    [SerializeField] private Vector3 _positionOffset;

    public EquippableCategory Category => _category;
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
