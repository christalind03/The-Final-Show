using Mirror;
using UnityEngine;

public class PhysicsSimulator : MonoBehaviour
{
    private PhysicsScene _physicsScene;
    private bool _simulatePhysicsScene;

    // TODO: Document
    private void Awake()
    {
        if (NetworkServer.active)
        {
            _physicsScene = gameObject.scene.GetPhysicsScene();
            _simulatePhysicsScene = _physicsScene.IsValid() && _physicsScene != Physics.defaultPhysicsScene;
        }
        else
        {
            enabled = false;
        }
    }

    // TODO: Document
    [ServerCallback]
    private void FixedUpdate()
    {
        if (_simulatePhysicsScene)
        {
            _physicsScene.Simulate(Time.fixedDeltaTime);
        }
    }
}
