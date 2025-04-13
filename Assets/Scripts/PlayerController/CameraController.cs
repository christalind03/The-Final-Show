using UnityEngine;
using Mirror;
using Cinemachine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controls the cameras when the player loads in to make sure the correct camera is assigned to each player
/// </summary>
public class CameraController : NetworkBehaviour
{
    public bool alive = true;

    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private GameObject virtualCameras;
    private Dictionary<uint, string> playerName;
    private Dictionary<uint, GameObject> playerObj;
    private List<uint> playerNetIds;

    private UIDocument uiDocument;
    private GameObject currentVirtualCameraHolder;
    private VisualElement rootVisualElement;
    private int currentCameraPos;

    // UI Stuff
    private TextElement currentPlayer;

    /// <summary>
    /// On the start of the client connection, only enable the uidocument that is owned by the current player so other uidocuments are not shown
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        currentVirtualCameraHolder = virtualCameras;
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

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
        playerName = new Dictionary<uint, string>();
        playerObj = new Dictionary<uint, GameObject>();
        playerNetIds = new List<uint>();
        uiDocument = GetComponent<UIDocument>();
        rootVisualElement = uiDocument.rootVisualElement;
        currentCameraPos = -1;
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
            rootVisualElement = uiDocument.rootVisualElement;
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

            RequestPlayerList();
            CmdRegisterSpectatorMessage();
        }
    }


    /// <summary>
    /// Add to invalid player list once the playe goes into spectator mode
    /// </summary>
    [Command]
    private void CmdRegisterSpectatorMessage()
    {
        GameplayManager gameplayManager = NetworkManager.FindObjectOfType<GameplayManager>();
        GameObject playerObj = connectionToClient.identity.gameObject;
        gameplayManager.AddInvalidPlayer(playerObj);
    }

    /// <summary>
    /// Unsubscribe to the callback events
    /// </summary>
    private void OnDisable()
    {
        if (!isOwned) return;
        if (UnityUtils.ContainsElement(rootVisualElement, "Pre", out Button PreBtn))
        {
            PreBtn.clicked -= OnPreviousClicked;
        }

        if (UnityUtils.ContainsElement(rootVisualElement, "Next", out Button NextBtn))
        {
            NextBtn.clicked -= OnNextClicked;
        }
    }

    /// <summary>
    /// When clicking on the previous button, the camera will rotation to the previous player's camera on the list of all cameras
    /// </summary>
    private void OnPreviousClicked()
    {
        if (currentCameraPos < 0 || currentCameraPos > playerObj.Count - 1) { currentCameraPos = 0; } // Make sure the cameraIndex is located within the range of the array, if not assign it the first camera
        GameObject oldVirtualCameras = playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras").gameObject;
        CinemachineVirtualCamera oldFollowCam = playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras/FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        oldFollowCam.Priority = 0;

        currentCameraPos--;
        if (currentCameraPos < 0) { currentCameraPos = playerObj.Count - 1; } // Circular loop back to the top of the list
        playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras").gameObject.SetActive(true);
        CinemachineVirtualCamera FollowCam = playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras/FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        FollowCam.Priority = 1;

        oldVirtualCameras.SetActive(false);
        currentPlayer.text = playerName[playerNetIds[currentCameraPos]];
        currentVirtualCameraHolder = playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras").gameObject;
    }

    /// <summary>
    /// When clicking the next button, the camera will rotation to the next player's camera on the list of all cameras
    /// </summary>
    private void OnNextClicked()
    {
        if (currentCameraPos < 0 || currentCameraPos > playerObj.Count - 1) { currentCameraPos = 0; } //make sure the camIdx is located within the range of the array, if not assign it the first camera
        GameObject oldVirtualCameras = playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras").gameObject;
        CinemachineVirtualCamera oldFollowCam = playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras/FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        oldFollowCam.Priority = 0;

        currentCameraPos++;
        currentCameraPos %= playerObj.Count; // Circular loop back to the bottom of the list
        playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras").gameObject.SetActive(true);
        CinemachineVirtualCamera FollowCam = playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras/FollowCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        FollowCam.Priority = 1;

        oldVirtualCameras.SetActive(false);
        currentPlayer.text = playerName[playerNetIds[currentCameraPos]];
        currentVirtualCameraHolder = playerObj[playerNetIds[currentCameraPos]].transform.Find("VirtualCameras").gameObject;
    }


    /// <summary>
    /// Request the player list
    /// </summary>
    private void RequestPlayerList()
    {
        ScoreBoard scoreboard = NetworkManager.FindObjectOfType<ScoreBoard>();
        playerName = new Dictionary<uint, string>();
        playerObj = new Dictionary<uint, GameObject>();
        playerNetIds = new List<uint>();
        currentCameraPos = -1;
        int i = 0;
        var gameObjects = NetworkManager.FindObjectsOfType<PlayerController>();

        foreach (KeyValuePair<uint, string> var in scoreboard.playerName)
        {
            playerName.Add(var.Key, var.Value);
            currentPlayer.text = playerName.GetValueOrDefault(netId);
        }

        foreach (PlayerController data in gameObjects)
        {
            playerObj.Add(data.netId, data.gameObject);
            playerNetIds.Add(data.netId);
            if (data.netId == netId)
            {
                currentCameraPos = i;
            }
            i++;
        }
    }

    private void PrintDictionaryDebug()
    {
        foreach (var data in playerName)
        {
            Debug.Log(data.Key + " " + data.Value);
        }
        foreach (var data in playerObj)
        {
            Debug.Log(data.Key + " " + data.Value.name);
        }
        foreach (var data in playerNetIds)
        {
            Debug.Log(data);
        }
        Debug.Log("Current Camera Pos " + currentCameraPos);
    }
}