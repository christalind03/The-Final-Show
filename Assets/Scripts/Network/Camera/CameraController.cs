using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;
using Unity.VisualScripting;

public class CameraController : NetworkBehaviour
{
    public GameObject cameraHolder;
    [SerializeField] private CinemachineVirtualCamera vc;

    public override void OnStartAuthority()
    {
        if(isOwned){
            cameraHolder.SetActive(true);
            vc.Priority = 1;
        }else{
            vc.Priority = 0;
        }
        base.OnStartAuthority();
    }
}
