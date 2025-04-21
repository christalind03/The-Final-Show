using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// Handles the spawning of mannequins in a networked environment -- instantiating and networking mannequins at specified spawn points.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class MannequinSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] mannequinPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool spawnOnStart = true;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        Debug.Log("MannequinSpawner Awake");
    }

    /// <summary>
    /// Called on the frame when the script is enabled just before any of the Update methods are called the first time.
    /// </summary>
    private void Start()
    {
        Debug.Log("MannequinSpawner Start");
    }

    /// <summary>
    /// Called on the server when the object is spawned.
    /// Initializes the spawner and starts the spawning process if spawnOnStart is true.
    /// </summary>
    public override void OnStartServer()
    {
        Debug.Log("MannequinSpawner OnStartServer");
        
        if (mannequinPrefabs == null || mannequinPrefabs.Length == 0)
        {
            Debug.LogError("No mannequin prefabs assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        // Wait for the network to be ready
        StartCoroutine(WaitForNetworkReady());
    }

    /// <summary>
    /// Waits for the network to be ready before spawning mannequins.
    /// </summary>
    private IEnumerator WaitForNetworkReady()
    {
        // Wait until the network is ready
        while (!NetworkServer.active)
        {
            yield return null;
        }

        if (spawnOnStart)
        {
            Debug.Log("Network is ready, attempting to spawn mannequins");
            SpawnMannequins();
        }
    }

    /// <summary>
    /// Spawns mannequins at each spawn point -- called on the server and will spawn a random mannequin from the mannequinPrefabs array at each spawn point.
    /// </summary>
    [Server]
    public void SpawnMannequins()
    {
        Debug.Log($"MannequinSpawner SpawnMannequins - Prefab count: {mannequinPrefabs.Length}, Spawn points: {spawnPoints.Length}");
        
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (mannequinPrefabs.Length > 0)
            {
                // Spawn a random mannequin at this point
                GameObject randomMannequin = mannequinPrefabs[Random.Range(0, mannequinPrefabs.Length)];
                Debug.Log($"Spawning mannequin: {randomMannequin.name} at {spawnPoint.position}");
                
                // Instantiate the mannequin directly at the spawn point
                GameObject spawnedMannequin = Instantiate(randomMannequin, spawnPoint.position, spawnPoint.rotation);

                // Verify NetworkIdentity
                if (!spawnedMannequin.TryGetComponent<NetworkIdentity>(out var netId))
                {
                    Debug.LogError($"Spawned mannequin {spawnedMannequin.name} is missing NetworkIdentity!");
                }
                else
                {
                    Debug.Log($"Spawning networked mannequin {spawnedMannequin.name} with netId {netId.netId}");
                }
                
                // Spawn the mannequin on the network
                NetworkServer.Spawn(spawnedMannequin);
                
                Debug.Log($"Spawned mannequin at {spawnPoint.position}");
            }
        }
    }

    /// <summary>
    /// Called on clients when the object is spawned.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("MannequinSpawner OnStartClient");
    }
} 