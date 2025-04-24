using Mirror;
using UnityEngine;

public class PlayerVisibility : NetworkBehaviour
{
    private CharacterController _characterController;
    private GameObject _model;

    /// <summary>
    /// Initializes the chracter's controller and skinned mesh renderer component references.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        _characterController = gameObject.GetComponent<CharacterController>();
        _model = gameObject.transform.Find("Base_Character").gameObject;
    }

    /// <summary>
    /// Toggles the visibility of the character's controller and skinned mesh renderer on the server.
    /// </summary>
    /// <param name="isVisible">Indicates whether or not the character should be visible or not</param>
    [Command]
    public void CmdToggleVisibility(bool isVisible)
    {
        _characterController.enabled = isVisible;
        _model.SetActive(isVisible);
        RpcToggleVisbility(isVisible);
    }

    /// <summary>
    /// Toggles the visibility of the character's controller and skinned mesh renderer on all clients.
    /// </summary>
    /// <param name="isVisible">Indicates whether or not the character should be visible or not</param>
    [ClientRpc]
    public void RpcToggleVisbility(bool isVisible)
    {
        if (isServer) return;
        _characterController.enabled = isVisible;
        _model.SetActive(isVisible);
    }
}
