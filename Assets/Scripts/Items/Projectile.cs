using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a projectile that deals damage when collinding with a target.
/// </summary>
public class Projectile : MonoBehaviour
{
    public float ProjectileDamage { get; set; } // Damage dealt
  
    /// <summary>
    /// Called when the projectile collides with another object.
    /// Checks for a <see cref="Health"/> component and applies damage if found.
    /// </summary>
    /// <param name="collision">The collision data </param>
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object has a Health component
        if (collision.gameObject.TryGetComponent(out Health health))
        {
            // Apply damage
            health.TakeDamage(ProjectileDamage);
        }

        // Destroy projectile after collision
        Destroy(gameObject);
    }
}

