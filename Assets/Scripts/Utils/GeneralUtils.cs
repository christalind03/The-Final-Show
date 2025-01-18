using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// A static utility class containing general helper methods.
/// </summary>
public static class GeneralUtils
{
    public const float DefaultTimeout = 0.25f;
    public static readonly BindingFlags ReflectionFlags =
        BindingFlags.FlattenHierarchy |
        BindingFlags.Instance |
        BindingFlags.NonPublic |
        BindingFlags.Public |
        BindingFlags.Static;

    /// <summary>
    /// Clones the field data from one object to another by copying the values of all fields.
    /// This method works for public and private fields, based on the provided reflection flags.
    /// </summary>
    /// <typeparam name="TObject">The type of the source and target objects</typeparam>
    /// <param name="sourceObject">The object to copy field values from</param>
    /// <param name="targetObject">The object to copy field values to</param>
    public static void CloneFieldData<TObject>(TObject sourceObject, TObject targetObject)
    {
        foreach (FieldInfo currentField in sourceObject.GetType().GetFields(ReflectionFlags))
        {
            object sourceField = currentField.GetValue(sourceObject);

            currentField.SetValue(targetObject, sourceField);
        }
    }

    /// <summary>
    /// Clones the property data from one object to another by copying the values of all properties.
    /// This method works for public and private properties, based on the provided reflection flags.
    /// </summary>
    /// <typeparam name="TObject">The type of the source and target objects</typeparam>
    /// <param name="sourceObject">The object to copy property values from</param>
    /// <param name="targetObject">The object to copy property values to</param>
    public static void ClonePropertyData<TObject>(TObject sourceObject, TObject targetObject)
    {
        foreach (PropertyInfo currentProperty in sourceObject.GetType().GetProperties(ReflectionFlags))
        {
            object sourceProperty = currentProperty.GetValue(sourceObject);

            currentProperty.SetValue(targetObject, sourceProperty);
        }
    }

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
