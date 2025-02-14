using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Resource", menuName = "Audio Resource")]
public class AudioResource : ScriptableObject
{
    public AudioClip AudioClip;

    [Range(0f, 1f)] public float Volume = 0.5f;
    [Range(0.1f, 3f)] public float Pitch = 1f;
    public bool Loop;

    [HideInInspector] public AudioSource AudioSource;
}
