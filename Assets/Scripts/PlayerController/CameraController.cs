using UnityEngine;
using Mirror;
using Cinemachine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;
using Steamworks;

/// <summary>
/// Controls the cameras when the player loads in to make sure the correct camera is assigned to each player
/// </summary>
public class CameraController : NetworkBehaviour
{
    public bool alive = true;

    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private GameObject virtualCameras;
    private List<string> playerName;
    private List<GameObject> playerObj;

    private UIDocument uiDocument;
    private GameObject currentVirtualCameraHolder;
    int playerIndex;

    // UI Stuff
    private TextElement currentPlayer;

    /// <summary>
    /// On the start of the client connection, only enable the uidocument that is owned by the current player so other uidocuments are not shown
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        uiDocument = GetComponent<UIDocument>();
        playerIndex = -1;
        currentVirtualCameraHolder = virtualCameras;

        // Checks if the camera/ui is owned by the player, enables the gameobject
        if (isOwned)
        {
            uiDocument.enabled = true;
            cameraHolder.SetActive(true);
        }
        else
        {
            uiDocument.enabled = false;
            cameraHolder.SetActive(false);
            virtualCameras.SetActive(false);
        }
    }

    /// <summary>
    /// Similar to unity start function but will only run for objects that the player has authority over
    /// </summary>
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        playerName = new List<string>();
        playerObj = new List<GameObject>();
    }

    /// TO DO: Documentation 
    [TargetRpc]
    public void TargetPlay(NetworkConnectionToClient target)
    {
        if (isOwned)
        {
            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;

            alive = true;
            VisualElement rootVisualElement = uiDocument.rootVisualElement;
            if (UnityUtils.ContainsElement(rootVisualElement, "Primary-Container", out VisualElement primContainer))
            {
                primContainer.AddToClassList("primaryContainer");
                primContainer.RemoveFromClassList("secondaryContainer");
            }

            if (UnityUtils.ContainsElement(rootVisualElement, "Spectator-Container", out VisualElement secContainer))
            {
                secContainer.AddToClassList("secondaryContainer");
                secContainer.RemoveFromClassList("primaryContainer");
            }

            gameObject.GetComponent<PlayerInterface>().enabled = true;
            gameObject.GetComponent<PlayerController>().enabled = true;

            gameObject.layer = 6;
            currentVirtualCameraHolder.transform.Find("FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>().Priority = 0;
            virtualCameras.transform.Find("FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>().Priority = 1;
            currentVirtualCameraHolder.gameObject.SetActive(false);
            virtualCameras.SetActive(true);
        }
    }

    /// <summary>
    /// Switch the player's gameplay mode into spectator made after they've died.
    /// </summary>
    public void Spectate()
    {
        if (isOwned && !alive)
        {
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;

            gameObject.GetComponent<PlayerController>().enabled = false;
            gameObject.GetComponent<PlayerInterface>().enabled = false;

            VisualElement rootVisualElement = uiDocument.rootVisualElement;
            if (UnityUtils.ContainsElement(rootVisualElement, "Primary-Container", out VisualElement primContainer))
            {
                primContainer.AddToClassList("secondaryContainer");
                primContainer.RemoveFromClassList("primaryContainer");
            }

            if (UnityUtils.ContainsElement(rootVisualElement, "Spectator-Container", out VisualElement secContainer))
            {
                secContainer.AddToClassList("primaryContainer");
                secContainer.RemoveFromClassList("secondaryContainer");
            }

            if (UnityUtils.ContainsElement(rootVisualElement, "Current", out Label Cur))
            {
                currentPlayer = Cur;
            }
            
            if (UnityUtils.ContainsElement(rootVisualElement, "Pre", out Button PreBtn))
            {
                PreBtn.clicked += OnPreviousClicked;
            }

            if (UnityUtils.ContainsElement(rootVisualElement, "Next", out Button NextBtn))
            {
                NextBtn.clicked += OnNextClicked;
            }

            CmdRequestPlayerList(); 
        }
    }

    /// <summary>
    /// When clicking on the previous button, the camera will rotation to the previous player's camera on the list of all cameras
    /// </summary>
    private void OnPreviousClicked()
    {
        if (playerIndex < 0 || playerIndex > playerObj.Count - 1) { playerIndex = 0; } // Make sure the cameraIndex is located within the range of the array, if not assign it the first camera
        GameObject oldVirtualCameras = playerObj[playerIndex].transform.Find("VirtualCameras").gameObject;
        CinemachineVirtualCamera oldFollowCam = playerObj[playerIndex].transform.Find("VirtualCameras/FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        oldFollowCam.Priority = 0;
        
        playerIndex--;
        if (playerIndex < 0) { playerIndex = playerObj.Count - 1; } // Circular loop back to the top of the list
        playerObj[playerIndex].transform.Find("VirtualCameras").gameObject.SetActive(true);
        CinemachineVirtualCamera FollowCam = playerObj[playerIndex].transform.Find("VirtualCameras/FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        FollowCam.Priority = 1;
        
        oldVirtualCameras.SetActive(false);
        currentPlayer.text = playerName[playerIndex];
        currentVirtualCameraHolder = playerObj[playerIndex].transform.Find("VirtualCameras").gameObject;
    }

    /// <summary>
    /// When clicking the next button, the camera will rotation to the next player's camera on the list of all cameras
    /// </summary>
    private void OnNextClicked()
    {
        if (playerIndex < 0 || playerIndex > playerObj.Count - 1) { playerIndex = 0; } //make sure the camIdx is located within the range of the array, if not assign it the first camera
        GameObject oldVirtualCameras = playerObj[playerIndex].transform.Find("VirtualCameras").gameObject;
        CinemachineVirtualCamera oldFollowCam = playerObj[playerIndex].transform.Find("VirtualCameras/FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        oldFollowCam.Priority = 0;
        
        playerIndex++;
        playerIndex %= playerObj.Count; // Circular loop back to the bottom of the list
        playerObj[playerIndex].transform.Find("VirtualCameras").gameObject.SetActive(true);
        CinemachineVirtualCamera FollowCam = playerObj[playerIndex].transform.Find("VirtualCameras/FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        FollowCam.Priority = 1;
        
        oldVirtualCameras.SetActive(false);
        currentPlayer.text = playerName[playerIndex];
        currentVirtualCameraHolder = playerObj[playerIndex].transform.Find("VirtualCameras").gameObject;
    }

    
    /// <summary>
    /// Request the player list from server
    /// </summary>
    [Command]
    private void CmdRequestPlayerList()
    {
        List<string> playerNames = new List<string>();
        List<GameObject> playerObjs = new List<GameObject>();
        foreach(var entry in PlayerManager.GetObjectList()){
            playerObjs.Add(entry.Value);
        }
        foreach(var entry in PlayerManager.GetPlayerNameList()){
            playerNames.Add(entry.Value);
        }
        TargetRpcRequestPlayerList(playerNames, playerObjs);
    }


    /// <summary>
    /// Server pass back the player list to the requested client
    /// </summary>
    /// <param name="playerNames">list of all player names</param>
    /// <param name="playerObjs">list of all player objects</param>
    [TargetRpc]
    private void TargetRpcRequestPlayerList(List<string> playerNames, List<GameObject> playerObjs)
    {
        playerName.Clear();
        playerObj.Clear();
        playerIndex = -1;
        playerName = playerNames;
        playerObj = playerObjs;

        foreach(GameObject curObj in playerObj){
            playerIndex++;
            if(curObj.GetComponent<NetworkIdentity>().netId == gameObject.GetComponent<NetworkIdentity>().netId){ break;}
        }
        currentPlayer.text = playerName[playerIndex];
    }

}