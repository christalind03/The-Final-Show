using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Base State/Enemy/Ability/Charge")]
public class ChargeAbilityState : EnemyState
{
    [SerializeField] private float _chargeSpeed = 10f;
    private Vector3 dir;

    public override void EnterState()
    {
        Debug.Log("Entering Charging State");
        StateContext.Animator.SetBool("Is Charging", true);
        dir = (StateContext.TargetTransform.position - StateContext.Transform.position).normalized;
        dir.y = 0;
        dir = dir * _chargeSpeed;
        StateContext.NavMeshAgent.ResetPath();
    }

    public override void ExitState()
    {
        StateContext.Animator.SetBool("Is Charging", false);
        Debug.Log("Leaving Charging State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState()
    {
        StateContext.NavMeshAgent.velocity = dir;
    }
}
