using Mirror;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract, base class representing a health system for game entities.
/// </summary>
public abstract class AbstractHealth : NetworkBehaviour
{
    [SerializeField]
    [SyncVar(hook = nameof(OnBaseHealth))]
    protected float _baseValue;

    [SyncVar(hook = nameof(OnCurrentHealth))]
    protected float _currentValue;

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
        if (TryGetComponent(out PlayerStats playerStats))
        {
            float reducedDamage = Mathf.Max(decreaseValue - playerStats.Defense.BaseValue, 0);
            CurrentValue -= reducedDamage;

            Debug.Log($"{gameObject.name} took {reducedDamage} damage after defense. Remaining health: {CurrentValue}/{BaseValue}");

            if (CurrentValue <= 0f) { TriggerDeath(); }
        }
        else
        {
            CurrentValue -= decreaseValue;
        }
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
    /// Reset the health of the target to base health
    /// </summary>
    /// <param name="target">target</param>
    [TargetRpc]
    public void TargetResetHealth(NetworkConnectionToClient target)
    {
        _currentValue = _baseValue;
    }

    /// <summary>
    /// Called on all clients when the base health value changes.
    /// This is performed with the <c>SyncVar</c> hook attached to <c>_currentValue</c>.
    /// </summary>
    /// <param name="previousValue">The previous base health value.</param>
    /// <param name="currentValue">The current base health value.</param>
    protected abstract void OnBaseHealth(float previousValue, float currentValue);

    /// </summary>
    /// Called on all clients when the current health value changes.
    /// This is performed with the <c>SyncVar</c> hook attached to <c>_currentValue</c>.
    /// </summary>
    /// <param name="previousValue">The previous current health value.</param>
    /// <param name="currentValue">The current current health value.</param>
    protected abstract void OnCurrentHealth(float previousValue, float currentValue);
    
    /// <summary>
    /// Handles the death of the object.
    /// </summary>
    protected abstract void TriggerDeath();
}
