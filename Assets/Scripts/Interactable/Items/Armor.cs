using Mirror;
using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Inventory Item/Armor")]
public class Armor : EquippableItem
{
    [Header("Armor Details")]
    [SerializeField] private float _attack;
    [SerializeField] private float _defense;
    [SerializeField] private float _health;
    [SerializeField] private float _stamina;

    public float Attack => _attack;
    public float Defense => _defense;
    public float Health => _health;
    public float Stamina => _stamina;
}

public static class ArmorSerializer
{
    public static void WriteArmor(this NetworkWriter networkWriter, Armor armor)
    {
        networkWriter.WriteString(armor.name);
    }

    public static Armor ReadInventoryItem(this NetworkReader networkReader)
    {
        return Resources.Load<Armor>($"Items/{networkReader.ReadString()}");
    }
}