using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Represents a ranged weapon that fires projectiles.
/// </summary>
public class RangedWeapon : Weapon
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject ProjectilePrefab;  // Reference to the Projectile prefab
    [SerializeField] private Transform firePoint;      // Point where the Projectile is instantiated
    public float shootingForce = 20f;                  // Force applied to the projectile when fired

    public int AmmoCapacity;
    public int CurrentAmmo;
    public float ReloadTime;      // In seconds
    public float ProjectileSpeed;


    private void Start()
    {
        if (isServer)
        {
            CurrentAmmo = AmmoCapacity;
        }
    }

    /// <summary>
    /// Executes the primary attack by firing a projectile.
    /// </summary>
    public override void Attack()
    {
        if (IsEquipped && CurrentAmmo > 0)
        {
            CurrentAmmo--;
            CmdShootProjectile();
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

    /// <summary>
    /// Executes a secondary (alternate) attack by firing a projectile with increased damage.
    /// </summary>
    public override void AlternateAttack()
    {
        if (IsEquipped && CurrentAmmo > 0)
        {
            CurrentAmmo--;
            CmdShootAlternateProjectile();
            Debug.Log($"{WeaponName} performed an alternate attack!");
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

    /// <summary>
    /// Fires the primary projectile from the weapon.
    /// </summary>
    [Command]
    private void CmdShootProjectile()
    {
        if (ProjectilePrefab != null && firePoint != null)
        {
            // Instantiate the Projectile at the fire point position and rotation
            Vector3 spawnPosition = firePoint.position + firePoint.forward * 0.5f; // Offset by 0.5 units to avoid collision with bow
            GameObject projectileInstance = Instantiate(ProjectilePrefab, firePoint.position, firePoint.rotation);
            NetworkServer.Spawn(projectileInstance);

            // Ensure the projectile doesn't collide with the bow
            Collider bowCollider = GetComponent<Collider>();
            Collider projectileCollider = projectileInstance.GetComponent<Collider>();
            if (bowCollider != null && projectileCollider != null)
            {
                Physics.IgnoreCollision(bowCollider, projectileCollider);
            }

            // Assign the projectile's damage
            if (projectileInstance.TryGetComponent(out Projectile projectileScript))
            {
                projectileScript.ProjectileDamage = this.Damage;
            }

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
        }
        else
        {
            Debug.LogError("Projectile prefab or fire point is not set.");
        }
    }

    /// <summary>
    /// Fires an alternate projectile with increased damage.
    /// </summary>
    [Command]
    private void CmdShootAlternateProjectile()
    {
        if (ProjectilePrefab != null && firePoint != null)
        {
            // Instantiate the Projectile at the fire point position and rotation
            Vector3 spawnPosition = firePoint.position + firePoint.forward * 0.5f; // Offset to avoid self-collision
            GameObject projectileInstance = Instantiate(ProjectilePrefab, spawnPosition, firePoint.rotation);
            NetworkServer.Spawn(projectileInstance);

            // Assign alternate damage to the projectile
            if (projectileInstance.TryGetComponent(out Projectile projectileScript))
            {
                projectileScript.ProjectileDamage = this.Damage * 2; // Double the base damage
            }

            // Add force to the projectile
            Rigidbody rb = projectileInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * shootingForce, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.LogError("Projectile prefab or fire point is not set.");
        }
    }

    /// <summary>
    /// Reloads the weapon to its maximum ammo capacity.
    /// </summary>
    public void Reload()
    {
        Debug.Log($"{WeaponName} is reloading...");
        CurrentAmmo = AmmoCapacity;
    }
}