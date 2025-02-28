using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Spawns mannequins with different armor at predefined spawn points.
/// Each spawn point will always spawn a mannequin, but the mannequin type is randomly chosen.
/// </summary>
public class MannequinSpawner : MonoBehaviour
{
    // These will each have different equippables
    [SerializeField] private GameObject[] mannequinPrefabs;
    // Spawn points for the mannequins
    [SerializeField] private Transform[] spawnPoints;
    /// <summary>
    /// Triggers the spawn
    /// </summary>
    private void Start()
    {
        SpawnMannequins();
    }
    /// <summary>
    /// Iterates through each spawn point and spawns a randomly selected mannequin prefab: item randomization.
    /// </summary>
    private void SpawnMannequins()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            int randomIndex = Random.Range(0, mannequinPrefabs.Length);
            Instantiate(mannequinPrefabs[randomIndex], spawnPoint.position, spawnPoint.rotation);
        }
    }
}



