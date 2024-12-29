using System.Collections.Generic;

/// <summary>
/// A static utility class containing general helper methods.
/// </summary>
public static class GeneralUtils
{
    /// <summary>
    /// Locates and returns the first key in a dictionary that corresponds to the given value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="targetDictionary">The dictionary to search through.</param>
    /// <param name="targetValue">The value to find the associated key for.</param>
    /// <returns>
    /// The key associated with the specific value, or the default value of <paramref name="targetDictionary"/> if the value is not found.
    /// </returns>
    public static TKey LocateDictionaryKey<TKey, TValue>(Dictionary<TKey, TValue> targetDictionary, TValue targetValue)
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
