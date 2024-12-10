using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract base class for melee weapon types in the game
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    public string WeaponName;
    public float Damage;
    public string DamageType;
    public float Cooldown;
    public bool IsEquipped;
    /// <summary>
    /// Equips the weapon
    /// </summary>
    public virtual void Equip()
    {
        IsEquipped = true;
        Debug.Log($"{WeaponName} equipped.");
    }
    /// <summary>
    /// Unequips the weapon
    /// </summary>
    public virtual void Unequip()
    {
        IsEquipped = false;
        Debug.Log($"{WeaponName} unequipped.");
    }
    /// <summary>
    /// Executes the primary attack for the weapon
    /// </summary>
    public abstract void Attack();

    /// <summary>
    ///  Executes the alternate (secondary) attack action for the weapon
    /// </summary>
    public abstract void AlternateAttack();
}
