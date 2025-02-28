using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Base State/Enemy/Ability/Teleport")]
public class TeleportAbilityState : EnemyState
{
    [SerializeField]
    [Tooltip("The distance from the player that the boss will teleport to")]
    private float _teleportDistance = 5f;
    public override void EnterState()
    {
        Debug.Log("Entering Teleport State");
        StateContext.Animator.SetBool("Is Teleporting", true);
        StateContext.MonoBehaviour.StartCoroutine(Teleport());
    }

    public override void ExitState()
    {
        StateContext.Animator.SetBool("Is Teleporting", false);
        Debug.Log("Leaving Teleport State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }

    private IEnumerator Teleport()
    {
        yield return new WaitForSeconds(1.0f); // Wait 1 second for wind-up animation
        // Get a location behind the current target and warp the enemy there
        Vector3 dest = _teleportDistance * (StateContext.TargetTransform.position - StateContext.Transform.position).normalized;
        dest = dest + StateContext.TargetTransform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dest, out hit, 1.0f, NavMesh.AllAreas))
        {
            StateContext.NavMeshAgent.Warp(hit.position);
            StateContext.Transform.LookAt(StateContext.TargetTransform);
        }
        else
        {
            Debug.Log("Ninja teleport sample failed");
        }
    }
}
