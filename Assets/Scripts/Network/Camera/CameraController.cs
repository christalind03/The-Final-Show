using UnityEngine;
using Mirror;
using Cinemachine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;

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
    private VisualTreeAsset spectatorUI;
    int playerIndex;

    // UI Stuff
    private VisualElement ui;
    private TextElement currentPlayer;
    private Button Previous;
    private Button Next;

    /// <summary>
    /// On the start of the client connection, only enable the uidocument that is owned by the current player so other uidocuments are not shown
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        uiDocument = GetComponent<UIDocument>();
        playerIndex = -1;

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
        spectatorUI = Resources.Load<VisualTreeAsset>("UI/SpectateUI");
        playerName = new List<string>();
        playerObj = new List<GameObject>();
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
            uiDocument.visualTreeAsset = spectatorUI;

            GetComponent<PlayerController>().enabled = false;

            ui = uiDocument.rootVisualElement;
            currentPlayer = ui.Q<VisualElement>("Tools").Q<TextElement>("Current");
            Previous = ui.Q<VisualElement>("Tools").Q<Button>("Pre");
            Next = ui.Q<VisualElement>("Tools").Q<Button>("Next");

            CmdRequestPlayerList(); 

            

            // Subscribe the UI buttons 
            Previous.clicked += OnPreviousClicked;
            Next.clicked += OnNextClicked;
        }
    }

    /// <summary>
    /// When clicking on the previous button, the camera will rotation to the previous player's camera on the list of all cameras
    /// </summary>
    private void OnPreviousClicked()
    {
        if (playerIndex < 0 || playerIndex > playerObj.Count - 1) { playerIndex = 0; } // Make sure the cameraIndex is located within the range of the array, if not assign it the first camera
        CinemachineVirtualCamera oldAimCam = playerObj[playerIndex].transform.Find("AimCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        CinemachineVirtualCamera oldFollowCam = playerObj[playerIndex].transform.Find("FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        
        playerIndex--;
        if (playerIndex < 0) { playerIndex = playerObj.Count - 1; } // Circular loop back to the top of the list
        playerObj[playerIndex].GetComponentInChildren<CinemachineVirtualCamera>().gameObject.transform.parent.gameObject.SetActive(true);

        

        currentPlayer.text = playerName[playerIndex];
    }

    /// <summary>
    /// When clicking the next button, the camera will rotation to the next player's camera on the list of all cameras
    /// </summary>
    private void OnNextClicked()
    {
        Debug.Log(playerIndex);
        if (playerIndex < 0 || playerIndex > playerObj.Count - 1) { playerIndex = 0; } //make sure the camIdx is located within the range of the array, if not assign it the first camera

        playerObj[playerIndex].GetComponent<CinemachineVirtualCamera>().gameObject.SetActive(false);
        playerIndex++;
        playerIndex %= playerObj.Count; // Circular loop back to the bottom of the list
        playerObj[playerIndex].GetComponent<CinemachineVirtualCamera>().gameObject.SetActive(true);
        currentPlayer.text = playerName[playerIndex];
    }

    
    /// <summary>
    /// Request the player list from server
    /// </summary>
    [Command]
    private void CmdRequestPlayerList()
    {
        List<string> playerNames = new List<string>();
        List<GameObject> playerObjs = new List<GameObject>();
        foreach(var entry in PlayerManager.GetPlayerNameList()){
            playerNames.Add(entry.Key);
            playerObjs.Add(entry.Value);
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