using System.Collections;
using Mirror;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class SettingSaveLoadTest
{
    private GameObject testObj;
    [UnityTest]
    public IEnumerator Test_Save_Settings()
    {
        yield return SceneManager.LoadSceneAsync("Network-Lobby", LoadSceneMode.Single);
        CustomNetworkManager manager = NetworkManager.FindObjectOfType<CustomNetworkManager>();
        manager.StartHost();
        NetworkConnectionToClient test = new NetworkConnectionToClient(1);
        manager.OnServerReady(test);
        testObj = test.identity.gameObject;
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Gameplay-Intermission");
        yield return new WaitForSeconds(0.5f);  

        if (NetworkServer.connections.TryGetValue(0, out NetworkConnectionToClient conn))
        {
            GameObject playerObject = conn.identity.gameObject;   
            SettingSaveLoad settingSaveLoad = playerObject.GetComponentInChildren<SettingSaveLoad>();
            SettingsMenu settingsMenu = playerObject.GetComponentInChildren<SettingsMenu>();
            Assert.NotNull(settingsMenu);
            Assert.NotNull(settingSaveLoad);
            // Mock items
            settingsMenu.dropdownElements["ScreenSetting"].index = 0;
            settingsMenu.sliderElements["CameraSens"].value = 3.5f;
            settingsMenu.sliderElements["MusicSlider"].value = 1.0f;

            // Simulate saving settings
            NetworkManager.singleton.StopClient();

            yield return new WaitForSeconds(0.5f);  

            // Assert saved values
            Assert.AreEqual(0, PlayerPrefs.GetInt("Screen Setting"));
            Assert.AreEqual(1.0f, PlayerPrefs.GetFloat("Music Volume"), 0.01f);
            Assert.AreEqual(3.5f, PlayerPrefs.GetFloat("Camera Sensitivity"), 0.01f);
        }
    }

    [UnityTest]
    public IEnumerator Test_Load_Settings()
    {
        // Simulate saved settings
        PlayerPrefs.SetInt("Screen Setting", 0);
        PlayerPrefs.SetFloat("Camera Sensitivity", 3.5f);
        PlayerPrefs.SetFloat("Music Volume", 1.0f);
        
        yield return SceneManager.LoadSceneAsync("Network-Lobby", LoadSceneMode.Single);

        CustomNetworkManager manager = NetworkManager.FindObjectOfType<CustomNetworkManager>();
        manager.StartHost();
        NetworkConnectionToClient test = new NetworkConnectionToClient(1);
        manager.OnServerReady(test);
        testObj = test.identity.gameObject;

        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Gameplay-Intermission");
        yield return new WaitForSeconds(0.5f);  

        if (NetworkServer.connections.TryGetValue(0, out NetworkConnectionToClient conn))
        {
            GameObject playerObject = conn.identity.gameObject;     
            SettingSaveLoad settingSaveLoad = playerObject.GetComponentInChildren<SettingSaveLoad>();
            SettingsMenu settingsMenu = playerObject.GetComponentInChildren<SettingsMenu>();
            Assert.NotNull(settingsMenu);
            Assert.NotNull(settingSaveLoad);

            Assert.AreEqual(0, settingsMenu.dropdownElements["ScreenSetting"].index);
            Assert.AreEqual(3.5f, settingsMenu.sliderElements["CameraSens"].value, 0.01f);
            Assert.AreEqual(1.0f, settingsMenu.sliderElements["MusicSlider"].value, 0.01f);
        }
    }

    [TearDown]
    public void TearDown() {
        PlayerPrefs.DeleteKey("Screen Setting");
        PlayerPrefs.DeleteKey("Camera Sensitivity");
        PlayerPrefs.DeleteKey("Music Volume");
        PlayerPrefs.Save();

        Object.DestroyImmediate(testObj);
        
        if (NetworkManager.singleton != null && NetworkManager.singleton.isNetworkActive)
        {
            NetworkManager.singleton.StopHost();
        }

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                GameObject.Destroy(conn.identity.gameObject);
            }
        }
        NetworkServer.Shutdown();
        NetworkClient.Shutdown();
    }
}
