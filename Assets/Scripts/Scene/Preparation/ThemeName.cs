using Mirror;

public class ThemeName : NetworkBehaviour
{
    [SyncVar] public string theme;

    /// <summary>
    /// Sets the string theme of this class to the theme in GamePlayManager
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        GameplayManager manager = NetworkManager.FindObjectOfType<GameplayManager>();
        theme = manager.GetTheme();                        
    }
}
