using UnityEngine;

[CreateAssetMenu(fileName = "New Intermission Gameplay State", menuName = "Base State/Gameplay/Intermission")]
public class GameplayStateIntermission : GameplayState
{
    public override void EnterState()
    {
        Debug.Log("Entering the INTERMISSION gameplay state...");
        base.EnterState();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting the INTERMISSION gameplay state...");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
