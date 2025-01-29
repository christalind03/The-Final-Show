// Ensure this is only compiled and ran for the Unity Editor
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates materials using the ToonShader for each unique color found in the materials of the target objects.
/// </summary>
public class ToonifyMaterials : MonoBehaviour
{
    [SerializeField] private GameObject[] targetObjects;

    /// <summary>
    /// Initializ
    /// </summary>
    private void Start()
    {
        GenerateMaterials(LocateUniqueColors());
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Locates all unique colors used in the materials of the target objects.
    /// </summary>
    /// <returns>A list of unique colors found in the materials of the target objects.</returns>
    private List<Color> LocateUniqueColors()
    {
        List<Color> uniqueColors = new List<Color>();

        foreach (GameObject targetObject in targetObjects)
        {
            Renderer[] objectRenderers = targetObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer objectRenderer in objectRenderers)
            {
                foreach (Material objectMaterial in objectRenderer.sharedMaterials)
                {
                    if (objectMaterial == null) { continue; }
                    if (!uniqueColors.Contains(objectMaterial.color))
                    {
                        uniqueColors.Add(objectMaterial.color);
                    }
                }
            }
        }

        return uniqueColors;
    }

    /// <summary>
    /// Generates materials using the ToonShader for each unique color found in the target objects' materials.
    /// </summary>
    private void GenerateMaterials(List<Color> uniqueColors)
    {
        foreach (Color uniqueColor in uniqueColors)
        {
            Material thisMaterial = new Material(Shader.Find("Shader Graphs/ToonShader"));

            thisMaterial.SetColor("_Color", uniqueColor);

            AssetDatabase.CreateAsset(thisMaterial, Path.Combine("Assets\\Materials", $"#{RGB2HEX(uniqueColor)}.mat"));
        }
    }

    /// <summary>
    /// Converts a <see cref="Color"/> object into a HEX string.
    /// </summary>
    /// <param name="targetColor">The color to convert into a HEX string.</param>
    /// <returns>A string representing the color in HEX format.</returns>
    private string RGB2HEX(Color targetColor)
    {
        return ColorUtility.ToHtmlStringRGB(targetColor);
    }
}
#endif