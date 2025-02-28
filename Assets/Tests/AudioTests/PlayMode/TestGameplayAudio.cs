using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestGameplayAudio
{
    [UnityTest]
    public IEnumerator SwitchScenes()
    {
        string initialScene = "Gameplay-Intermission";
        string nextScene = "Gameplay-Preparation";

        // Load the initial scene first
        yield return SceneManager.LoadSceneAsync(initialScene, LoadSceneMode.Single);
        Assert.AreEqual(initialScene, SceneManager.GetActiveScene().name);

        Assert.IsNotNull(GameplayAudio.Instance);
        Assert.IsNotNull(GameplayAudio.Instance.AudioAssets);

        // Verify the initial scene's audio clip is playing
        Assert.AreEqual(initialScene, GameplayAudio.Instance.CurrentScene);

        AudioSource initialAudio = RetrieveAudio(GameplayAudio.Instance.gameObject);
        Assert.IsNotNull(initialAudio);
        Assert.IsTrue(initialAudio.isPlaying);

        // Load the next scene to test audio transitions
        yield return SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
        Assert.AreEqual(nextScene, SceneManager.GetActiveScene().name);

        // Wait for the crossfade to complete
        yield return new WaitForSeconds(GameplayAudio.Instance.FadeDuration);

        // Verify the next scene's audio clip is playing
        Assert.AreEqual(nextScene, GameplayAudio.Instance.CurrentScene);

        AudioSource nextAudio = RetrieveAudio(GameplayAudio.Instance.gameObject);
        Assert.IsNotNull(nextAudio);
        Assert.IsTrue(nextAudio.isPlaying);

        // Ensure it's not the same audio clip playing
        Assert.AreNotSame(initialAudio, nextAudio);

        yield return null;
    }

    public AudioSource RetrieveAudio(GameObject gameObject)
    {
        foreach (AudioSource audioSource in gameObject.GetComponents<AudioSource>())
        {
            if (audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        return null;
    }
}
