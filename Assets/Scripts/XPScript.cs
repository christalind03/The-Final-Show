using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class XPScript : NetworkBehaviour
{
    
    private GameplayManager gameplayManager;
    
    /// <summary>
    /// Initializes the script object with the GameplayManager
    /// </summary>
    void Start()
    {
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
        // TODO: Make it spin or something cool
    }

    private void OnTriggerEnter(Collider collider)
    {
        // If the script collides with a player
        if(collider.gameObject.tag == "Player")
        {
            // Increment player's script scoreboard count (future maybe)
            // Increment total scripts in gameplayManager
            //gameplayManager.CollectScript();
            Debug.Log("Script Collected");
            // Destroy object
            Destroy(gameObject);
        }
    }
}
