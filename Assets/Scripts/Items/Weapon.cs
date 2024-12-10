using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// An abstract base class for melee weapon types in the game
/// </summary>
public abstract class Weapon : NetworkBehaviour
{
    public string WeaponName;
    public float Damage;
    public string DamageType;
    public float Cooldown;
    public bool IsEquipped;

    /// <summary>
    /// Equips the weapon
    /// </summary>
    [Command]
    public virtual void CmdEquip()
    {
        IsEquipped = true;
        RpcEquip();
        Debug.Log($"{WeaponName} equipped.");
    }

    [ClientRpc]
    private void RpcEquip()
    {
        IsEquipped = true;
        Debug.Log($"{WeaponName} equipped on all clients.");
    }

    /// <summary>
    /// Unequips the weapon
    /// </summary>
    [Command]
    public virtual void CmdUnequip()
    {
        IsEquipped = false;
        RpcUnequip();
        Debug.Log($"{WeaponName} unequipped.");
    }

    [ClientRpc]
    private void RpcUnequip()
    {
        IsEquipped = false;
        Debug.Log($"{WeaponName} unequipped on all clients.");
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