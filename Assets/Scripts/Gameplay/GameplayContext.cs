public class GameplayContext
{
    public readonly CustomNetworkManager CustomNetworkManager;
    public readonly GameplayManager GameplayManager;

    public GameplayContext(CustomNetworkManager customNetworkManager, GameplayManager gameplayManager)
    {
        CustomNetworkManager = customNetworkManager;
        GameplayManager = gameplayManager;
    }
}
