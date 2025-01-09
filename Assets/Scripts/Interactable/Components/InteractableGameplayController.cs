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

    // TODO: Document
    private void Start()
    {
        _networkManager = GameObject.FindAnyObjectByType<CustomNetworkManager>();
    }

    // TODO: Document
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
