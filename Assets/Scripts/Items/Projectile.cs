//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Mirror;

///// <summary>
///// Represents a projectile that deals damage when collinding with a target.
///// </summary>
//public class Projectile : NetworkBehaviour
//{
//    [SyncVar]
//    public float ProjectileDamage; // Damage dealt

//    /// <summary>
//    /// Called when the projectile collides with another object.
//    /// Checks for a <see cref="Health"/> component and applies damage if found.
//    /// </summary>
//    /// <param name="collision">The collision data </param>
//    private void OnCollisionEnter(Collision collision)
//    {
//        if (isServer)
//        {
//            // Check if the collided object has a Health component
//            if (collision.gameObject.TryGetComponent(out Health health))
//            {
//                // Apply damage
//                health.TakeDamage(ProjectileDamage);
//            }
//        }
//        // Notify all clients to destroy the projectile
//        RpcDestroyProjectile();
//    }

//    /// <summary>
//    /// Destroys the projectile across all clients.
//    /// </summary>
//    [ClientRpc]
//    private void RpcDestroyProjectile()
//    {
//        Destroy(gameObject);
//    }
//}