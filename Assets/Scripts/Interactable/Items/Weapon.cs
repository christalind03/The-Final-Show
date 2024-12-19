using Mirror;
using UnityEngine;

public class Weapon : EquippableItem
{
    [Header("Weapon Parameters")]
    [SerializeField] protected LayerMask _attackLayers;
    [SerializeField] protected float _attackCooldown;
    [SerializeField] protected float _attackDamage;

    public LayerMask AttackLayers => _attackLayers;
    public float AttackCooldown => _attackCooldown;
    public float AttackDamage => _attackDamage;
}

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