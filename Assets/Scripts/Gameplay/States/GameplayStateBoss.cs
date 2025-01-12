using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Gameplay State", menuName = "Base State/Gameplay/Boss")]
public class GameplayStateBoss : GameplayState
{
    public override void EnterState()
    {
        Debug.Log("Entering the BOSS gameplay state...");
        base.EnterState();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting the BOSS gameplay state...");
        base.ExitState();
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
