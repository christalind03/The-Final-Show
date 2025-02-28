using Mirror;
using System;
using UnityEngine;

/// <summary>
/// Manages audio playback for networked entities, allowing 2D and 3D audio sources to be dynamically
/// initialized, changed, and synchronized across clients using Mirror.
/// </summary>
public class AudioManager : NetworkBehaviour
{
    [SerializeField] private AudioAsset[] audioAssets;

    /// <summary>
    /// Initializes audio assets by creating AudioSource components and assigning its associated resource, if available.
    /// </summary>
    private void Awake()
    {
        if (audioAssets != null)
        {
            foreach (AudioAsset audioAsset in audioAssets)
            {
                audioAsset.AudioSource = CreateAudioSource(audioAsset.Is3D);
            
                if (audioAsset.Resource != null)
                {
                    InitializeAudio(audioAsset);
                }
            }
        }
    }

    /// <summary>
    /// Creates and applies configurations for the <see cref="AudioSource"/> component.
    /// </summary>
    /// <param name="is3D">Whether or not the audio should be spatialized.</param>
    /// <returns>An AudioSource component.</returns>
    private AudioSource CreateAudioSource(bool is3D)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();

        if (is3D)
        {
            audioSource.spatialize = true;
            audioSource.spatialBlend = 1.0f; // Enables 3D audio

            audioSource.minDistance = 5;
            audioSource.maxDistance = 15;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }

        return audioSource;
    }

    /// <summary>
    /// Initializes an <see cref="AudioAsset"/> by assigning its <see cref="AudioSource"/> properties from <see cref="AudioResource"/>.
    /// </summary>
    /// <param name="audioAsset">The audio asset to initialize.</param>
    private void InitializeAudio(AudioAsset audioAsset)
    {
        audioAsset.AudioSource.clip = audioAsset.Resource.AudioClip;
        audioAsset.AudioSource.volume = audioAsset.Resource.Volume;
        audioAsset.AudioSource.pitch = audioAsset.Resource.Pitch;
        audioAsset.AudioSource.loop = audioAsset.Resource.Loop;
    }   

    /// <summary>
    /// Updates the audio resource for a specified audio asset and reinitializes it.
    /// </summary>
    /// <param name="targetSource">The name of the audio asset to update.</param>
    /// <param name="audioResource">The <see cref="AudioResource"/> to assign.</param>
    public void ChangeAudio(string targetSource, AudioResource audioResource)
    {
        AudioAsset targetAsset = Array.Find(audioAssets, audioAsset => audioAsset.Name == targetSource);
        targetAsset.Resource = audioResource;
        InitializeAudio(targetAsset);
    }

    /// <summary>
    /// Commands the server to play an <see cref="AudioAsset"/> across all clients.
    /// </summary>
    /// <param name="audioName">The name of the <see cref="AudioAsset"/> to play.</param>
    [Command(requiresAuthority = false)]
    public void CmdPlay(string audioName)
    {
        RpcPlay(audioName);
    }

    /// <summary>
    /// Commands the server to stop all currently playing audio assets across all clients.
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdStop()
    {
        foreach (AudioAsset audioAsset in audioAssets)
        {
            RpcStop(audioAsset.Name);
        }
    }

    /// <summary>
    /// Commands the server to stop a specific <see cref="AudioAsset"/> acriss all clients.
    /// </summary>
    /// <param name="audioName">The name of the <see cref="AudioAsset"/> to stop.</param>
    [Command(requiresAuthority = false)]
    public void CmdStop(string audioName)
    {
        RpcStop(audioName);
    }

    /// <summary>
    /// Instructs all clients to play a specified <see cref="AudioAsset"/>.
    /// </summary>
    /// <param name="audioName">The name of the <see cref="AudioAsset"/> to play.</param>
    [ClientRpc]
    protected void RpcPlay(string audioName)
    {
        AudioAsset audioAsset = Array.Find(audioAssets, audioAsset => audioAsset.Name == audioName);

        if (audioAsset == null)
        {
            UnityUtils.LogWarning($"Unable to locate audio asset '{audioName}' on {gameObject.name}.");
            return;
        }

        if (audioAsset.AudioSource.isPlaying == false)
        {
            audioAsset.AudioSource.Play();
        }
    }

    /// <summary>
    /// Instructs all clients to stop a specified <see cref="AudioAsset"/>.
    /// </summary>
    /// <param name="audioName">The name of the <see cref="AudioAsset"/> to stop.</param>
    [ClientRpc]
    protected void RpcStop(string audioName)
    {
        AudioAsset audioAsset = Array.Find(audioAssets, audioAsset => audioAsset.Name == audioName);

        if (audioAsset == null)
        {
            UnityUtils.LogWarning($"Unable to locate audio asset '{audioName}' on {gameObject.name}.");
            return;
        }

        audioAsset.AudioSource.Stop();
    }
}
