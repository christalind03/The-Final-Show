using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Weapon
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject ProjectilePrefab;  // Reference to the Projectile prefab
    [SerializeField] private Transform firePoint;      // Point where the Projectile is instantiated
    public float shootingForce = 20f;

    public int AmmoCapacity;
    public int CurrentAmmo;
    public float ReloadTime;
    public float ProjectileSpeed;


    private void Start()
    {
        CurrentAmmo = AmmoCapacity;
    }

    public override void Attack()
    {
        if (IsEquipped && CurrentAmmo > 0)
        {
            CurrentAmmo--;
            Debug.Log($"{WeaponName} fires an Projectile!");
            ShootProjectile();  // Call the method to shoot an Projectile
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

    private void ShootProjectile()
    {
        if (ProjectilePrefab != null && firePoint != null)
        {
            // Instantiate the Projectile at the fire point position and rotation
            GameObject Projectile = Instantiate(ProjectilePrefab, firePoint.position, firePoint.rotation);

            // Get the Rigidbody component from the Projectile to add force
            Rigidbody rb = Projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * shootingForce, ForceMode.Impulse);
          
                Debug.Log("Projectile shot with force: " + (firePoint.forward * shootingForce));
                Debug.Log("Projectile velocity after shot: " + rb.velocity);
            }
            else
            {
                Debug.LogError("Projectile prefab is missing a Rigidbody component.");
            }
        }
        else
        {
            Debug.LogError("Projectile prefab or fire point is not set.");
        }
    }

    public void Reload()
    {
        Debug.Log($"{WeaponName} is reloading...");
        CurrentAmmo = AmmoCapacity;
    }
}

