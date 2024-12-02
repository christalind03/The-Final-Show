using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeStatus : MonoBehaviour
{

    private CameraController ccontroller; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.name != "Floor"){
            ccontroller = other.gameObject.GetComponentInParent<CameraController>();
            ccontroller.alive = false;
            ccontroller.Spectate();
            Debug.Log(other.gameObject.name);
        }
    }
}
