using UnityEngine;

/// <summary>
/// Manages the gameplay context to be shared across the gameplay state machine and respective states.
/// </summary>
public class GameplayContext
{
    public GameplayTheme GameplayTheme { get; private set; }

    private readonly GameplayTheme[] _gameplayThemes;

    public int _scriptsCollected; // Stores the number of scripts collected in the current run
    public int _lifetimeScriptsCollected; // Stores the total number of scripts collected across all runs

    /// <summary>
    /// Initializes a new instance of the <see cref="GameplayContext"/> class.
    /// </summary>
    /// <param name="gameplayThemes">An array of available gameplay themes.</param>
    public GameplayContext(GameplayTheme[] gameplayThemes)
    {
        _gameplayThemes = gameplayThemes;
        _scriptsCollected = 0;
        _lifetimeScriptsCollected = 0;
    }

    /// <summary>
    /// Randomizes the gameplay theme by selecting one at random from te available themes.
    /// </summary>
    public void RandomizeTheme()
    {
        int themeIndex = Random.Range(0, _gameplayThemes.Length);
        GameplayTheme = _gameplayThemes[themeIndex];
    }

    /// <summary>
    /// Increments the number of scripts collected (current and lifetime)
    /// </summary>
    public void CollectScript()
    {
        _scriptsCollected++;
        _lifetimeScriptsCollected++;
        Debug.Log("Current Scripts: " + _scriptsCollected);
        Debug.Log("Lifetime Scripts: " + _lifetimeScriptsCollected);
    }
}
