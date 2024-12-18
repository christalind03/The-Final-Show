using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Inventory Item/Melee Weapon")]
public class MeleeWeapon : Weapon
{
    // This will be displayed underneath the "Weapon Parameters" header (inherited from "Weapon").
    [SerializeField] private float _attackAngle;
    [SerializeField] private float _attackRange;

    public float AttackAngle => _attackAngle;
    public float AttackRange => _attackRange;

    public override void Attack(GameObject playerObject)
    {
        Transform playerTransform = playerObject.transform;
        Collider[] hitTargets = Physics.OverlapSphere(playerTransform.position, _attackRange, _targetLayers);

        foreach (Collider hitCollider in hitTargets)
        {
            Vector3 targetDirection = (hitCollider.transform.position - playerTransform.position).normalized;
            float targetAngle = Vector3.Dot(playerTransform.forward, targetDirection); // Although we could use Angle(), Dot() is computationally cheaper
            bool withinRange = targetAngle > Mathf.Cos(_attackAngle * Mathf.Deg2Rad / 2);

            if (hitCollider.TryGetComponent(out Health healthComponent) && withinRange)
            {
                healthComponent.CmdTakeDamage(_attackDamage);
            }
        }
    }

    // NOTE: This should deal 1.5x the damage of a normal attack
    public override void AlternateAttack(GameObject playerObject) { }
}
