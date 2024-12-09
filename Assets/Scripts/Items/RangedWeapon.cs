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
            GameObject projectileInstance = Instantiate(ProjectilePrefab, firePoint.position, firePoint.rotation);
            Debug.Log($"Projectile instantiated at {firePoint.position} with rotation {firePoint.rotation}");
            Debug.DrawRay(firePoint.position, firePoint.forward * 0.5f, Color.green, 2f);

            // Add force to projectile to shoot it forward
            Rigidbody rb = projectileInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * shootingForce, ForceMode.Impulse);
          
            }
            else
            {
                Debug.LogError("Projectile prefab is missing a Rigidbody component.");
            }

            // Assign the projectile's damage and target tag
            if (projectileInstance.TryGetComponent(out Projectile projectileScript))
            {
                projectileScript.Damage = Damage;         // Assign damage
                projectileScript.TargetTag = "Enemy";    // Assign target tag
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

