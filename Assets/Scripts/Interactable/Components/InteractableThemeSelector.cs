using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Allows a player to select a theme manually for demo purposes
/// </summary>
public class InteractableThemeSelector : NetworkBehaviour, IInteractable
{
    [SerializeField] private int _themeIndex;
    private GameplayManager _gameplayManager;

    private void Start()
    {
        _gameplayManager = GameplayManager.Instance;
    }

    public void Interact(GameObject playerObject)
    {
        if (!isServer) { return; }
        _gameplayManager.SelectTheme(_themeIndex);
    }
}