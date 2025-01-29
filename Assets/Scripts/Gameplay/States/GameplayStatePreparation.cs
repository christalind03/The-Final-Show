using UnityEngine;

[CreateAssetMenu(fileName = "New Preparation Gameplay State", menuName = "Base State/Gameplay/Preparation")]
public class GameplayStatePreparation : GameplayState
{
    public override void EnterState()
    {
        Debug.Log($"Gameplay Theme: {StateContext.GameplayTheme.Theme}");
        base.EnterState();
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
