using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Inventory Item/Melee Weapon")]
public class MeleeWeapon : Weapon
{
    // This will be displayed underneath the "Weapon Parameters" header (inherited from "Weapon").
    [SerializeField] private float _attackAngle;
    [SerializeField] private float _attackRange;

    public float AttackAngle => _attackAngle;
    public float AttackRange => _attackRange;
}
