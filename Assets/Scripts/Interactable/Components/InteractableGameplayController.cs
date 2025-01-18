using Mirror;
using System;
using UnityEngine;

public class InteractableGameplayController : NetworkBehaviour, IInteractable
{
    [SerializeField] private GameplayManager.State _targetState;

    [Header("Countdown Properties")]
    [SerializeField] private bool _enableCountdown;
    [SerializeField] private CountdownMessage _countdownProperties;
   
    private CustomNetworkManager _networkManager;

    /// <summary>
    /// Locates the instance of the custom network manager in the scene.
    /// </summary>
    private void Start()
    {
        _networkManager = GameObject.FindAnyObjectByType<CustomNetworkManager>();
    }

    /// <summary>
    /// Handles interaction with the specified player.
    /// If the server is active, it either initiates a countdown before transitioning to a new state
    /// or immediately performs the state transition.
    /// </summary>
    /// <param name="playerObject">The player object that triggered the interaction</param>
    public void Interact(GameObject playerObject)
    {
        if (!isServer) { return; }

        Action performStateTransition = () => GameplayManager.Instance.TransitionToState(_targetState);

        if (_enableCountdown)
        {
            _networkManager.Countdown(_countdownProperties, performStateTransition);
        }
        else
        {
            performStateTransition.Invoke();
        }
    }
}
