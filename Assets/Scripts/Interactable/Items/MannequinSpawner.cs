using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// Handles the spawning of mannequins and their skinned items in a networked environment.
/// Spawns the mannequin and its items separately, then connects them through parenting.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class MannequinSpawner : NetworkBehaviour
{
    [Header("Mannequin Settings")]
    [SerializeField] private GameObject[] mannequinPrefabs;
    
    [Header("Item Settings")]
    [SerializeField] private GameObject[] skinnedItemPrefabs;
    
    [Header("Spawn Settings")]
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

        if (skinnedItemPrefabs == null || skinnedItemPrefabs.Length == 0)
        {
            Debug.LogError("No skinned item prefabs assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        Debug.Log($"MannequinSpawner setup: {mannequinPrefabs.Length} mannequin prefabs, {skinnedItemPrefabs.Length} skinned items, {spawnPoints.Length} spawn points");

        StartCoroutine(WaitForNetworkReady());
    }

    /// <summary>
    /// Waits for the network to be ready before spawning mannequins.
    /// </summary>
    private IEnumerator WaitForNetworkReady()
    {
        Debug.Log("Waiting for network to be ready...");
        while (!NetworkServer.active)
        {
            yield return null;
        }
        Debug.Log("Network is ready, attempting to spawn mannequins");
        SpawnMannequins();
    }

    /// <summary>
    /// Spawns mannequins at each spawn point -- called on the server and will spawn a random mannequin from the mannequinPrefabs array at each spawn point.
    /// </summary>
    [Server]
    public void SpawnMannequins()
    {
        Debug.Log($"MannequinSpawner SpawnMannequins - Mannequin prefabs: {mannequinPrefabs.Length}, Item prefabs: {skinnedItemPrefabs.Length}, Spawn points: {spawnPoints.Length}");
        
        foreach (Transform spawnPoint in spawnPoints)
        {
            Debug.Log($"Attempting to spawn at point: {spawnPoint.position}");
            if (mannequinPrefabs.Length > 0)
            {
                GameObject randomMannequin = mannequinPrefabs[Random.Range(0, mannequinPrefabs.Length)];
                Debug.Log($"Spawning mannequin: {randomMannequin.name} at {spawnPoint.position}");
                
                GameObject spawnedMannequin = Instantiate(randomMannequin, spawnPoint.position, spawnPoint.rotation);
                NetworkServer.Spawn(spawnedMannequin);
                Debug.Log($"Spawned mannequin with network ID: {spawnedMannequin.GetComponent<NetworkIdentity>().netId}");

                foreach (GameObject itemPrefab in skinnedItemPrefabs)
                {
                    Debug.Log($"Attempting to spawn item: {itemPrefab.name}");
                    
                    GameObject spawnedItem = Instantiate(itemPrefab, spawnedMannequin.transform.position, spawnedMannequin.transform.rotation);
                    NetworkServer.Spawn(spawnedItem);
                    
                    Debug.Log($"Spawned item {spawnedItem.name} at mannequin position");
                }
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
