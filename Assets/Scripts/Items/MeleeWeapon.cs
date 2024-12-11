using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Represents a melee weapon type in the game.
/// Inherits from the <see cref="Weapon"/> base class.
/// </summary>
public class MeleeWeapon : Weapon
{
    public float Range;
    public float SwingSpeed;
    public LayerMask targetMask;

    /// <summary>
    /// Executes the primary attack of the weapon
    /// </summary>
    [Command]
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
    /// <summary>
    /// Executes the alternative (secondary) attack of the weapon. Deals more damage
    /// </summary>
    [Command]
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

    /// <summary>
    /// Performs the primary attack logic, applying damage to targets in range.
    /// </summary>
    [Server]
    private void PerformAttack()
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, Range, targetMask);
            // target.transform.parent.gameObject.TryGetComponent(out Health targetHealth)
        foreach (Collider target in hitTargets)
        {
            if (target.TryGetComponent(out Health targetHealth))
            {
                targetHealth.TakeDamage(Damage);
            }
        }
    }
    /// <summary>
    /// Performs the alternate attack logic, applying increased damage to targets in range.
    /// </summary>
    [Server]
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
}