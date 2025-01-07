using UnityEngine;

[CreateAssetMenu(fileName = "New Dungeon Gameplay State", menuName = "Base State/Gameplay/Dungeon")]
public class GameplayStateDungeon : GameplayState
{
    public override void EnterState()
    {
        Debug.Log("Entering the DUNGEON gameplay state...");
        base.EnterState();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting the DUNGEON gameplay state...");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
