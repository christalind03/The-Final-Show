using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Manuscript : NetworkBehaviour
{
    
    private GameplayManager _gameplayManager;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _launchSpeed = 1f;
    [Range(0f, 90f)]
    [SerializeField]
    [Tooltip("The angle measured in degress upwards from the horizontal")]
    private float _launchAngle = 45f;
    
    private Rigidbody rb;

    /// <summary>
    /// Initializes the script object with the GameplayManager and adds a velocity to launch the manuscript outward
    /// </summary>
    void Start()
    {
        if (!isServer) { return; }
        // Get the GameplayManager
        _gameplayManager = GameplayManager.Instance;
        if ( _gameplayManager == null )
        {
            Debug.Log("Gameplay Manager not found");
        }
        // Apply a velocity change forwards and up according to launch angle
        rb = GetComponent<Rigidbody>();
        Vector3 launchDir = Quaternion.AngleAxis(_launchAngle, -transform.right) * transform.forward;
        rb.AddForce(_launchSpeed * launchDir, ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) { return; }
        // Rotate the script every frame because it looks cool
        transform.Rotate(0, Time.deltaTime*_rotationSpeed, 0);
    }

    /// <summary>
    /// Used to detect when a player collects the script
    /// </summary>
    private void OnTriggerEnter(Collider collider)
    {
        if (!isServer) { return; }
        // If the script collides with a player
        if (collider.gameObject.tag == "Player")
        {
            // TODO: Increment player's script scoreboard count (future maybe)
            // Increment total scripts in gameplayManager
            if (_gameplayManager != null)
            {
                _gameplayManager.CollectScript();
            }
            // Destroy object
            Destroy(gameObject);
        }
    }
}
