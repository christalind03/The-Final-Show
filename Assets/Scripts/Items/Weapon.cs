using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string WeaponName;
    public float Damage;
    public string DamageType;
    public float Cooldown;
    public bool IsEquipped;

    public virtual void Equip()
    {
        IsEquipped = true;
        Debug.Log($"{WeaponName} equipped.");
    }

    public virtual void Unequip()
    {
        IsEquipped = false;
        Debug.Log($"{WeaponName} unequipped.");
    }

    public abstract void Attack();

    public abstract void AlternateAttack();
}
