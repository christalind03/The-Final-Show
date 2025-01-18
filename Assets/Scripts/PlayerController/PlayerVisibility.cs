using Mirror;
using UnityEngine;

public class PlayerVisibility : NetworkBehaviour
{
    private CharacterController _characterController;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    // TODO: Document
    private void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController>();
        _skinnedMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    // TODO: Document
    [Command(requiresAuthority = true)]
    public void CmdToggleVisibility(bool isVisible)
    {
        _characterController.enabled = isVisible;
        _skinnedMeshRenderer.enabled = isVisible;
        RpcToggleVisbility(isVisible);
    }

    // TODO: Docuemnt
    [ClientRpc]
    public void RpcToggleVisbility(bool isVisible)
    {
        _characterController.enabled = isVisible;
        _skinnedMeshRenderer.enabled = isVisible;
    }
}
