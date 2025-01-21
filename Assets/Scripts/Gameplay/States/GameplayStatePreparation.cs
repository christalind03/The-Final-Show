using UnityEngine;

[CreateAssetMenu(fileName = "New Preparation Gameplay State", menuName = "Base State/Gameplay/Preparation")]
public class GameplayStatePreparation : GameplayState
{
    public override void EnterState()
    {
        Debug.Log("Entering the PREPARATION gameplay state...");
        base.EnterState();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting the PREPARATION gameplay state...");
        base.ExitState();
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
