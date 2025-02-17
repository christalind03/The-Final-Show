using Mirror;
using System;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    [System.Serializable]
    public class AudioAsset
    {
        public bool Is3D;
        public string Name;
        public AudioResource Resource;
        [HideInInspector] public AudioSource AudioSource;
    }

    public AudioAsset[] AudioAssets;

    // TODO: Document
    private void Awake()
    {
        foreach (AudioAsset audioAsset in AudioAssets)
        {
            audioAsset.AudioSource = CreateAudioSource(audioAsset.Is3D);
            
            if (audioAsset.Resource != null)
            {
                InitializeAudio(audioAsset);
            }
        }
    }

    // TODO: Document
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

    // TODO: Document
    private void InitializeAudio(AudioAsset audioAsset)
    {
        audioAsset.AudioSource.clip = audioAsset.Resource.AudioClip;
        audioAsset.AudioSource.volume = audioAsset.Resource.Volume;
        audioAsset.AudioSource.pitch = audioAsset.Resource.Pitch;
        audioAsset.AudioSource.loop = audioAsset.Resource.Loop;
    }   

    // TODO: Document
    public void ChangeAudio(string targetSource, AudioResource audioResource)
    {
        AudioAsset targetAsset = Array.Find(AudioAssets, audioAsset => audioAsset.Name == targetSource);
        targetAsset.Resource = audioResource;
        InitializeAudio(targetAsset);
    }

    // TODO: Document
    [Command(requiresAuthority = false)]
    public void CmdPlay(string audioName)
    {
        RpcPlay(audioName);
    }

    // TODO: Document
    [Command(requiresAuthority = false)]
    public void CmdStop()
    {
        foreach (AudioAsset audioAsset in AudioAssets)
        {
            RpcStop(audioAsset.Name);
        }
    }

    // TODO: Document
    [Command(requiresAuthority = false)]
    public void CmdStop(string audioName)
    {
        RpcStop(audioName);
    }

    // TODO: Document
    [ClientRpc]
    public void RpcPlay(string audioName)
    {
        AudioAsset audioAsset = Array.Find(AudioAssets, audioAsset => audioAsset.Name == audioName);

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

    // TODO: Document
    [ClientRpc]
    public void RpcStop(string audioName)
    {
        AudioAsset audioAsset = Array.Find(AudioAssets, audioAsset => audioAsset.Name == audioName);

        if (audioAsset == null)
        {
            UnityUtils.LogWarning($"Unable to locate audio asset '{audioName}' on {gameObject.name}.");
            return;
        }

        audioAsset.AudioSource.Stop();
    }
}
