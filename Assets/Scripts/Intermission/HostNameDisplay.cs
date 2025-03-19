using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HostNameDisplay : NetworkBehaviour
{
    [SerializeField] private Text _hostText;

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameObject Host = NetworkServer.localConnection.identity?.gameObject;
        if(Host != null)
        {
            _hostText.text = "STARRING " + Host.name;
        }
    }
}
