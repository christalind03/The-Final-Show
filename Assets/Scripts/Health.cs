using Mirror;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the health system for an entity in the game.
/// Handles incoming damage/healing and death behavior.
/// </summary>
public class Health : NetworkBehaviour
{
    [SerializeField] private float _baseValue;

    [Header("Player Properties")]
    [SerializeField] private GameObject _playerBody;

    [SyncVar(hook = nameof(OnHealthChange))]
    private float _currentValue;
    private List<float> _modifierList;

    /// <summary>
    /// Get the base value of the stat, including any active modifiers into the final calculation.
    /// </summary>
    public float BaseValue
    {
        get
        {
            float modifierSum = 0;

            // Prevent null error messages in the console if the object was set without a constructor. 
            _modifierList ??= new List<float>();
            _modifierList.ForEach(modifierValue => modifierSum += modifierValue);

            return _baseValue + modifierSum;
        }
        private set
        {
            _baseValue = value;
        }
    }

    /// <summary>
    /// Gets the current value of the stat, clamped between zero and the BaseValue (including modifiers).
    /// </summary>
    public float CurrentValue
    {
        get => _currentValue;
        private set
        {
            _currentValue = Mathf.Clamp(value, 0f, BaseValue);
        }
    }

    /// <summary>
    /// Initialize a new instance of the Stat class with a specified base value.
    /// </summary>
    /// <param name="baseValue">The initial value of the stat.</param>
    private void Start()
    {
        _currentValue = _baseValue;
        _modifierList = new List<float>();
    }

    /// <summary>
    /// Increase the current value of the stat by the specified amount.
    /// </summary>
    /// <param name="increaseValue">The amount to increase the current value by.</param>
    [Command(requiresAuthority = false)]
    public void CmdHeal(float increaseValue)
    {
        CurrentValue += increaseValue;
    }

    /// <summary>
    /// Decreases the current value of the stat by the specified amount.
    /// If the current value is below zero, then call <c>TriggerDeath()</c>.
    /// </summary>
    /// <param name="decreaseValue">The amount to decrease the current value by.</param>
    [Command(requiresAuthority = false)]
    public void CmdDamage(float decreaseValue)
    {
        CurrentValue -= decreaseValue;

        Debug.Log($"{gameObject.name} took {decreaseValue} damage. Remaining health: {CurrentValue}/{BaseValue}");
        if (CurrentValue <= 0f) { TriggerDeath(); }
    }

    /// <summary>
    /// Adds a modifier to the modifier list.
    /// Allows for temporary adjustments to the base value.
    /// </summary>
    /// <param name="modifierValue">The amount to increase the base value by.</param>
    [Command(requiresAuthority = false)]
    public void CmdAddModifier(float modifierValue)
    {
        if (modifierValue != 0)
        {
            _modifierList.Add(modifierValue);
        }
    }

    /// <summary>
    /// Removes a modifier from the modifier list.
    /// Allows for the removal of temporary adjustments to the base value.
    /// </summary>
    /// <param name="modifierValue">The amount to decrease the base value by.</param>
    [Command(requiresAuthority = false)]
    public void CmdRemoveModifier(float modifierValue)
    {
        if (modifierValue != 0f)
        {
            _modifierList.Remove(modifierValue);
        }
    }

    /// <summary>
    /// Called on all clients when the current health value changes.
    /// This is performed with the <c>SyncVar</c> hook attached to <c>_currentValue</c>.
    /// </summary>
    /// <param name="oldHealth">The previous health value.</param>
    /// <param name="newHealth">The current health value.</param>
    private void OnHealthChange(float oldHealth, float newHealth)
    {
        // We can add in our visual health updates like health bars here
        Debug.Log($"{gameObject.name} health changed: {oldHealth}/{_baseValue} -> {newHealth}/{_baseValue}");
    }

    /// <summary>
    /// Handles the death of an entity.
    /// </summary>
    [Server]
    private void TriggerDeath()
    {
        if (gameObject.CompareTag("Player"))
        {
            Spectate();
        }
        else
        {
            Destroy(gameObject);
        }

        Debug.Log($"{gameObject.name} has died.");
        RpcOnDeath();
    }

    /// <summary>
    /// Handles the player's death and updates clients with appropriate actions.
    /// </summary>
    [ClientRpc]
    private void RpcOnDeath()
    {
        if (!isLocalPlayer || isServer) { return; }
        if (gameObject.CompareTag("Player")) { Spectate(); }

        Debug.Log($"{gameObject.name} death broadcasted to all clients.");
    }

    /// <summary>
    /// Switches the player to spectate mode by modifying the player's layer and camera behavior.
    /// </summary>
    private void Spectate()
    {
        _playerBody.layer = 0;
        CameraController cameraController = gameObject.GetComponent<CameraController>();
        cameraController.alive = false;
        cameraController.Spectate();
    }
}
