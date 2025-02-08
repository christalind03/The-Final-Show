using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Manuscript : NetworkBehaviour
{
    
    private GameplayManager gameplayManager;
    [SerializeField] private float rotationSpeed = 10f;
    
    /// <summary>
    /// Initializes the script object with the GameplayManager
    /// </summary>
    void Start()
    {
        if (!isServer) { return; }
        // Get GameplayManager
        gameplayManager = GameplayManager.Instance;
        if ( gameplayManager == null )
        {
            Debug.Log("Gameplay Manager not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) { return; }
        // TODO: Make it spin or something cool
        transform.Rotate(0, Time.deltaTime*rotationSpeed, 0);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!isServer) { return; }
        // If the script collides with a player
        if (collider.gameObject.tag == "Player")
        {
            // Increment player's script scoreboard count (future maybe)
            // Increment total scripts in gameplayManager
            if (gameplayManager != null)
            {
                gameplayManager.CollectScript();
            }
            Debug.Log("Script Collected");
            // Destroy object
            Destroy(gameObject);
        }
    }
}
