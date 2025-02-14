using Mirror;
using System;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    [System.Serializable]
    private class AudioAsset
    {
        public string Name;
        public AudioResource Resource;
    }

    [SerializeField] private AudioAsset[] _audioAssets;
    [HideInInspector] private AudioSource _audioSource;

    // TODO: Document
    private void Awake()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();

        foreach (AudioAsset audioAsset in _audioAssets)
        {
            audioAsset.Resource.AudioSource = _audioSource;
            audioAsset.Resource.AudioSource.clip = audioAsset.Resource.AudioClip;
            audioAsset.Resource.AudioSource.volume = audioAsset.Resource.Volume;
            audioAsset.Resource.AudioSource.pitch = audioAsset.Resource.Pitch;
            audioAsset.Resource.AudioSource.loop = audioAsset.Resource.Loop;
        }
    }

    // TODO: Document
    public void CmdPlay(string audioName)
    {
        RpcPlay(audioName);
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
            UnityUtils.LogWarning($"{audioName} audio asset does not exist on {gameObject.name}.");
            return;
        }

        audioAsset.Resource.AudioSource.Play();
    }

    // TODO: Document
    public void RpcStop(string audioName)
    {
        AudioAsset audioAsset = Array.Find(_audioAssets, audioAsset => audioAsset.Name == audioName);

        if (audioAsset == null)
        {
            UnityUtils.LogWarning($"{audioName} audio asset does not exist on {gameObject.name}.");
            return;
        }

        audioAsset.Resource.AudioSource.Stop();
    }
}
