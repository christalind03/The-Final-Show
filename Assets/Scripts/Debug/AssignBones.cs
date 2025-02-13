using UnityEngine;

public class AssignBones : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _sampleBones;

    // TODO: Document
    private void OnValidate()
    {
        if (_sampleBones != null)
        {
            SkinnedMeshRenderer thisRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

            thisRenderer.bones = _sampleBones.bones;
        }
    }
}
