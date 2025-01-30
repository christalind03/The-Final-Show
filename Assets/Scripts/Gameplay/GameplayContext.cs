using UnityEngine;

/// <summary>
/// Manages the gameplay context to be shared across the gameplay state machine and respective states.
/// </summary>
public class GameplayContext
{
    public GameplayTheme GameplayTheme { get; private set; }

    private readonly GameplayTheme[] _gameplayThemes;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameplayContext"/> class.
    /// </summary>
    /// <param name="gameplayThemes">An array of available gameplay themes.</param>
    public GameplayContext(GameplayTheme[] gameplayThemes)
    {
        _gameplayThemes = gameplayThemes;
    }

    /// <summary>
    /// Randomizes the gameplay theme by selecting one at random from te available themes.
    /// </summary>
    public void RandomizeTheme()
    {
        int themeIndex = Random.Range(0, _gameplayThemes.Length);
        GameplayTheme = _gameplayThemes[themeIndex];
    }
}
