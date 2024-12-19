using Mirror;
using System.Collections;
using UnityEngine;

public class CombatController : NetworkBehaviour
{
    [Header("Combat References")]
    [SerializeField] private Transform _cameraTransform;

    private bool _canAttack;

    private void Awake()
    {
        _canAttack = true;        
    }

    // TODO: Document
    public void Attack(Weapon playerWeapon)
    {
        if (_canAttack)
        {
            Debug.Log("Attacking!");

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

            StartCoroutine(TriggerCooldown(playerWeapon.AttackCooldown));
        }
    }

    // TODO: Document
    private void MeleeAttack(MeleeWeapon playerWeapon)
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, playerWeapon.AttackRange, playerWeapon.AttackLayers);

        foreach (Collider hitCollider in hitTargets)
        {
            Vector3 targetDirection = (hitCollider.transform.position - transform.position).normalized;
            float targetAngle = Vector3.Dot(transform.forward, targetDirection); // Although we could use Angle(), Dot() is computationally cheaper
            float attackRange = Mathf.Cos(playerWeapon.AttackAngle * Mathf.Rad2Deg / 2);
            bool inRange = attackRange < targetAngle;

            if (inRange && hitCollider.TryGetComponent(out Health healthComponent))
            {
                healthComponent.CmdTakeDamage(playerWeapon.AttackDamage);
            }
        }
    }

    // TODO: Document
    private void Shoot(RangedWeapon rangedWeapon)
    {
        Debug.Log("Attempting to shoot...");

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
                Debug.Log($"{rangedWeapon.name} ran out of ammunition!");
            }
        }
    }

    // TODO: Document
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

    // TODO: Documentation
    [ClientRpc]
    private void RpcShoot(RangedWeapon rangedWeapon, GameObject projectileObject, Vector3 initialPosition, Vector3 finalPosition)
    {
        Projectile projectileComponent = projectileObject.GetComponent<Projectile>();
        projectileComponent.AttackDamage = rangedWeapon.AttackDamage;
        projectileComponent.AttackLayers = rangedWeapon.AttackLayers;
        projectileComponent.transform.LookAt(finalPosition);

        Vector3 targetDirection = (finalPosition - initialPosition).normalized;

        Rigidbody projectileRigidbody = projectileObject.GetComponent<Rigidbody>();
        projectileRigidbody.velocity = targetDirection * rangedWeapon.ProjectileSpeed;
    }

    // TODO: Documentation
    private void Reload(RangedWeapon rangedWeapon)
    {
        rangedWeapon.AmmoCount = rangedWeapon.ClipCapacity;
    }

    // TODO: Document
    private IEnumerator TriggerCooldown(float attackCooldown)
    {
        _canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }
}
