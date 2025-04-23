using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the gameplay context to be shared across the gameplay state machine and respective states.
/// </summary>
public class GameplayContext
{
    public GameplayTheme GameplayTheme { get; private set; }

    public readonly GameplayTheme[] _gameplayThemes;
    public List<GameObject> invalidPlayers = new List<GameObject>();

    public int scriptsCollected; // Stores the number of scripts collected in the current run
    public int lifetimeScriptsCollected; // Stores the total number of scripts collected across all runs
    public int scriptsNeeded = 1; // This would change depending on difficulty, using predefined value for testing

    /// <summary>
    /// Initializes a new instance of the <see cref="GameplayContext"/> class.
    /// </summary>
    /// <param name="gameplayThemes">An array of available gameplay themes.</param>
    public GameplayContext(GameplayTheme[] gameplayThemes)
    {
        _gameplayThemes = gameplayThemes;
        scriptsCollected = 0;
        lifetimeScriptsCollected = 0;
    }

    /// <summary>
    /// Randomizes the gameplay theme by selecting one at random from the available themes.
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
        scriptsCollected++;
        lifetimeScriptsCollected++;
    }
}
