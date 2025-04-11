using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HostNameDisplay : NetworkBehaviour
{
    [SerializeField] private Text _hostText;
    [SyncVar] private string hostName;


    /// <summary>
    /// Starts the display function with a delay
    /// </summary>
    public override void OnStartClient()
    {
        StartCoroutine(Display());
        base.OnStartClient();
    }

    /// <summary>
    /// Displays the host name on the "Starring" text in intermission
    /// </summary>
    /// <returns></returns>
    private IEnumerator Display()
    {
        yield return new WaitForEndOfFrame();

        if (isServer)
        {
            GameObject Host = NetworkServer.localConnection.identity?.gameObject;
            if (Host != null)
            {
                hostName = Host.name;
                _hostText.text = "STARRING " + hostName;
            }
        }
        else
        {
            if(hostName != null)
            {
                _hostText.text = "STARRING " + hostName;
            }
        }
    }
}
