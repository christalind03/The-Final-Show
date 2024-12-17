using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Inventory Item/Armor")]
public class Armor : InventoryItem
{
   public enum ArmorCategory
    {
        Head,
        Chest,
        Legs,
        Feet
    }

    [Header("Armor Details")]
    [SerializeField] private ArmorCategory _armorCategory;
    [SerializeField] private float _attack;
    [SerializeField] private float _defense;
    [SerializeField] private float _healthPoints;
    [SerializeField] private float _stamina;

    public float Attack => _attack;
    public float Defense => _defense;
    public float HealthPoints => _healthPoints;
    public float Stamina => _stamina;
}
