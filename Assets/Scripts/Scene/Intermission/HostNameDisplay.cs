using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HostNameDisplay : NetworkBehaviour
{
    [SerializeField] private Text _hostText;
    [SyncVar] private string hostName;


    /// <summary>
    /// Displays the host name on the "Starring" text in intermission
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

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
