using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Network Manager that controls all the server and client processing
/// </summary>
public class CustomNetworkManager : NetworkManager
{
    public ulong LobbyId { get; set; }
    public static CustomNetworkManager Instance => (CustomNetworkManager)NetworkManager.singleton;
    private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();

    // TODO: Document
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        // Register handlers for custom network messages for every client that connects
        NetworkClient.RegisterHandler<CountdownMessage>(OnCountdown);
        NetworkClient.RegisterHandler<SpectateMessage>(OnSpectate);
    }

    // TODO: Document
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        // Unregister handlers for custom network messages for every client that disconnects
        NetworkClient.UnregisterHandler<CountdownMessage>();
        NetworkClient.UnregisterHandler<SpectateMessage>();
    }

    // TODO: Document
    public override void OnServerReady(NetworkConnectionToClient clientConnection)
    {
        if (clientConnection.isReady) { return; }

        base.OnServerReady(clientConnection);

        if (clientConnection.identity == null)
        {
            StartCoroutine(SpawnPlayer(clientConnection));
        }
        else
        {
            StartCoroutine(RelocatePlayer(clientConnection));
        }
    }

    // TODO: Document
    public IEnumerator SpawnPlayer(NetworkConnectionToClient clientConnection)
    {
        if (clientConnection.identity != null) { yield break; }

        Transform startPosition = GetStartPosition();
        GameObject networkPlayer = Instantiate(playerPrefab, startPosition.position, startPosition.rotation);

        yield return new WaitForEndOfFrame();

        NetworkServer.AddPlayerForConnection(clientConnection, networkPlayer);
    }

    // TODO: Document
    public IEnumerator RelocatePlayer(NetworkConnectionToClient clientConnection)
    {
        GameObject networkPlayer = clientConnection.identity.gameObject;
        Transform startPosition = GetStartPosition();

        NetworkServer.RemovePlayerForConnection(clientConnection);

        if (startPosition != null)
        {
            networkPlayer.transform.position = startPosition.position;
            networkPlayer.transform.rotation = startPosition.rotation;
        }

        yield return new WaitForEndOfFrame();
        
        NetworkServer.AddPlayerForConnection(clientConnection, networkPlayer);
    }

    #region CountdownMessage Functionality
    // This region contains functionality for handling countdown timers on both the clients and server side.

    // TODO: Document
    public void Countdown(CountdownMessage countdownMessage, Action callbackFn)
    {
        NetworkServer.SendToAll(countdownMessage);
        
        // Checks if this coroutine is active and disables the coroutine if it is active so no more
        // than one type is running 
        if(activeCoroutines.TryGetValue("Server", out Coroutine value))
        {
            StopCoroutine(value);
            activeCoroutines.Remove("Server");
        }
        activeCoroutines.Add("Server", StartCoroutine(ServerCountdown(countdownMessage.Duration, callbackFn)));
    }

    // TODO: Document
    private void OnCountdown(CountdownMessage countdownMessage)
    {
        // Checks if this coroutine is active and disables the coroutine if it is active so no more
        // than one type is running 
        if(activeCoroutines.TryGetValue("Client", out Coroutine value))
        {
            StopCoroutine(value);
            activeCoroutines.Remove("Client");
        }
        activeCoroutines.Add("Client", StartCoroutine(ClientCountdown(countdownMessage)));
    }

    // TODO: Document
    private IEnumerator ClientCountdown(CountdownMessage countdownMessage)
    {
        // Wait until the client identity is defined
        // Since the client may be switching scenes during this time, we want to ensure we retrieve the correct identity instance
        yield return new WaitUntil(() => NetworkClient.connection.identity);

        // Retrieve the client's root visual element through their UI Document component
        GameObject clientObject = NetworkClient.connection.identity.gameObject;
        VisualElement rootVisualElement = clientObject.GetComponent<UIDocument>().rootVisualElement;

        // Check to ensure that the client's target UI element is defined. If not, throw an error.
        if (countdownMessage.MessageElement == null)
        {
            throw new ArgumentNullException(
                nameof(countdownMessage.MessageElement),
                "The 'MessageElement' property is required to propagate notifications to clients. Please specify a PlayerUI visual element."
            );
        }

        // If the target UI element was successfully found, display the countdown for the client
        if (UnityUtils.ContainsElement(rootVisualElement, countdownMessage.MessageElement, out Label uiElement))
        {
            uiElement.style.opacity = 1;
            int timeRemaining = countdownMessage.Duration;

            while (0 < timeRemaining)
            {
                int minutesRemaining = timeRemaining / 60;
                int secondsRemaining = timeRemaining % 60;
                string formattedMessage = countdownMessage.MessageFormat;

                if (string.IsNullOrEmpty(formattedMessage))
                {
                    formattedMessage = $"{minutesRemaining}:{secondsRemaining:D2}";
                }
                else
                {
                    switch (countdownMessage.MessageDisplay)
                    {
                        case CountdownMessage.DisplayMode.Both:
                            formattedMessage = formattedMessage
                                .Replace("{MINUTES}", minutesRemaining.ToString())
                                .Replace("{SECONDS}", secondsRemaining.ToString());
                        
                            break;

                        case CountdownMessage.DisplayMode.Minutes:
                            formattedMessage = formattedMessage
                                .Replace("{MINUTES}", minutesRemaining.ToString())
                                .Replace("{SECONDS}", "");

                            break;

                        case CountdownMessage.DisplayMode.Seconds:
                            formattedMessage = formattedMessage
                                .Replace("{MINUTES}", "")
                                .Replace("{SECONDS}", secondsRemaining.ToString());

                            break;

                        case CountdownMessage.DisplayMode.None:
                            formattedMessage = string.Empty;
                            break;
                    }
                }

                uiElement.text = formattedMessage;
                yield return new WaitForSeconds(1f);
                timeRemaining--;
            }
            
            uiElement.style.opacity = 0;
        }
    }

    // TODO: Document
    private IEnumerator ServerCountdown(int countdownDuration, Action callbackFn)
    {
        // The duration is countdownDuration + .05 because the client needs a little 
        // bit of buffer to disable UI before getting teleported to the next scene 
        yield return new WaitForSeconds((float)(countdownDuration + .05));
        callbackFn?.Invoke();
    }

    #endregion

    #region SpectateMessage Functionality

    // TODO: Document
    private void OnSpectate(SpectateMessage spectateMessage)
    {
        // NOTE: We may have to ensure the correct scene before enabling spectator mode
        //       Since no issues have yet surfaced, we will deal with that at a later point

        StartCoroutine(NetworkUtils.WaitUntilReady((NetworkIdentity clientIdentity) =>
        {
            GameObject playerObject = clientIdentity.gameObject;

            playerObject.GetComponent<PlayerHealth>().CmdDamage(float.MaxValue);
            playerObject.GetComponent<PlayerVisibility>().CmdToggleVisibility(false);
        }));
    }

    #endregion
}
