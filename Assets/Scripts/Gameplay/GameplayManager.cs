using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : StateManager<GameplayManager.State, GameplayState, GameplayContext>
{
    public enum State
    {
        Boss,
        Defeat,
        Dungeon,
        Intermission,
        Preparation,
    }

    public static GameplayManager Instance { get; private set; }

    private int _startPositionIndex;

    // TODO: Document
    public override void OnStartServer()
    {
        if (Instance == null)
        {
            CustomNetworkManager customNetworkManager = GameObject.FindObjectOfType<CustomNetworkManager>();

            Instance = this;
            StateContext = new GameplayContext(customNetworkManager, this);

            _startPositionIndex = 0;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // TODO: Document
    public void BroadcastScene(string destinationScene)
    {
        foreach (NetworkConnectionToClient clientConnection in NetworkServer.connections.Values)
        {
            if (clientConnection.identity != null)
            {
                StartCoroutine(AssignScene(clientConnection.identity.gameObject, destinationScene));
            }
        }

        _startPositionIndex = 0;
    }

    // TODO: Document
    private IEnumerator AssignScene(GameObject targetPlayer, string destinationScene)
    {
        if (!targetPlayer.TryGetComponent(out NetworkIdentity clientIdentity)) { yield break; }

        NetworkConnectionToClient clientConnection = clientIdentity.connectionToClient;
        if (clientConnection == null) { yield break; }

        clientConnection.Send(new SceneMessage
        {
            sceneName = targetPlayer.scene.name,
            sceneOperation = SceneOperation.UnloadAdditive,
            customHandling = true,
        });

        // TODO: Perform a WaitForSeconds until the scene transition has completed

        NetworkServer.RemovePlayerForConnection(clientConnection);

        Transform startPosition = RetrieveStartPosition(destinationScene);
        if (startPosition != null)
        {
            targetPlayer.transform.position = startPosition.position;
            targetPlayer.transform.LookAt(Vector3.up);
        }

        SceneManager.MoveGameObjectToScene(targetPlayer, SceneManager.GetSceneByPath(destinationScene));

        clientConnection.Send(new SceneMessage
        {
            sceneName = destinationScene,
            sceneOperation = SceneOperation.LoadAdditive,
            customHandling = true,
        });

        NetworkServer.AddPlayerForConnection(clientConnection, targetPlayer);
    }

    // TODO: Document
    private Transform RetrieveStartPosition(string destinationScene)
    {
        List<Transform> startPositions = GameObject
            .FindObjectsOfType<NetworkStartPosition>()
            .Where(startPosition => startPosition.gameObject.scene.name == Path.GetFileNameWithoutExtension(destinationScene))
            .Select(startPosition => startPosition.transform)
            .ToList();

        // Remove any dead transforms
        startPositions.RemoveAll(transform => transform == null);

        if (startPositions.Count == 0) { return null; }

        if (StateContext.CustomNetworkManager.playerSpawnMethod == PlayerSpawnMethod.Random)
        {
            return startPositions[UnityEngine.Random.Range(0, startPositions.Count)];
        }
        else
        {
            Transform startPosition = startPositions[_startPositionIndex];
            _startPositionIndex = (_startPositionIndex + 1) % startPositions.Count;
            return startPosition;
        }
    }

    // TODO: Document
    public void LocateObject<TComponent>(Action<TComponent> onFound) where TComponent : Component
    {
        StartCoroutine(UnityUtils.WaitForObject(GeneralUtils.DefaultTimeout, onFound));
    }
}
