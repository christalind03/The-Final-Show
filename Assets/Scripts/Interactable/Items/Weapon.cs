using Mirror;
using UnityEngine;

/// <summary>
/// Represents a weapon that can be equipped by a player or entity.
/// </summary>
public class Weapon : InventoryItem
{
    [Header("Weapon Parameters")]
    [SerializeField] protected LayerMask _attackLayers;
    [SerializeField] protected float _attackCooldown;
    [SerializeField] protected float _attackDamage;

    public LayerMask AttackLayers => _attackLayers;
    public float AttackCooldown => _attackCooldown;
    public float AttackDamage => _attackDamage;
}

/// <summary>
/// Provides serialization and deserialization methods for <see cref="Weapon"/> objects over a network.
/// </summary>
public static class WeaponSerializer
{
    public static void WriteWeapon(this NetworkWriter networkWriter, Weapon weapon)
    {
        networkWriter.WriteString(weapon.name);
    }

    public static Weapon ReadWeapon(this NetworkReader networkReader)
    {
        return Resources.Load<Weapon>($"Items/{networkReader.ReadString()}");
    }
}