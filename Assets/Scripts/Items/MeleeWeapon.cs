using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    public float Range;
    public float SwingSpeed;

    public override void Attack()
    {
        if (IsEquipped)
        {
            Debug.Log($"{WeaponName} swings with speed {SwingSpeed} and range {Range}.");
            PerformAttack();
        }
        else
        {
            Debug.Log("Weapon not equipped!");
        }
    }

    public override void AlternateAttack()
    {
        if (IsEquipped)
        {
            Debug.Log($"{WeaponName} performs a heavy swing!");
            PerformAlternateAttack();
        }
        else
        {
            Debug.Log("Weapon not equipped!");
        }
    }

    private void PerformAttack()
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, Range);

        foreach (Collider target in hitTargets)
        {
            if (target.TryGetComponent(out Health targetHealth))
            {
                targetHealth.TakeDamage(Damage);
            }
        }
    }

    private void PerformAlternateAttack()
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, Range);

        foreach (Collider target in hitTargets)
        {
            if (target.TryGetComponent(out Health targetHealth))
            {
                targetHealth.TakeDamage(Damage * 1.5f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
