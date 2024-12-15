using UnityEngine;
using Mirror;
using Cinemachine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Controls the cameras when the player loads in to make sure the correct camera is assigned to each player
/// </summary>
public class CameraController : NetworkBehaviour
{
    public bool alive = true;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private UIDocument uiDocument;
    private CinemachineVirtualCamera[] virtualCameras;
    [SyncVar] private List<string> players = new List<string>();
    private VisualTreeAsset spectatorUI;
    int cameraIndex;

    //UI stuff
    private VisualElement ui;
    private TextElement currentPlayer;
    private Button Previous;
    private Button Next;

    /// <summary>
    /// On the start of the client connection, only enable the uidocument that is owned by the current player so other uidocuments are not shown
    /// </summary>
    public override void OnStartClient() {
        uiDocument = GetComponent<UIDocument>();
        cameraIndex = -1;
        
        if (isOwned)
        {
            uiDocument.enabled = true;
        }
        else
        {
            uiDocument.enabled = false;
        }
        
        base.OnStartClient();
    }

    /// <summary>
    /// Similar to unity start function but will only run for objects that the player has authority over
    /// </summary>
    public override void OnStartAuthority() {
        spectatorUI = Resources.Load<VisualTreeAsset>("UI/SpectateUI");
        
        // Checks if the camera is owned by the player, enables the gameobject and set the priority of the different cinemachine
        if (isOwned)
        {
            cameraHolder.SetActive(true);
            virtualCamera.Priority = 1;
        }
        else
        {
            virtualCamera.Priority = 0;
        }
        
        base.OnStartAuthority();
    }

    /// <summary>
    /// Switch the player's gameplay mode into spectator made after they've died. Player controller will be completely disabled since 
    /// the player should not be able to move. The ui for spectator mode will also be replacing the gameplay UI.
    /// </summary>
    public void Spectate() {
        virtualCameras = FindObjectsOfType<CinemachineVirtualCamera>(); //get all cinemachine vc in the gamespace and save them into an list to loop through
        
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
            
            getCamIdx(); //all cameras are stored in a list, this gets the index of the camera associated with the local player 
            getAllPlayers(); //get the list of all names of current players in the game so it can be displayed on the bar

            // Subscribe the ui buttons 
            Previous.clicked += OnPreviousClicked;
            Next.clicked += OnNextClicked;
        }
    }

    /// <summary>
    /// When clicking on the previous button, the camera will rotation to the previous player's camera on the list of all cameras
    /// </summary>
    private void OnPreviousClicked(){
        if (cameraIndex < 0 || cameraIndex > virtualCameras.Length - 1) { cameraIndex = 0;} // Make sure the cameraIndex is located within the range of the array, if not assign it the first camera
        virtualCameras[cameraIndex].Priority = 0;
        cameraIndex--;

        if (cameraIndex < 0){ cameraIndex = virtualCameras.Length - 1;} // Circular loop back to the top of the list
        virtualCameras[cameraIndex].Priority = 1;
        currentPlayer.text = players[cameraIndex];
    }

    /// <summary>
    /// When clicking the next button, the camera will rotation to the next player's camera on the list of all cameras
    /// </summary>
    private void OnNextClicked(){
        if (cameraIndex < 0 || cameraIndex > virtualCameras.Length - 1) { cameraIndex = 0;} //make sure the cameraIdex is located within the range of the array, if not assign it the first camera

        virtualCameras[cameraIndex].Priority = 0;
        cameraIndex++;
        cameraIndex%=virtualCameras.Length; //circular loop back to the bottom of the list
        virtualCameras[cameraIndex].Priority = 1;
        currentPlayer.text = players[cameraIndex];
    }

    /// <summary>
    /// Calls the server and makes the server fill out the list of all players' name 
    /// </summary>
    [Command]
    private void getAllPlayers() {
        players.Clear();
        
        foreach (CinemachineVirtualCamera cam in virtualCameras)
        {
            players.Add(cam.transform.parent.name);
        }
        
        TargetUpdatePlayerList(players);
    }

    /// <summary>
    /// Update the client's players list with the list of all players' name from the server. 
    /// Also sets the current player text of the UI.
    /// </summary>
    /// <param name="updatedPlayers">updatePlayers is the list of player names</param>
    [TargetRpc]
    private void TargetUpdatePlayerList(List<string> updatedPlayers) {
        players = updatedPlayers;
        currentPlayer.text = players[cameraIndex];
    }

    /// <summary>
    /// Using network identity, this finds the index of the local player's camera in the list of cameras
    /// </summary>
    [Command]
    private void getCamIdx() {
        NetworkIdentity identity = connectionToClient.identity;
        if (identity != null)
        {
            // Access the GameObject associated with the NetworkIdentity
            GameObject playerGameObject = identity.gameObject;
            foreach (CinemachineVirtualCamera cam in virtualCameras)
            {
                cameraIndex++;
                if (cam.transform.parent.name == playerGameObject.name) { break; }
            }
        }
        else
        {
            Debug.LogWarning("No NetworkIdentity found for this connection.");
        }

        TargetUpdateCamIdx(cameraIndex);
    }

    /// <summary>
    /// Updates the target client's cameraIndex with the index found by the server
    /// </summary>
    /// <param name="cam">Camera index in the camera list</param>
    [TargetRpc]
    private void TargetUpdateCamIdx(int cam) {
        cameraIndex = cam;
    }
}