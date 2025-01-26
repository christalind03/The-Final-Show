using UnityEngine;

public class GameplayContext
{
    public GameplayTheme GameplayTheme { get; private set; }

    private readonly GameplayTheme[] _gameplayThemes;

    // TODO: Document
    public GameplayContext(GameplayTheme[] gameplayThemes)
    {
        _gameplayThemes = gameplayThemes;
    }

    // TODO: Document
    public void RandomizeTheme()
    {
        int themeIndex = Random.Range(0, _gameplayThemes.Length);
        GameplayTheme = _gameplayThemes[themeIndex];
    }
}
