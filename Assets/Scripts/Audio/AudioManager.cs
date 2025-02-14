using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Manager", menuName = "Audio Manager")]
public class AudioManager : ScriptableObject
{
    [System.Serializable]
    private class AudioEntry
    {
        public string Name;
        public AudioClip AudioClip;

        [Range(0f, 1f)] public float Volume = 0.5f;
        [Range(0.1f, 3f)] public float Pitch = 1f;
        public bool Loop;

        [HideInInspector] public AudioSource AudioSource;
    }

    [SerializeField] private AudioEntry[] _audioEntries;
    [HideInInspector] public AudioSource AudioSource;

    // TODO: Document
    public void Initialize()
    {
        foreach (AudioEntry audioEntry in _audioEntries)
        {
            audioEntry.AudioSource = AudioSource;
            audioEntry.AudioSource.clip = audioEntry.AudioClip;
            audioEntry.AudioSource.volume = audioEntry.Volume;
            audioEntry.AudioSource.pitch = audioEntry.Pitch;
            audioEntry.AudioSource.loop = audioEntry.Loop;
        }
    }

    // TODO: Document
    public void Play(string audioName)
    {
        AudioEntry audioEntry = Array.Find(_audioEntries, audioEntry => audioEntry.Name == audioName);

        if (audioEntry != null)
        {
            audioEntry.AudioSource.Play();
        }
    }

    // TODO: Document
    public void Stop(string audioName)
    {
        AudioEntry audioEntry = Array.Find(_audioEntries, audioEntry => audioEntry.Name == audioName);

        if (audioEntry != null)
        {
            audioEntry.AudioSource.Stop();
        }
    }
}
