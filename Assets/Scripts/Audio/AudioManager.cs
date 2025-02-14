using Mirror;
using System;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    [System.Serializable]
    private class AudioAsset
    {
        public bool Is3D;
        public string Name;
        public AudioResource Resource;
    }

    [SerializeField] private AudioAsset[] _audioAssets;

    // TODO: Document
    private void Awake()
    {
        foreach (AudioAsset audioAsset in _audioAssets)
        {
            audioAsset.Resource.AudioSource = CreateAudioSource(audioAsset.Is3D);
            audioAsset.Resource.AudioSource.clip = audioAsset.Resource.AudioClip;
            audioAsset.Resource.AudioSource.volume = audioAsset.Resource.Volume;
            audioAsset.Resource.AudioSource.pitch = audioAsset.Resource.Pitch;
            audioAsset.Resource.AudioSource.loop = audioAsset.Resource.Loop;
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
    public void CmdPlay(string audioName)
    {
        RpcPlay(audioName);
    }

    // TODO: Document
    public void CmdStop()
    {
        RpcStop();
    }

    // TODO: Document
    public void CmdStop(string audioName)
    {
        RpcStop(audioName);
    }

    // TODO: Document
    public void RpcPlay(string audioName)
    {
        AudioAsset audioAsset = Array.Find(_audioAssets, audioAsset => audioAsset.Name == audioName);

        if (audioAsset == null)
        {
            UnityUtils.LogWarning($"Unable to locate audio asset '{audioName}' on {gameObject.name}.");
            return;
        }
        else
        {
            Debug.Log($"{audioAsset.Resource.AudioSource.clip.name}: {audioAsset.Resource.AudioSource.isPlaying}");
            if (audioAsset.Resource.AudioSource.isPlaying == false)
            {
                audioAsset.Resource.AudioSource.Play();
            }
        }
    }

    // TODO: Document
    public void RpcStop()
    {
        foreach (AudioAsset audioAsset in _audioAssets)
        {
            audioAsset.Resource.AudioSource.Stop();
        }
    }

    // TODO: Document
    public void RpcStop(string audioName)
    {
        AudioAsset audioAsset = Array.Find(_audioAssets, audioAsset => audioAsset.Name == audioName);

        if (audioAsset == null)
        {
            UnityUtils.LogWarning($"Unable to locate audio asset '{audioName}' on {gameObject.name}.");
            return;
        }

        audioAsset.Resource.AudioSource.Stop();
    }
}
