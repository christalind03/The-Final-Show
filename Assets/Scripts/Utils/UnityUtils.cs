using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

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

    // TODO: Document
    public static bool ContainsElement<TElement>(VisualElement rootVisualElement, string targetElement, out TElement uiElement) where TElement : VisualElement
    {
        if (rootVisualElement == null)
        {
            uiElement = null;
            return false;
        }
        else
        {
            uiElement = rootVisualElement.Query<TElement>(targetElement);

            if (uiElement != null)
            {
                return true;
            }
            else
            {
                LogError($"Unable to locate {typeof(TElement)} titled '{targetElement}'", 3);
                return false;
            }
        }

    }

    // TODO: Update Documentation
    /// <summary>
    /// Logs an error message to the Unity console, prefixed with the script's name.
    /// </summary>
    /// <param name="errorMessage">The error message to log.</param>
    public static void LogError(string errorMessage, int frameIndex = 1)
    {
        string scriptName = FetchScriptName(frameIndex);

        UnityEngine.Debug.LogError($"[{scriptName}] {errorMessage}");
    }

    // TODO: Update Documentation
    /// <summary>
    /// Logs a warning message to the Unity console, prefixed with the script's name.
    /// </summary>
    /// <param name="warningMessage">The warning message to log.</param>
    public static void LogWarning(string warningMessage, int frameIndex = 1)
    {
        string scriptName = FetchScriptName(frameIndex);

        UnityEngine.Debug.LogWarning($"[{scriptName}] {warningMessage}");
    }

    // TODO: Update Documentation
    /// <summary>
    /// Retrieves the name of the current script.
    /// </summary>
    /// <returns>The name of the script calling the current method.</returns>
    private static string FetchScriptName(int frameIndex)
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame stackFrame = stackTrace.GetFrame(frameIndex);
        string scriptName = stackFrame.GetMethod().DeclaringType.Name;

        return scriptName;
    }
}
