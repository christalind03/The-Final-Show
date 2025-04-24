using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Base State/Enemy/Attacking")]
public class AttackingState : EnemyState
{
    public override void EnterState()
    {
        StateContext.Animator.SetBool("Is Attacking", true);
        StateContext.MonoBehaviour.StartCoroutine(Attack());
    }

    public override void ExitState() 
    {
        StateContext.Animator.SetBool("Is Attacking", false);
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }

    /// <summary>
    /// Waits an amount of time given by AttackDelay for the animation to wind up, then deals damage to the player.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(StateContext.AttackDelay);
        // TODO: Check if the target is still in range after the wind-up
        if (StateContext.TargetTransform.root.TryGetComponent(out AbstractHealth targetHealth))
        {
            targetHealth.CmdDamage(StateContext.AttackStats.AttackDamage);
        }
    }
}
