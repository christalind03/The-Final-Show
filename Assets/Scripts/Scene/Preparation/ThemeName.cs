using Mirror;
using UnityEngine;

public class ThemeName : NetworkBehaviour
{
    [SyncVar] public string theme;

    public override void OnStartServer()
    {
        base.OnStartServer();
        GameplayManager manager = NetworkManager.FindObjectOfType<GameplayManager>();
        theme = manager.GetTheme();                        
    }
}
