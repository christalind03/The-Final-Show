using Mirror;
using UnityEngine;

public class InteractableGameplayController : NetworkBehaviour, IInteractable
{
    [SerializeField] private GameplayManager.State _targetState;

    // TODO: Document
    public void Interact(GameObject playerObject)
    {
        if (!isServer) { return; }

        GameplayManager.Instance.TransitionToState(_targetState);
    }
}
