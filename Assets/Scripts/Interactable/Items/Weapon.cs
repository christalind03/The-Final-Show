using Mirror;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Represents a weapon that can be equipped by a player or entity.
/// </summary>
public class Weapon : InventoryItem
{
    [Header("Weapon Parameters")]
    [SerializeField] protected AudioResource _attackAudio;
    [SerializeField] protected LayerMask _attackLayers;
    [SerializeField] protected float _attackCooldown;
    [SerializeField] protected float _attackDamage;
    [SerializeField, Range(0f, 1f)] protected float _criticalStrikeChance;

    [Header("Animation Parameters")]
    [SerializeField] protected AnimatorOverrideController _animatorController;

    public AudioResource AttackAudio => _attackAudio;
    public LayerMask AttackLayers => _attackLayers;
    public float AttackCooldown => _attackCooldown;
    public float AttackDamage => _attackDamage;
    public float CriticalStrikeChance => _criticalStrikeChance;
    public AnimatorOverrideController AnimatorController => _animatorController;
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