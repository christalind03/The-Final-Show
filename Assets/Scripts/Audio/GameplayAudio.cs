using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayAudio : MonoBehaviour
{
    [Min(0f), SerializeField] public float FadeDuration;
    [SerializeField] public AudioAsset[] AudioAssets;

    public static GameplayAudio Instance;
    [HideInInspector] public string CurrentScene;

    /// <summary>
    /// Initializes the singleton instance of the audio manager, assigns audio sources to all defined 
    /// audio assets, and starts playing the scene-specific music.
    /// </summary>
    public void Awake()
    {
        if (Instance == null)
        {
            // Load all AudioAssets
            if (AudioAssets != null)
            {
                foreach (AudioAsset audioAsset in AudioAssets)
                {
                    audioAsset.AudioSource = gameObject.AddComponent<AudioSource>();

                    audioAsset.AudioSource.clip = audioAsset.Resource.AudioClip;
                    audioAsset.AudioSource.volume = audioAsset.Resource.Volume;
                    audioAsset.AudioSource.pitch = audioAsset.Resource.Pitch;
                    audioAsset.AudioSource.loop = audioAsset.Resource.Loop;

                    audioAsset.AudioSource.outputAudioMixerGroup = audioAsset.Resource.AudioMixerGroup;
                }
            }

            CurrentScene = gameObject.scene.name;

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += OnSceneChanged;
            Play(CurrentScene);
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Attempts to play the audio associated with the given scene name by finding the corresponding 
    /// <see cref="AudioAsset"/> and starting a fade-in effect if it is not already playing.
    /// </summary>
    /// <param name="sceneName">The name of the scene whose audio should be played.</param>
    private void Play(string sceneName)
    {
        AudioAsset audioAsset = Array.Find(AudioAssets, audioAsset => audioAsset.Name == sceneName);

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

    /// <summary>
    /// Handles scene transitions by initiating a crossfade between the current scene's audio and the new scene's audio.
    /// Updates the <see cref="CurrentScene"/> to reflect the new scene.
    /// </summary>
    /// <param name="_">The previous scene (unused).</param>
    /// <param name="nextScene">The newly loaded scene.</param>
    private void OnSceneChanged(Scene _, Scene nextScene)
    {
        if(Instance.isActiveAndEnabled){
            StartCoroutine(CrossFade(CurrentScene, nextScene.name));
            CurrentScene = nextScene.name;            
        }
    }

    /// <summary>
    /// Performs a crossfade transition between the current and upcoming scene audio.
    /// The current track fades out while the new track fades in over a specified duration.
    /// </summary>
    /// <param name="currentAudio">The name of the currently playing audio track.</param>
    /// <param name="upcomingAudio">The name of the audio track to transition to.</param>
    /// <returns>An <see cref="IEnumerator"/> for use with <see cref="Coroutine"/>.</returns>
    private IEnumerator CrossFade(string currentAudio, string upcomingAudio)
    {
        AudioAsset currentTrack = Array.Find(AudioAssets, audioAsset => audioAsset.Name == currentAudio);
        AudioAsset upcomingTrack = Array.Find(AudioAssets, audioAsset => audioAsset.Name == upcomingAudio);

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

        for (float currentTime = 0f; currentTime < FadeDuration; currentTime += Time.deltaTime)
        {
            currentTrack.AudioSource.volume = Mathf.Lerp(currentStartVolume, 0f, currentTime / FadeDuration);
            upcomingTrack.AudioSource.volume = Mathf.Lerp(upcomingStartVolume, upcomingTrack.Resource.Volume, currentTime / FadeDuration);
            yield return null;
        }

        upcomingTrack.AudioSource.volume = upcomingTrack.Resource.Volume;
        currentTrack.AudioSource.volume = 0f;
        currentTrack.AudioSource.Stop();
    }

    /// <summary>
    /// Gradually increases the volume of the specified <see cref="AudioAsset"/> over a set duration, creating a fade-in effect.
    /// </summary>
    /// <param name="audioAsset">The <see cref="AudioAsset"/> to fade in.</param>
    /// <returns>An <see cref="IEnumerator"/> for use with <see cref="Coroutine"/>.</returns>
    private IEnumerator FadeIn(AudioAsset audioAsset)
    {
        float startVolume = 0f;
        float targetVolume = audioAsset.Resource.Volume;

        audioAsset.AudioSource.Play();

        for (float currentTime = 0f; currentTime < FadeDuration; currentTime += Time.deltaTime)
        {
            audioAsset.AudioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / FadeDuration);
            yield return null;
        }

        audioAsset.AudioSource.volume = targetVolume;
    }
}
