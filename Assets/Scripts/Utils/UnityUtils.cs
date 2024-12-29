using System.Diagnostics;
using UnityEngine;

/// <summary>
/// A static utility class containing helper methods for Unity related tasks.
/// </summary>
public static class UnityUtils
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
    /// Logs an error message to the Unity console, prefixed with the script's name.
    /// </summary>
    /// <param name="errorMessage">The error message to log.</param>
    public static void LogError(string errorMessage)
    {
        string scriptName = FetchScriptName();

        UnityEngine.Debug.LogError($"[{scriptName}] {errorMessage}");
    }

    /// <summary>
    /// Logs a warning message to the Unity console, prefixed with the script's name.
    /// </summary>
    /// <param name="warningMessage">The warning message to log.</param>
    public static void LogWarning(string warningMessage)
    {
        string scriptName = FetchScriptName();

        UnityEngine.Debug.LogWarning($"[{scriptName}] {warningMessage}");
    }

    /// <summary>
    /// Retrieves the name of the current script.
    /// </summary>
    /// <returns>The name of the script calling the current method.</returns>
    private static string FetchScriptName()
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame stackFrame = stackTrace.GetFrame(1);
        string scriptName = stackFrame.GetMethod().DeclaringType.Name;

        return scriptName;
    }
}
