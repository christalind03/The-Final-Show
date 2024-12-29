using System.Diagnostics;
using UnityEngine;

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
    public static void LogError(string errorMessage)
    {
        string scriptName = FetchScriptName();

        UnityEngine.Debug.LogError($"[{scriptName}] {errorMessage}");
    }

    // TODO: Document
    public static void LogWarning(string warningMessage)
    {
        string scriptName = FetchScriptName();

        UnityEngine.Debug.LogWarning($"[{scriptName}] {warningMessage}");
    }

    // TODO: Document
    private static string FetchScriptName()
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame stackFrame = stackTrace.GetFrame(1);
        string scriptName = stackFrame.GetMethod().DeclaringType.Name;

        return scriptName;
    }
}
