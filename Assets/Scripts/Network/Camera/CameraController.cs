using UnityEngine;
using Mirror;
using Cinemachine;


/// <summary>
/// Controls the cameras when the player loads in to make sure the correct camera is assigned to each player
/// </summary>
public class CameraController : NetworkBehaviour
{
    public GameObject cameraHolder;
    [SerializeField] private CinemachineVirtualCamera vc;

    /// <summary>
    /// Similar to unity start function but will only run for objects that the player has authority over
    /// </summary>
    public override void OnStartAuthority()
    {
        // checks if the camera is owned by the player, enables the gameobject and set the priority of the different cinemachine
        if(isOwned){
            cameraHolder.SetActive(true);
            vc.Priority = 1;
        }else{
            vc.Priority = 0;
        }
        base.OnStartAuthority();
    }
}
