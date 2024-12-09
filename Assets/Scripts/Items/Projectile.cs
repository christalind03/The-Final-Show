using UnityEngine;
public class Projectile : MonoBehaviour
{
    public float Damage { get; set; } // Damage dealt
    public string TargetTag { get; set; } // Tag of valid targets
    private void Start()
    {
        Debug.Log($"Projectile spawned: {gameObject.name} at {transform.position}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collided with {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        // Check if collided object matches the target tag
        if (collision.gameObject.CompareTag(TargetTag))
        {
            // Try to get the Health component and apply damage
            if (collision.gameObject.TryGetComponent(out Health health))
            {
                health.TakeDamage(Damage);
                Debug.Log($"Projectile hit {collision.gameObject.name} for {Damage} damage.");
            }
        }

        // Destroy projectile after collision
        Destroy(gameObject);
    }

    // May or may not use function
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Projectile triggered collision with {other.gameObject.name}");

        // Check if the object has the correct tag
        if (other.gameObject.CompareTag(TargetTag))
        {
            // Check if the object has a Health component and apply damage
            if (other.gameObject.TryGetComponent(out Health health))
            {
                health.TakeDamage(Damage);
                Debug.Log($"Dealt {Damage} damage to {other.gameObject.name}");
            }
        }

        // Destroy the projectile after the collision
        Destroy(gameObject);
    }

}

