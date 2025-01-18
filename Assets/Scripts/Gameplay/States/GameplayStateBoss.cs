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

    /// <summary>
    /// When the target scene is loaded, searches for the primary enemy in the scene and initializes the enemy's health reference, if found.
    /// </summary>
    /// <param name="activeScene">The scene that was loaded</param>
    /// <param name="loadMode">The mode in which the scene was loaded</param>
    protected override void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        base.OnSceneLoaded(activeScene, loadMode);

        GameplayManager.Instance.FindObject((EnemyHealth targetObject) =>
        {
            if (targetObject != null)
            {
                _enemyExists = true;
                _enemyHealth = targetObject;
            }
        });
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    
    /// <summary>
    /// Updates the state logic during gameplay.
    /// Monitors the primary enemy's health status and triggers a state transition into <see cref="GameplayManager.State.Intermission"/>
    /// when the enemy is defeated
    /// </summary>
    public override void UpdateState()
    {
        // The primary enemy's health script becomes null when the enemy is defeated, as the reference is destroyed
        // However, when initially loading in to the scene, this health script is automatically set to be null
        // So, we have an additional check to ensure that the enemy existed at some point during this scene to prevent transitioning states too early
        if (_enemyExists && _enemyHealth == null)
        {
            CustomNetworkManager.Instance.StopAllCoroutines();
            GameplayManager.Instance.TransitionToState(GameplayManager.State.Intermission);
        }
    }
}
