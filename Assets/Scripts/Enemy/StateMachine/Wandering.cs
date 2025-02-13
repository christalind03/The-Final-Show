using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Base State/Enemy/Wandering")]
public class WanderingState : EnemyState
{
    private float wanderRange = 10.0f;
    private float movementSpeed = 1.0f;

    public override void EnterState()
    {
        Debug.Log("Entering Wandering State");
        StateContext.NavMeshAgent.ResetPath(); // Clear the current path
        StateContext.NavMeshAgent.stoppingDistance = 0; // Allows the enemy to reach wander points
        StateContext.NavMeshAgent.speed = movementSpeed; // Update speed
    }

    public override void ExitState()
    {
        Debug.Log("Leaving Wandering State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }

    public override void UpdateState()
    {
        // This code is referenced from the documentation for NavMesh.SamplePosition: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AI.NavMesh.SamplePosition.html
        // If the enemy is done with the path
        if (StateContext.NavMeshAgent.remainingDistance <= StateContext.NavMeshAgent.stoppingDistance)
        {
            // Chose a random nearby point on the NavMesh to navigate to
            Vector3 point;
            if (RandomPoint(StateContext.Transform.position, wanderRange, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
                StateContext.NavMeshAgent.SetDestination(point);
            }
        }
        // Update animator speed
        StateContext.Animator.SetFloat("Speed", StateContext.NavMeshAgent.velocity.magnitude);
    }

    /// <summary>
    /// Generates a random point within a given range of a given center and tries to find a nearby point on the Navmesh.
    /// </summary>
    /// <param name="center">The center point around which to find a random position.</param>
    /// <param name="range">The maximum distance from the center.</param>
    /// <param name="result">The output for the NavMesh position if found. Otherwise, it is Vector3.zero</param>
    /// <returns>Returns true if a valid point on the NavMesh is found, returns false otherwise</returns>
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}