using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(ParticleSystem))]
public class TeleportPoof : NetworkBehaviour
{
    private ParticleSystem _poof;
    
    // When the object is created, play the particle system
    void Start()
    {
        _poof = gameObject.GetComponent<ParticleSystem>();
        _poof.Play();
    }

    // Destroy the gameObject once the particle system has stopped
    void Update()
    {
        if ( _poof.isStopped )
        {
            Destroy(gameObject);
        }
    }
}
