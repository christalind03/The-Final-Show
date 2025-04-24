using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Base State/Enemy/Ability/Push")]
public class PushAbilityState : EnemyState
{
    [SerializeField] private float _pushStrength = 1.5f;
    [SerializeField] private float _pushDelay = 1f;
    private bool _pushReady = false;
    public override void EnterState()
    {
        StateContext.NavMeshAgent.destination = StateContext.Transform.position; // Stop moving to use ability
        StateContext.Animator.SetBool("Is Pushing", true);
        _pushReady = false;
        StateContext.MonoBehaviour.StartCoroutine(PushDelay());
    }

    public override void ExitState()
    {
        StateContext.Animator.SetBool("Is Pushing", false);
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState()
    {
        if (_pushReady)
        {
            // go through FOV's detected objects and use their character controller to move them away each frame
            foreach (GameObject obj in StateContext.FieldOfView.DetectedObjects)
            {
                PlayerController playerController = obj.GetComponent<PlayerController>();
                if (playerController)
                {
                    Vector3 dir = (obj.transform.position - StateContext.Transform.position).normalized;
                    Vector3 vect = dir * Time.deltaTime * _pushStrength;
                    playerController.RpcExternalMove(vect);
                }
            }
        }
    }

    public IEnumerator PushDelay()
    {
        yield return new WaitForSeconds(_pushDelay);
        _pushReady = true;
    }
}

