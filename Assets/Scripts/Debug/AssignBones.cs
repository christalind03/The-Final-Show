using UnityEngine;

/// <summary>
/// Assigns bone transforms from a reference SkinnedMeshRenderer to the SkinnedMeshRenderer on this GameObject.
/// </summary>
public class AssignBones : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _sampleBones;

    /// <summary>
    /// Automatically assigns the bones from the reference SkinnedMeshRenderer to this GameObject's SkinnedMeshRenderer
    /// whenever a value is changed in the inspector.
    /// </summary>
    private void OnValidate()
    {
        if (_sampleBones != null)
        {
            SkinnedMeshRenderer thisRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

            thisRenderer.bones = _sampleBones.bones;
        }
    }
}
