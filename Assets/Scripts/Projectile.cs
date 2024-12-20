using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Projectile : NetworkBehaviour
{
    public float AttackDamage;
    public LayerMask AttackLayers;

    // TODO: Document
    private void OnCollisionEnter(Collision collisionEvent)
    {
        if (isServer)
        {
            GameObject collisionObject = collisionEvent.gameObject;

            if (UnityExtensions.ContainsLayer(AttackLayers, collisionObject.layer) && collisionObject.TryGetComponent(out Health healthComponent))
            {
                healthComponent.CmdDamage(AttackDamage);
            }
        }

        Destroy(gameObject);
    }
}
