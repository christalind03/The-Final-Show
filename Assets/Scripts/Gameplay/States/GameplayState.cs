using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameplayState : BaseState<GameplayManager.State, GameplayContext>
{
    [SerializeField]
    [Tooltip("The scene associated with the Gameplay State")]
    private SceneAsset _targetScene;

    // TODO: Documentation
    public override void EnterState()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.name != _targetScene.name)
        {
            NetworkManager.singleton.ServerChangeScene(_targetScene.name);
        }
    }
}
