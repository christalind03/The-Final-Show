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
    public GameObject cameraHolder;
    public bool alive = true;
    [SerializeField] private CinemachineVirtualCamera vc;
    [SerializeField] private UIDocument uIDocument;
    private CinemachineVirtualCamera[] vcameras;
    [SyncVar] private List<string> players = new List<string>();
    private VisualTreeAsset specUI;
    int camIdx;

    //UI stuff
    private VisualElement ui;
    private TextElement currentPlayer;
    private Button Previous;
    private Button Next;

    public override void OnStartClient() {
        uIDocument = GetComponent<UIDocument>();
        camIdx = -1;
        if(isOwned){
            uIDocument.enabled = true;
        }else{
            uIDocument.enabled = false;
        }
        base.OnStartClient();
    }

    /// <summary>
    /// Similar to unity start function but will only run for objects that the player has authority over
    /// </summary>
    public override void OnStartAuthority() {
        specUI = Resources.Load<VisualTreeAsset>("UI/SpectateUI");
        // checks if the camera is owned by the player, enables the gameobject and set the priority of the different cinemachine
        if(isOwned){
            cameraHolder.SetActive(true);
            vc.Priority = 1;
        }else{
            vc.Priority = 0;
        }
        base.OnStartAuthority();
    }

    public void Spectate() {
        vcameras = FindObjectsOfType<CinemachineVirtualCamera>();
        if(isOwned && !alive){
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            uIDocument.visualTreeAsset = specUI;

            GetComponent<PlayerControllerNetwork>().enabled = false;

            ui = uIDocument.rootVisualElement;
            currentPlayer = ui.Q<VisualElement>("Tools").Q<TextElement>("Current");
            Previous = ui.Q<VisualElement>("Tools").Q<Button>("Pre");
            Next = ui.Q<VisualElement>("Tools").Q<Button>("Next");
            

            getCamIdx();
            getAllPlayers();

            Previous.clicked += OnPreviousClicked;
            Next.clicked += OnNextClicked;
                
        }
    }

    private void OnPreviousClicked(){
        if(camIdx < 0 || camIdx > vcameras.Length - 1) { camIdx = 0;}

        vcameras[camIdx].Priority = 0;
        camIdx--;
        if(camIdx < 0){ camIdx = vcameras.Length - 1;}
        vcameras[camIdx].Priority = 1;
        currentPlayer.text = players[camIdx];
    }
    private void OnNextClicked(){
        if(camIdx < 0 || camIdx > vcameras.Length - 1) { camIdx = 0;}

        Debug.Log(camIdx);
        Debug.Log("length " + vcameras.Length);
        vcameras[camIdx].Priority = 0;
        camIdx++;
        camIdx%=vcameras.Length;
        Debug.Log(camIdx);
        vcameras[camIdx].Priority = 1;
        currentPlayer.text = players[camIdx];
    }

    [Command]
    private void getAllPlayers() {
        players.Clear();
        foreach(CinemachineVirtualCamera cam in vcameras){
            players.Add(cam.transform.parent.name);
        }
        TargetUpdatePlayerList(players);
    }

    [TargetRpc]
    private void TargetUpdatePlayerList(List<string> updatedPlayers) {
        players = updatedPlayers;
        currentPlayer.text = players[camIdx];
    }

    [Command]
    private void getCamIdx() {
        NetworkIdentity identity = connectionToClient.identity;
        if (identity != null) {
            // Access the GameObject associated with the NetworkIdentity
            GameObject playerGameObject = identity.gameObject;
            foreach(CinemachineVirtualCamera cam in vcameras){
                camIdx++;
                if(cam.transform.parent.name == playerGameObject.name) {
                    break;
                }
            }
        } else {
            Debug.LogWarning("No NetworkIdentity found for this connection.");
        }
        TargetUpdateCamIdx(camIdx);
    }

    [TargetRpc]
    private void TargetUpdateCamIdx(int cam) {
        camIdx = cam;
    }
}