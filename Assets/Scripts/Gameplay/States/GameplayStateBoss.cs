using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Boss Gameplay State", menuName = "Base State/Gameplay/Boss")]
public class GameplayStateBoss : GameplayState
{
    private bool _enemyExists;
    private EnemyHealth _enemyHealth;

    public override void EnterState()
    {
        Debug.Log("Entering the BOSS gameplay state...");
        base.EnterState();
    }

    //// TODO: Document
    //protected override void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    //{
    //    base.OnSceneLoaded(activeScene, loadMode);

    //    StateContext.GameplayManager.FindObject((EnemyHealth targetObject) =>
    //    {
    //        if (targetObject != null)
    //        {
    //            Debug.Log("Boss found successfully!");
    //            _enemyExists = true;
    //            _enemyHealth = targetObject;
    //        }
    //    });
    //}

    public override void ExitState()
    {
        Debug.Log("Exiting the BOSS gameplay state...");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    
    public override void UpdateState()
    {
        // The primary enemy's health script becomes null when the enemy is defeated, as the reference is destroyed
        // However, when initially loading in to the scene, this health script is automatically set to be null
        // So, we have an additional check to ensure that the enemy existed at some point during this scene to prevent transitioning states too early
        if (_enemyExists && _enemyHealth == null)
        {
            StateContext.CustomNetworkManager.StopAllCoroutines();
            StateContext.GameplayManager.TransitionToState(GameplayManager.State.Intermission);
        }
    }
}
