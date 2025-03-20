using Mirror;

public class ScriptManagement : NetworkBehaviour
{
    [SyncVar] public int scriptsNeeded;
    [SyncVar] public int currentScript;
    
    /// <summary>
    /// Runs when server starts to initialize necessary info
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        GameplayManager manager = NetworkManager.FindObjectOfType<GameplayManager>();
        scriptsNeeded = manager.GetScriptsNeeded();
    }

    /// <summary>
    /// Updates the collect script task UI
    /// </summary>
    public void UpdateMessage()
    {
        foreach(NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            PlayerInterface playerInterface = conn.identity.GetComponent<PlayerInterface>();
            if(currentScript >= scriptsNeeded)
            {
                playerInterface.RpcRefreshScriptCount("Completed Required Scripts Collection");
            }
            else
            {
                playerInterface.RpcRefreshScriptCount(GetInfo());
            }
        }
    }

    /// <summary>
    /// Constructs and returns the information string
    /// </summary>
    /// <returns>string text to be displayed</returns>
    private string GetInfo()
    {
        return currentScript + "/" + scriptsNeeded + " Scripts";
    }
}
