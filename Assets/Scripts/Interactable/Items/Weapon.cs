using UnityEngine;

public abstract class Weapon : EquippableItem
{
    [Header("Weapon Parameters")]
    [SerializeField] protected LayerMask _targetLayers;
    [SerializeField] protected float _attackCooldown;
    [SerializeField] protected float _attackDamage;

    public float AttackCooldown => _attackCooldown;
    public float AttackDamage => _attackDamage;

    public abstract void Attack(GameObject playerObject);
    public abstract void AlternateAttack(GameObject playerObject);
}
