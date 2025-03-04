using Mirror;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the player's combat actions including melee attacks, ranged shooting, and weapon cooldown management.
/// </summary>
public class CombatController : NetworkBehaviour
{
    [Header("Combat References")]
    [SerializeField] private Transform _cameraTransform;

    private AudioManager _audioManager;
    private PlayerInterface _playerInterface;
    private PlayerStats _playerStats;
    private bool _canAttack;

    // TODO: Document
    private void Awake()
    {
        _audioManager = gameObject.GetComponent<AudioManager>();
    }

    /// <summary>
    /// Initializes the combat controller, enabling attacks.
    /// </summary>
    public override void OnStartAuthority()
    {
        _playerInterface = gameObject.GetComponent<PlayerInterface>();
        _playerStats = gameObject.GetComponent<PlayerStats>();
        _canAttack = true;

        base.OnStartAuthority();
    }

    /// <summary>
    /// Executes an attack using the specified weapon, depending on its type.
    /// </summary>
    /// <param name="playerWeapon">The current weapon being used for the attack.</param>
    public void Attack(Weapon playerWeapon)
    {
        if (_canAttack)
        {
            switch (playerWeapon)
            {
                case MeleeWeapon meleeWeapon:
                    MeleeAttack(meleeWeapon);
                    break;

                case RangedWeapon rangedWeapon:
                    Shoot(rangedWeapon);
                    break;

                default:
                    Debug.LogWarning($"{playerWeapon} is not yet supported by the combat controller.");
                    break;
            }

            _audioManager.CmdPlay("Weapon");
            StartCoroutine(TriggerCooldown(playerWeapon.AttackCooldown));
        }
    }

    /// <summary>
    /// Performs a melee attack using the specified melee weapon, damaging all valid targets within range.
    /// </summary>
    /// <param name="playerWeapon">The melee weapon being used for the attack.</param>
    private void MeleeAttack(MeleeWeapon playerWeapon)
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, playerWeapon.AttackRange, playerWeapon.AttackLayers);

        foreach (Collider hitCollider in hitTargets)
        {
            Vector3 targetDirection = (hitCollider.transform.position - transform.position).normalized;
            float targetAngle = Vector3.Dot(transform.forward, targetDirection); // Although we could use Angle(), Dot() is computationally cheaper
            float attackRange = Mathf.Cos(playerWeapon.AttackAngle * Mathf.Rad2Deg / 2);
            bool inRange = attackRange < targetAngle;

            if (inRange && hitCollider.TryGetComponent(out AbstractHealth healthComponent))
            {
                float finalDamage = playerWeapon.AttackDamage;
                float critChance = playerWeapon.CriticalStrikeChance;

                if (Random.value < critChance)
                {
                    finalDamage *= 2;
                    Debug.Log("Critical Strike! Damage: " + finalDamage);
                }
                else
                {
                    Debug.Log("Normal Attack. Damage: " + finalDamage);
                }

                healthComponent.CmdDamage(finalDamage);
            }
        }
    }

    /// <summary>
    /// Fires a ranged weapon, consuming ammunition and spawning projectiles if available.
    /// Automatically reloads the weapon if the clip is empty.
    /// </summary>
    /// <param name="rangedWeapon">The ranged weapon used for shooting.</param>
    private void Shoot(RangedWeapon rangedWeapon)
    {
        if (0 < rangedWeapon.AmmoCount)
        {
            rangedWeapon.AmmoCount--;
            CmdShoot(rangedWeapon);
        }
        else
        {
            if (0 < rangedWeapon.ClipCount)
            {
                rangedWeapon.ClipCount--;
                Reload(rangedWeapon);
            }
            else
            {
                // TODO: Play some sort of sound indicator notifying the user that their ammo is out.
                Debug.Log($"{rangedWeapon.name} ran out of ammunition!");
            }
        }

        // TODO: If the user reloads before the current clip is gone, ensure that the difference between the clip capacity and current amount is added to the remaining amount
        _playerInterface.RefreshAmmo(rangedWeapon.AmmoCount, rangedWeapon.ClipCapacity * rangedWeapon.ClipCount);
    }

    /// <summary>
    /// Spawns and launches a projectile on the server for a ranged weapon attack.
    /// </summary>
    /// <param name="rangedWeapon">The ranged weapon being used for the attack.</param>
    [Command]
    private void CmdShoot(RangedWeapon rangedWeapon)
    {
        Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit raycastHit);

        Vector3 initialPosition = transform.position + rangedWeapon.ProjectileOffset;
        Vector3 finalPosition = raycastHit.point;

        GameObject projectileObject = Instantiate(rangedWeapon.ProjectilePrefab, initialPosition, Quaternion.identity);
        NetworkServer.Spawn(projectileObject);

        RpcShoot(rangedWeapon, projectileObject, initialPosition, finalPosition);
    }

    /// <summary>
    /// Updates all the clients with the projectile spawn and trajectory information for a ranged weapon attack.
    /// </summary>
    /// <param name="rangedWeapon">The ranged weapon being used for the attack.</param>
    /// <param name="projectileObject">The spawned projectile GameObject.</param>
    /// <param name="initialPosition">The starting position of the projectile.</param>
    /// <param name="finalPosition">The target position of the projectile.</param>
    [ClientRpc]
    private void RpcShoot(RangedWeapon rangedWeapon, GameObject projectileObject, Vector3 initialPosition, Vector3 finalPosition)
    {
        Projectile projectileComponent = projectileObject.GetComponent<Projectile>();
        float finalDamage = rangedWeapon.AttackDamage;
        float critChance = rangedWeapon.CriticalStrikeChance;

        if (Random.value < critChance)
        {
            finalDamage *= 2;
            Debug.Log("Critical Strike! Damage: " + finalDamage);
        }
        else
        {
            Debug.Log("Normal Attack. Damage: " + finalDamage);
        }
      
        projectileComponent.AttackDamage = finalDamage;
        projectileComponent.AttackLayers = rangedWeapon.AttackLayers;
        projectileComponent.transform.LookAt(finalPosition);

        Vector3 targetDirection = (finalPosition - initialPosition).normalized;

        Rigidbody projectileRigidbody = projectileObject.GetComponent<Rigidbody>();
        projectileRigidbody.velocity = targetDirection * rangedWeapon.ProjectileSpeed;
    }

    /// <summary>
    /// Reloads a ranged weapon by refilling its ammunition count from its clip capacity.
    /// </summary>
    /// <param name="rangedWeapon">The ranged weapon to reload.</param>
    private void Reload(RangedWeapon rangedWeapon)
    {
        rangedWeapon.AmmoCount = rangedWeapon.ClipCapacity;
    }

    /// <summary>
    /// Prevents the player from attacking for the duration of the weapon's cooldown.
    /// </summary>
    /// <param name="attackCooldown">The cooldown duration in seconds.</param>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
    private IEnumerator TriggerCooldown(float attackCooldown)
    {
        _canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }
}
