using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides extension methods for common operations throughout the project.
/// </summary>
public static class UnityExtensions
{
    /// <summary>
    /// Checks if a specific layer is included within a given <see cref="LayerMask"/>
    /// </summary>
    /// <param name="layerMask">The <see cref="LayerMask"/> to check.</param>
    /// <param name="targetLayer">The target layer to search for.</param>
    /// <returns><c>true</c> if the <paramref name="targetLayer"/> is included in the <paramref name="layerMask"/>; otherwise <c>false</c>.</returns>
    public static bool ContainsLayer(LayerMask layerMask, int targetLayer)
    {
        return layerMask == (layerMask | (1 << targetLayer));
    }

    /// <summary>
    /// Finds the first key in a dictionary that corresponds to the specific value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="targetDictionary">The dictionary to search through.</param>
    /// <param name="targetValue">The value to find the associated key for.</param>
    /// <returns>
    /// The key associated with the specific value, or the default value of <paramref name="targetDictionary"/> if the value is not found.
    /// </returns>
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
