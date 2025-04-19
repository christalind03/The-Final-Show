using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StarSpawner : NetworkBehaviour
{
    [SerializeField] GameObject _starPrefab;
    // Spawn an interactable star object on start
    void Start()
    {
        GameObject starObj = Instantiate(_starPrefab, gameObject.transform.position, Quaternion.identity);
        NetworkServer.Spawn(starObj);
    }
}
