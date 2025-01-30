using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a stat that can be modified and adjusted dynamically.
/// </summary>
public class Stat
{
    private float _baseValue;
    private float _currentValue;
    private float _modifierSum;
    private List<float> _modifierList;

    public event Action<float, float> OnBaseChange;
    public event Action<float, float> OnCurrentChange;

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
            if (_baseValue != value)
            {
                _baseValue = value;
            }
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
            float clampedValue = Mathf.Clamp(value, 0f, BaseValue);

            if (_currentValue != clampedValue)
            {
                float previousValue = _currentValue;
                float currentValue = clampedValue;

                OnCurrentChange?.Invoke(previousValue, currentValue);
                _currentValue = currentValue;
            }
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
    /// Additionally adds the modifier value to the current value to maintain value scaling.
    /// </summary>
    /// <param name="modifierValue"></param>
    public void AddModifier(float modifierValue)
    {
        if (modifierValue != 0f)
        {
            _modifierList.Add(modifierValue);
            SimulateBaseChange();
        }
    }

    /// <summary>
    /// Removes a modifier from the modifier list.
    /// Allows for the removal of temporary adjustments to the base value.
    /// Additionally removes the modifier value from the current value to maintain value scaling.
    /// </summary>
    /// <param name="modifierValue"></param>
    public void RemoveModifier(float modifierValue)
    {
        if (modifierValue != 0f)
        {
            _modifierList.Remove(modifierValue);
            SimulateBaseChange();
        }
    }

    /// <summary>
    /// Simulates a change in the base value by summing the base value and modifiers and comparing it to the previous combined value.
    /// If the simulated base value has changed, it invokes the <see cref="OnBaseChange"/> event with the previous and current values.
    /// </summary>
    private void SimulateBaseChange()
    {
        float previousValue = _baseValue + _modifierSum;
        float currentValue = _baseValue;

        // Prevent null error messages in the console if the object was set without a constructor.
        _modifierList.ForEach(modifierValue => currentValue += modifierValue);

        if (previousValue != currentValue)
        {
            OnBaseChange?.Invoke(previousValue, currentValue);
            _modifierSum = currentValue;
        }
    }
}
