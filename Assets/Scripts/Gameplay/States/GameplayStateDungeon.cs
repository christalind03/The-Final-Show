using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Dungeon Gameplay State", menuName = "Base State/Gameplay/Dungeon")]
public class GameplayStateDungeon : GameplayState
{
    private SafeZone _safeZone;

    public override void EnterState()
    {
        Debug.Log("Entering the DUNGEON gameplay state...");
        base.EnterState();
    }

    // TODO: Document
    protected override void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        if (activeScene.name != TargetScene) { return; }

        GameplayManager.Instance.FindObject((SafeZone targetObject) =>
        {
            if (targetObject != null)
            {
                Debug.Log("Safe Zone found successfully!");
                _safeZone = targetObject;
            }
        });

        // The following code is the base implementation of OnSceneLoaded with
        // countdown timer callback adjustments specifically for this state
        
        if (IsTimed)
        {
            CustomNetworkManager.Instance.Countdown(CountdownMessage, () =>
            {
                if (_safeZone != null && _safeZone.ContainsPlayers)
                {
                    Debug.Log("Transitioning to the boss state...");
                    GameplayManager.Instance.TransitionToState(GameplayManager.State.Boss);

                    List<GameObject> invalidPlayers = NetworkUtils.RetrievePlayers().Except(_safeZone.SafePlayers).ToList();

                    foreach (GameObject invalidPlayer in invalidPlayers)
                    {
                        NetworkIdentity invalidPlayerIdentity = invalidPlayer.GetComponent<NetworkIdentity>();

                        invalidPlayerIdentity.connectionToClient.Send(new SpectateMessage() { });
                    }
                }
                else
                {
                    GameplayManager.Instance.TransitionToState(TransitionState);
                }
            });
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting the DUNGEON gameplay state...");
        base.ExitState();
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
