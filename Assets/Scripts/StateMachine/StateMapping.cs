using System;

/// <summary>
/// Represents a mapping between a state enumeration key and its corresponding state behavior.
/// </summary>
/// <typeparam name="EState">The enumerable type representing the states in the mapping</typeparam>
[System.Serializable]
public struct StateMapping<EState> where EState : Enum
{
    public EState Key;
    public BaseState<EState> Value;
}
