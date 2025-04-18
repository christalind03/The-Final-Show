using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Base State/Enemy/Ability/Charge")]
public class ChargeAbilityState : EnemyState
{
    [SerializeField] private float _chargeSpeed = 10f;
    [SerializeField] private float _launchStrength = 10f;
    [SerializeField] private float _chargeDamage = 10f;
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

    public override void OnTriggerEnter(Collider otherCollider)
    {
        // When a player enters the trigger, deal damage
        GameObject obj = otherCollider.gameObject;
        if (obj.tag == "Player")
        {
            PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
            if (playerHealth)
            {
                playerHealth.CmdDamage(_chargeDamage);
            }
            // apply an upwards velocity
            PlayerController playerController = obj.GetComponent<PlayerController>();
            playerController.RpcExternalUp(_launchStrength);
        }
    }

    public override void OnTriggerExit(Collider otherCollider) { }
    
    // TODO: Currently is a little janky. A better solution would be to apply a velocity when a player enters the trigger.
    // This would require modifying the PlayerController script.
    public override void OnTriggerStay(Collider otherCollider)
    {
        // When a player stays in the trigger, move them away
        /*
        GameObject obj = otherCollider.gameObject;
        if (obj.tag == "Player")
        {
            PlayerController playerController = obj.GetComponent<PlayerController>();
            if (playerController)
            {
                Vector3 dir = obj.transform.position - StateContext.Transform.position;
                // Get the cross product between the boss' forward vector and the vector pointing toward the player
                // If the y-component is positive, then the player is to the boss' right
                // If the y-component is negative, then the player is to the boss' left
                Vector3 cross = Vector3.Cross(StateContext.Transform.forward, dir);
                if (cross.y > 0f)
                {
                    // apply force to boss' right
                    dir = (StateContext.Transform.forward + StateContext.Transform.right + Vector3.up).normalized;
                }
                else
                {
                    // apply force to boss' left
                    dir = (StateContext.Transform.forward - StateContext.Transform.right + Vector3.up).normalized;
                }
                Vector3 vect = dir * Time.deltaTime * _launchStrength;
                playerController.RpcExternalMove(vect);
            }
        }
        */
    }
    
    public override void UpdateState()
    {
        StateContext.NavMeshAgent.velocity = dir;
    }
}
