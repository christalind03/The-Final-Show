using System;

/// <summary>
/// Represents a mapping between a state enumeration key and its corresponding state behavior.
/// </summary>
/// <typeparam name="EState">The enumerable type representing the states in the mapping</typeparam>
/// <typeparam name="TStateContext">The context type representing the shared data or dependencies provided to states</typeparam>
[System.Serializable]
public struct StateMapping<EState, TStateContext> where EState : Enum
{
    public EState Key;
    public BaseState<EState, TStateContext> Value;
}
