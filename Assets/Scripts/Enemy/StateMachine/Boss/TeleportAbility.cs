using Mirror;
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
    [SerializeField] GameObject _poofPrefab;
    public override void EnterState()
    {
        StateContext.NavMeshAgent.isStopped = true;
        StateContext.Animator.SetBool("Is Teleporting", true);
        StateContext.MonoBehaviour.StartCoroutine(Teleport());
    }

    public override void ExitState()
    {
        StateContext.Animator.SetBool("Is Teleporting", false);
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }

    private IEnumerator Teleport()
    {
        // Spawn the smoke slightly in front of the enemy since it takes a small amount of time for it to stop moving
        Vector3 startLoc = StateContext.Transform.position + StateContext.Transform.forward * StateContext.NavMeshAgent.velocity.magnitude / 4; // 4 just feels like a good value
        GameObject startPoof = Instantiate(_poofPrefab, startLoc, Quaternion.identity);
        NetworkServer.Spawn(startPoof);
        yield return new WaitForSeconds(1.0f); // Wait 1 second for wind-up animation
        // Get a location behind the current target and warp the enemy there
        Vector3 dest = _teleportDistance * (StateContext.TargetTransform.position - StateContext.Transform.position).normalized;
        dest = dest + StateContext.TargetTransform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dest, out hit, 1.0f, NavMesh.AllAreas))
        {
            GameObject destPoof = Instantiate(_poofPrefab, hit.position, Quaternion.identity);
            NetworkServer.Spawn(destPoof);
            yield return new WaitForSeconds(0.5f); // Small delay to let some smoke spawn before actually warping
            StateContext.NavMeshAgent.Warp(hit.position);
            StateContext.Transform.LookAt(StateContext.TargetTransform);
        }
        else
        {
            Debug.Log("Ninja teleport sample failed");
        }
    }
}
