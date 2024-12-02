using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Weapon
{
    public int AmmoCapacity;
    public int CurrentAmmo;
    public float ReloadTime;
    public float ProjectileSpeed;

    public override void Attack()
    {
        if (IsEquipped && CurrentAmmo > 0)
        {
            CurrentAmmo--;
            Debug.Log($"{WeaponName} fires a projectile!");
            PerformAttack();
        }
        else if (CurrentAmmo <= 0)
        {
            Debug.Log($"{WeaponName} is out of ammo!");
        }
        else
        {
            Debug.Log("Weapon not equipped!");
        }
    }

    public override void AlternateAttack()
    {
        Debug.Log($"{WeaponName} fires a charged shot!");
        // Implement charged shot logic
    }

    public void Reload()
    {
        Debug.Log($"{WeaponName} is reloading...");
        CurrentAmmo = AmmoCapacity;
    }

    private void PerformAttack()
    {
        // Spawn a projectile and set its velocity
        Debug.Log("Projectile launched!");
    }
}
