using Mirror;
using UnityEngine;

/// <summary>
/// Represents a ranged weapon that can be equipped by a player or entity.
/// </summary>
[CreateAssetMenu(fileName = "New Ranged Weapon", menuName = "Inventory Item/Ranged Weapon")]
public class RangedWeapon : Weapon
{
    // This will be displayed underneath the `Weapon Parameters` header (inherited from `Weapon`)
    [Header("Projectile Parameters")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed;

    public GameObject ProjectilePrefab => _projectilePrefab;
    public float ProjectileSpeed => _projectileSpeed;
}

/// <summary>
/// Provides serialization and deserialization methods for <see cref="RangedWeapon"/> objects over a network.
/// </summary>
public static class RangedWeaponSerializer
{
    public static void WriteRangedWeapon(this NetworkWriter networkWriter, RangedWeapon rangedWeapon)
    {
        networkWriter.WriteString(rangedWeapon.name);
    }

    public static RangedWeapon ReadInventoryItem(this NetworkReader networkReader)
    {
        return Resources.Load<RangedWeapon>($"Items/{networkReader.ReadString()}");
    }
}
