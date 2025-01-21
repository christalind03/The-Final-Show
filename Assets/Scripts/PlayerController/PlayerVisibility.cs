using Mirror;
using UnityEngine;

public class PlayerVisibility : NetworkBehaviour
{
    private CharacterController _characterController;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    /// <summary>
    /// Initializes the chracter's controller and skinned mesh renderer component references.
    /// </summary>
    private void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController>();
        _skinnedMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    /// <summary>
    /// Toggles the visibility of the character's controller and skinned mesh renderer on the server.
    /// </summary>
    /// <param name="isVisible">Indicates whether or not the character should be visible or not</param>
    [Command]
    public void CmdToggleVisibility(bool isVisible)
    {
        _characterController.enabled = isVisible;
        _skinnedMeshRenderer.enabled = isVisible;
        RpcToggleVisbility(isVisible);
    }

    /// <summary>
    /// Toggles the visibility of the character's controller and skinned mesh renderer on all clients.
    /// </summary>
    /// <param name="isVisible">Indicates whether or not the character should be visible or not</param>
    [ClientRpc]
    public void RpcToggleVisbility(bool isVisible)
    {
        _characterController.enabled = isVisible;
        _skinnedMeshRenderer.enabled = isVisible;
    }
}
