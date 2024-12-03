using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Weapon
{
    public int AmmoCapacity;
    public int CurrentAmmo;
    public float ReloadTime;
    public float ProjectileSpeed;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject arrowPrefab;  // Reference to the arrow prefab
    [SerializeField] private Transform firePoint;      // Point where the arrow is instantiated

    private void Start()
    {
        CurrentAmmo = AmmoCapacity;
    }

    public override void Attack()
    {
        if (IsEquipped && CurrentAmmo > 0)
        {
            CurrentAmmo--;
            Debug.Log($"{WeaponName} fires an arrow!");
            ShootArrow();  // Call the method to shoot an arrow
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

    private void ShootArrow()
    {
        if (arrowPrefab != null && firePoint != null)
        {
            // Instantiate the arrow at the fire point position and rotation
            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);

            // Get the Rigidbody component from the arrow to add force
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * ProjectileSpeed, ForceMode.Impulse);
            }
            else
            {
                Debug.LogError("Arrow prefab is missing a Rigidbody component.");
            }
        }
        else
        {
            Debug.LogError("Arrow prefab or fire point is not set.");
        }
    }

    public void Reload()
    {
        Debug.Log($"{WeaponName} is reloading...");
        CurrentAmmo = AmmoCapacity;
    }
}

