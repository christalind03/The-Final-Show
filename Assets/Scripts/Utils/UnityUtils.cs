using System;
using System.Collections;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A static utility class containing helper methods for Unity related tasks.
/// </summary>
public static class UnityUtils
{
    /// <summary>
    /// Calculates the combined bounds that encompass all the given individual bounds.
    /// This method finds the center of all the boundaries and encapsulates each boundary into a single bounding box.
    /// https://stackoverflow.com/a/11960936/18086470
    /// </summary>
    /// <param name="allBoundaries">An array of <see cref="Bounds"/> objects to be combined into a single bound</param>
    /// <returns>A <see cref="Bounds"/> object representing the smallest bounding box that contains all the provided boundaries</returns>
    public static Bounds CalculateBounds(Bounds[] allBoundaries)
    {
        // Calculate the center of the bounding region
        Vector3 objectCenter = Vector3.zero;

        foreach (Bounds currentBoundaries in allBoundaries)
        {
            objectCenter += currentBoundaries.center;
        }

        objectCenter /= allBoundaries.Length;

        // Calculate the boundaries of the object
        Bounds objectBounds = new Bounds(objectCenter, Vector3.zero);

        foreach (Bounds currentBoundaries in allBoundaries)
        {
            objectBounds.Encapsulate(currentBoundaries);
        }

        return objectBounds;
    }

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
    /// Attempts to find and retrieve a specific UI element of type <typeparamref name="TElement"/> within the provided <paramref name="rootVisualElement"/>
    /// using the specified <paramref name="targetElement"/> selector.
    /// </summary>
    /// <typeparam name="TElement">The type of UI element to search for</typeparam>
    /// <param name="rootVisualElement">The root visual element that serves as the search container</param>
    /// <param name="targetElement">The name or class of the target element to find</param>
    /// <param name="uiElement">An output parameter that will contain the found UI element, if successful</param>
    /// <returns>Returns <c>true</c> if hte element is foudn; otherwise <c>false</c>.</returns>
    public static bool ContainsElement<TElement>(VisualElement rootVisualElement, string targetElement, out TElement uiElement) where TElement : VisualElement
    {
        if (rootVisualElement == null)
        {
            LogError($"The provided rootVisualElement is null", 3);
            uiElement = null;
            return false;
        }
        else
        {
            uiElement = rootVisualElement.Query<TElement>(targetElement.Trim());

            if (uiElement == null)
            {
                LogError($"Unable to locate {typeof(TElement)} titled '{targetElement}'", 3);
                return false;
            }
            else
            {
                return true;
            }
        }

    }

    /// <summary>
    /// Logs an error message to the Unity console, prefixed with the script's name.
    /// </summary>
    /// <param name="frameIndex">The index of the frame in the call stack to fetch the script name from.</param>
    /// <param name="errorMessage">The error message to log.</param>
    public static void LogError(string errorMessage, int frameIndex = 1)
    {
        string scriptName = FetchScriptName(frameIndex);

        UnityEngine.Debug.LogError($"[{scriptName}] {errorMessage}");
    }

    /// <summary>
    /// Logs a warning message to the Unity console, prefixed with the script's name.
    /// </summary>
    /// <param name="frameIndex">The index of the frame in the call stack to fetch the script name from.</param>
    /// <param name="warningMessage">The warning message to log.</param>
    public static void LogWarning(string warningMessage, int frameIndex = 1)
    {
        string scriptName = FetchScriptName(frameIndex);

        UnityEngine.Debug.LogWarning($"[{scriptName}] {warningMessage}");
    }

    /// <summary>
    /// Generates a random position within the provided <see cref="Bounds"/>.
    /// The position is chosen by randomly selecting x, y, and z coordinates within the minimum and maximum values of the bounding box.
    /// </summary>
    /// <param name="objectBounds">The <see cref="Bounds"/> representing the region within which the position will be randomized</param>
    /// <returns>A <see cref="Vector3"/> representing a random position within the specified bounds.</returns>
    public static Vector3 RandomizePosition(Bounds objectBounds)
    {
        float x = UnityEngine.Random.Range(objectBounds.min.x, objectBounds.max.x);
        float y = UnityEngine.Random.Range(objectBounds.min.y, objectBounds.max.y);
        float z = UnityEngine.Random.Range(objectBounds.min.z, objectBounds.max.z);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Waits for the first instance of <typeparamref name="TComponent"/> to be found within the scene,
    /// within the provided <paramref name="timeLimit"/>. Once the component is found, the specified callback is invoked.
    /// </summary>
    /// <typeparam name="TComponent">The type of component to search the scene for</typeparam>
    /// <param name="timeLimit">The maximum time (in seconds) to wait before aborting the search</param>
    /// <param name="onFound">The callback that is invoked once the component is found</param>
    /// <returns>An <c>IEnumerator</c> for coroutine execution</returns>
    public static IEnumerator WaitForObject<TComponent>(float timeLimit, Action<TComponent> onFound) where TComponent : Component
    {
        float startTime = Time.time;
        TComponent targetObject = null;

        yield return new WaitUntil(() =>
        {
            if ((targetObject = GameObject.FindFirstObjectByType<TComponent>()) != null)
            {
                onFound.Invoke(targetObject);
                return true;
            }

            return timeLimit <= Time.time - startTime;
        });

    }

    /// <summary>
    /// Retrieves the name of the current script.
    /// </summary>
    /// <param name="frameIndex">The index of the frame in the call stack to fetch the script name from.</param>
    /// <returns>The name of the script calling the current method.</returns>
    private static string FetchScriptName(int frameIndex)
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame stackFrame = stackTrace.GetFrame(frameIndex);
        string scriptName = stackFrame.GetMethod().DeclaringType.Name;

        return scriptName;
    }
}
