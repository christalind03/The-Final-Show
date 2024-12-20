using Mirror;
using System.Collections.Generic;
using UnityEngine;

// TODO: Redocument entire file
/// <summary>
/// Represents a stat that can be modified and adjusted dynamically.
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
    /// <param name="modifierValue"></param>
    [Command(requiresAuthority = false)]
    public void CmdAddModifier(float modifierValue)
    {
        if (modifierValue != 0)
        {
            _modifierList.Add(modifierValue);
            _currentValue += modifierValue;
        }
    }

    /// <summary>
    /// Removes a modifier from the modifier list.
    /// Allows for the removal of temporary adjustments to the base value.
    /// </summary>
    /// <param name="modifierValue"></param>
    [Command(requiresAuthority = false)]
    public void CmdRemoveModifier(float modifierValue)
    {
        if (modifierValue != 0f)
        {
            _modifierList.Remove(modifierValue);
            _currentValue -= modifierValue;
        }
    }

    // TODO: Document
    private void OnHealthChange(float oldHealth, float newHealth)
    {
        // We can add in our visual health updates like health bars here
        Debug.Log($"{gameObject.name} health changed: {oldHealth}/{_baseValue} -> {newHealth}/{_baseValue}");
    }

    // TODO: Document
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

    // TODO: Document
    [ClientRpc]
    private void RpcOnDeath()
    {
        if (!isLocalPlayer || isServer) { return; }
        if (gameObject.CompareTag("Player")) { Spectate(); }

        Debug.Log($"{gameObject.name} death broadcasted to all clients.");
    }

    // TODO: Document
    private void Spectate()
    {
        _playerBody.layer = 0;
        CameraController cameraController = gameObject.GetComponent<CameraController>();
        cameraController.alive = false;
        cameraController.Spectate();
    }
}
