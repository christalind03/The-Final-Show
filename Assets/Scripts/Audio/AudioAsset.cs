using UnityEngine;

/// <summary>
/// Represents an audio asset with its associated properties.
/// </summary>
[System.Serializable]
public class AudioAsset
{
    public bool Is3D;
    public string Name;
    public AudioResource Resource;
    [HideInInspector] public AudioSource AudioSource;
}
