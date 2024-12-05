using UnityEngine;

public class ChangeStatus : MonoBehaviour
{

    private CameraController ccontroller; 

    /// <summary>
    /// When collision happens, change the alive status to false and call spectate mode 
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.name != "Floor"){
            ccontroller = other.gameObject.GetComponentInParent<CameraController>();
            ccontroller.alive = false;
            ccontroller.Spectate();
        }
    }
}
