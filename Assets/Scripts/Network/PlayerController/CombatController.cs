using Mirror;
using System.Collections;
using UnityEngine;

public class CombatController : NetworkBehaviour
{
    private bool _canAttack;

    private void Awake()
    {
        _canAttack = true;        
    }

    // TODO: Document
    public void Attack(Weapon playerWeapon)
    {
        if (_canAttack)
        {
            switch (playerWeapon)
            {
                case MeleeWeapon meleeWeapon:
                    MeleeAttack(meleeWeapon);
                    break;

                default:
                    Debug.LogWarning($"{playerWeapon} is not yet supported by the combat controller.");
                    break;
            }

            StartCoroutine(TriggerCooldown(playerWeapon.AttackCooldown));
        }
    }

    // TODO: Document
    private void MeleeAttack(MeleeWeapon playerWeapon)
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, playerWeapon.AttackRange, playerWeapon.AttackLayers);

        foreach (Collider hitCollider in hitTargets)
        {
            Vector3 targetDirection = (hitCollider.transform.position - transform.position).normalized;
            float targetAngle = Vector3.Dot(transform.forward, targetDirection); // Although we could use Angle(), Dot() is computationally cheaper
            float attackRange = Mathf.Cos(playerWeapon.AttackAngle * Mathf.Rad2Deg / 2);
            bool inRange = attackRange < targetAngle;

            if (inRange && hitCollider.TryGetComponent(out Health healthComponent))
            {
                healthComponent.CmdTakeDamage(playerWeapon.AttackDamage);
            }
        }
    }

    // TODO: Document
    private IEnumerator TriggerCooldown(float attackCooldown)
    {
        _canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }
}
