using UnityEngine;
using Mirror;
using System.Collections;

[RequireComponent(typeof(NetworkIdentity))]
public class MannequinSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] mannequinPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool spawnOnStart = true;

    private void Awake()
    {
        Debug.Log("MannequinSpawner Awake");
    }

    private void Start()
    {
        Debug.Log("MannequinSpawner Start");
    }

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

    [Server]
    public void SpawnMannequins()
    {
        Debug.Log($"MannequinSpawner SpawnMannequins - Prefab count: {mannequinPrefabs.Length}, Spawn points: {spawnPoints.Length}");
        
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (mannequinPrefabs.Length > 0)
            {
                GameObject randomMannequin = mannequinPrefabs[Random.Range(0, mannequinPrefabs.Length)];
                Debug.Log($"Instantiating mannequin: {randomMannequin.name} at {spawnPoint.position}");
                
                GameObject spawnedMannequin = Instantiate(randomMannequin, spawnPoint.position, spawnPoint.rotation);
                
                // Get all NetworkIdentity components in the spawned mannequin and its children
                NetworkIdentity[] identities = spawnedMannequin.GetComponentsInChildren<NetworkIdentity>();
                Debug.Log($"Found {identities.Length} NetworkIdentity components");
                
                // First spawn the root object
                NetworkServer.Spawn(spawnedMannequin);
                
                // Then spawn all networked objects
                foreach (NetworkIdentity identity in identities)
                {
                    if (identity.gameObject != spawnedMannequin) // Skip the root object -- already spawned
                    {
                        Debug.Log($"Spawning networked object: {identity.gameObject.name}");
                        NetworkServer.Spawn(identity.gameObject);
                    }
                }
                
                Debug.Log($"Spawned mannequin with {identities.Length} networked objects at {spawnPoint.position}");
            }
        }
    }
} 