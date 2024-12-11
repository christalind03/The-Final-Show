using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Represents a stat that can be modified and adjusted dynamically.
/// </summary>
[System.Serializable]
public class Stat
{
    [SerializeField] private float _baseValue;
    [SerializeField] private float _currentValue;

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
    public Stat(float baseValue)
    {
        _baseValue = baseValue;
        _currentValue = baseValue;
        _modifierList = new List<float>();
    }

    /// <summary>
    /// Increase the current value of the stat by the specified amount.
    /// </summary>
    /// <param name="increaseValue">The amount to increase the current value by.</param>
    public void Increase(float increaseValue)
    {
        CurrentValue += increaseValue;
    }

    /// <summary>
    /// Decreases the current value of the stat by the specified amount.
    /// </summary>
    /// <param name="decreaseValue">The amount to decrease the current value by.</param>
    public void Decrease(float decreaseValue)
    {
        CurrentValue -= decreaseValue;
    }

    /// <summary>
    /// Adds a modifier to the modifier list.
    /// Allows for temporary adjustments to the base value.
    /// </summary>
    /// <param name="modifierValue"></param>
    public void AddModifier(float modifierValue)
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
    /// <param name="modifierValue"></param>
    public void RemoveModifier(float modifierValue)
    {
        if (modifierValue != 0f)
        {
            _modifierList.Remove(modifierValue);
        }
    }
}
