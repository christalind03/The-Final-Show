using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayMusic : MonoBehaviour
{
    [Min(0f), SerializeField] private float fadeDuration;
    [SerializeField] private AudioAsset[] audioAssets;

    private string currentScene;

    public static GameplayMusic Instance;

    // TODO: Document
    private void Awake()
    {
        if (Instance == null)
        {
            // Load all AudioAssets
            if (audioAssets != null)
            {
                foreach (AudioAsset audioAsset in audioAssets)
                {
                    audioAsset.AudioSource = gameObject.AddComponent<AudioSource>();

                    audioAsset.AudioSource.clip = audioAsset.Resource.AudioClip;
                    audioAsset.AudioSource.volume = audioAsset.Resource.Volume;
                    audioAsset.AudioSource.pitch = audioAsset.Resource.Pitch;
                    audioAsset.AudioSource.loop = audioAsset.Resource.Loop;
                }
            }

            currentScene = gameObject.scene.name;

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += OnSceneChanged;
            Play(currentScene);
        }
        else
        {
            Destroy(this);
        }
    }

    // TODO: Document
    private void Play(string sceneName)
    {
        AudioAsset audioAsset = Array.Find(audioAssets, audioAsset => audioAsset.Name == sceneName);

        if (audioAsset == null)
        {
            UnityUtils.LogWarning($"Unable to locate audio assets '{sceneName}' on {gameObject.name}.");
            return;
        }

        if (audioAsset.AudioSource.isPlaying == false)
        {
            StartCoroutine(FadeIn(audioAsset));
        }
    }

    // TODO: Document
    private void OnSceneChanged(Scene _, Scene nextScene)
    {
        StartCoroutine(CrossFade(currentScene, nextScene.name));
        currentScene = nextScene.name;
    }

    // TODO: Document
    private IEnumerator CrossFade(string currentAudio, string upcomingAudio)
    {
        AudioAsset currentTrack = Array.Find(audioAssets, audioAsset => audioAsset.Name == currentAudio);
        AudioAsset upcomingTrack = Array.Find(audioAssets, audioAsset => audioAsset.Name == upcomingAudio);

        if (currentTrack == null || upcomingTrack == null)
        {
            UnityUtils.LogWarning($"Unable to locate audio assets {currentTrack.Name} or ${upcomingTrack.Name} on ${gameObject.name}");
            yield break;
        }

        if (currentTrack.Name == upcomingTrack.Name) { yield break; }

        // Ensure the upcoming track's volume is set to 0 (aka muted).
        upcomingTrack.AudioSource.volume = 0f;

        float currentStartVolume = currentTrack.AudioSource.volume;
        float upcomingStartVolume = upcomingTrack.AudioSource.volume;

        upcomingTrack.AudioSource.Play();

        for (float currentTime = 0f; currentTime < fadeDuration; currentTime += Time.deltaTime)
        {
            currentTrack.AudioSource.volume = Mathf.Lerp(currentStartVolume, 0f, currentTime / fadeDuration);
            upcomingTrack.AudioSource.volume = Mathf.Lerp(upcomingStartVolume, upcomingTrack.Resource.Volume, currentTime / fadeDuration);
            yield return null;
        }

        upcomingTrack.AudioSource.volume = upcomingTrack.Resource.Volume;
        currentTrack.AudioSource.volume = 0f;
        currentTrack.AudioSource.Stop();
    }

    // TODO: Document
    private IEnumerator FadeIn(AudioAsset audioAsset)
    {
        float startVolume = 0f;
        float targetVolume = audioAsset.Resource.Volume;

        audioAsset.AudioSource.Play();

        for (float currentTime = 0f; currentTime < fadeDuration; currentTime += Time.deltaTime)
        {
            audioAsset.AudioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeDuration);
            yield return null;
        }

        audioAsset.AudioSource.volume = targetVolume;
    }
}
