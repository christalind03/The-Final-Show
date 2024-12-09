using UnityEngine;
public class Projectile : MonoBehaviour
{
    public float ProjectileDamage { get; set; } // Damage dealt
    private void Start()
    {
    }

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

