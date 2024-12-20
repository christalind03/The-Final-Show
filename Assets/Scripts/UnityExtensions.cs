using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions
{
    // TODO: Documentation
    public static bool ContainsLayer(LayerMask layerMask, int targetLayer)
    {
        return layerMask == (layerMask | (1 << targetLayer));
    }

    // TODO: Documentation
    public static TKey FindKey<TKey, TValue>(Dictionary<TKey, TValue> targetDictionary, TValue targetValue)
    {
        foreach (var dictionaryEntry in targetDictionary)
        {
            if (EqualityComparer<TValue>.Default.Equals(dictionaryEntry.Value, targetValue))
            {
                return dictionaryEntry.Key;
            }
        }

        return default;
    }
}
