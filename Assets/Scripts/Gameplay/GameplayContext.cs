public class GameplayContext
{
    public readonly GameplayManager GameplayManager;
    public readonly CustomNetworkManager NetworkManager;

    public GameplayContext(GameplayManager gameplayManager, CustomNetworkManager networkManager)
    {
        GameplayManager = gameplayManager;
        NetworkManager = networkManager;
    }
}
