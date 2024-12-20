using Mirror;
using UnityEngine;

/// <summary>
/// Represents a melee weapon that can be equipped by a player or entity.
/// </summary>
[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Inventory Item/Melee Weapon")]
public class MeleeWeapon : Weapon
{
    // This will be displayed underneath the "Weapon Parameters" header (inherited from "Weapon").
    [SerializeField] private float _attackAngle;
    [SerializeField] private float _attackRange;

    public float AttackAngle => _attackAngle;
    public float AttackRange => _attackRange;
}

/// <summary>
/// Provides serialization and deserialization methods for <see cref="MeleeWeapon"/> objects over a network.
/// </summary>
public static class MeleeWeaponSerializer
{
    public static void WriteMeleeWeapon(this NetworkWriter networkWriter, MeleeWeapon meeleeWeapon)
    {
        networkWriter.WriteString(meeleeWeapon.name);
    }

    public static MeleeWeapon ReadInventoryItem(this NetworkReader networkReader)
    {
        return Resources.Load<MeleeWeapon>($"Items/{networkReader.ReadString()}");
    }
}