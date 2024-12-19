using Mirror;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Weapon", menuName = "Inventory Item/Ranged Weapon")]
public class RangedWeapon : Weapon
{
    // This will be displayed underneath the `Weapon Parameters` header (inherited from `Weapon`)
    public int AmmoCount;
    public int ClipCount;
    [SerializeField] private int _clipCapacity;
    [SerializeField] private float _reloadTime;

    [Header("Projectile Parameters")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Vector3 _projectileOffset;
    [SerializeField] private float _projectileSpeed;

    public int ClipCapacity => _clipCapacity;
    public float ReloadTime => _reloadTime;

    public GameObject ProjectilePrefab => _projectilePrefab;
    public Vector3 ProjectileOffset => _projectileOffset;
    public float ProjectileSpeed => _projectileSpeed;
}

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
