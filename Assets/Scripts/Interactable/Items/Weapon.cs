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
