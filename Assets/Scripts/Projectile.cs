using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Projectile : NetworkBehaviour
{
    public float AttackDamage;
    public LayerMask AttackLayers;
    public LayerMask IgnoreLayers;

    /// <summary>
    /// Handles collision events for the object.
    /// Applies damage to any valid targets and destroys the object upon collision.
    /// </summary>
    /// <param name="collisionEvent">The collision event data provided by Unity.</param>
    private void OnCollisionEnter(Collision collisionEvent)
    {
        if (isServer)
        {
            GameObject collisionObject = collisionEvent.gameObject;

            if (UnityUtils.ContainsLayer(AttackLayers, collisionObject.layer) && collisionObject.TryGetComponent(out AbstractHealth healthComponent))
            {
                healthComponent.CmdDamage(AttackDamage);
            }

            if (!UnityUtils.ContainsLayer(IgnoreLayers, collisionObject.layer))
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}
