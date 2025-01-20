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

    /// <summary>
    /// Called when the client connects to the server.
    /// Registers custom network message handlers for countdown and spectate functionality.
    /// </summary>
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        // Register handlers for custom network messages for every client that connects
        NetworkClient.RegisterHandler<CountdownMessage>(OnCountdown);
        NetworkClient.RegisterHandler<SpectateMessage>(OnSpectate);
    }

    /// <summary>
    /// Called when the client disconnects from the server.
    /// Unregisters custom network message handlers for countdown and spectate functionality.
    /// </summary>
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        // Unregister handlers for custom network messages for every client that disconnects
        NetworkClient.UnregisterHandler<CountdownMessage>();
        NetworkClient.UnregisterHandler<SpectateMessage>();
    }

    /// <summary>
    /// Called when the client is ready (has the scene loaded) on the server.
    /// Spawns or relocates the player based on the client identity.
    /// </summary>
    /// <param name="clientConnection">The connection of the client that is ready</param>
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

    /// <summary>
    /// Spawns a new player for the given client connection.
    /// </summary>
    /// <param name="clientConnection">The connection of the client to spawn the player for</param>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
    public IEnumerator SpawnPlayer(NetworkConnectionToClient clientConnection)
    {
        if (clientConnection.identity != null) { yield break; }

        Transform startPosition = GetStartPosition();
        GameObject networkPlayer = Instantiate(playerPrefab, startPosition.position, startPosition.rotation);

        yield return new WaitForEndOfFrame();

        NetworkServer.AddPlayerForConnection(clientConnection, networkPlayer);
    }

    /// <summary>
    /// Relocates the player for the given client connection to a new spawn position.
    /// </summary>
    /// <param name="clientConnection">The connection of the client to relocate the player for</param>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
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

    /// <summary>
    /// Initiates a countdown on the server and synchronizes it with the clients.
    /// When the countdown finishes, the specified callback function is executed.
    /// </summary>
    /// <param name="countdownMessage">The countdown message to send to all clients</param>
    /// <param name="callbackFn">The callback function to invoke after the countdown finishes</param>
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

    /// <summary>
    /// Handles the countdown on the client side, updating the UI to reflect the remaining time.
    /// </summary>
    /// <param name="countdownMessage">The countdown message received from the server</param>
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

    /// <summary>
    /// Handles the countdown on the client side, displaying the countdown on the UI.
    /// </summary>
    /// <param name="countdownMessage">The countdown message containing information on how to display the countdown</param>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="countdownMessage"/> if null</exception>
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

    /// <summary>
    /// Handles the server countdown, waiting for the specified duration before invoking the callback.
    /// </summary>
    /// <param name="countdownDuration">The duration of the countdown in seconds</param>
    /// <param name="callbackFn">The callback function to invoke after the countdown is complete</param>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
    private IEnumerator ServerCountdown(int countdownDuration, Action callbackFn)
    {
        // The duration is countdownDuration + .05 because the client needs a little 
        // bit of buffer to disable UI before getting teleported to the next scene 
        yield return new WaitForSeconds((float)(countdownDuration + .05));
        callbackFn?.Invoke();
    }

    #endregion

    #region SpectateMessage Functionality

    /// <summary>
    /// Handles spectating functionality.
    /// Whena spectate message is received, the player is marked as
    /// not alive with their visibility is toggled off, enabling spectator mode.
    /// </summary>
    /// <param name="spectateMessage">The <see cref="SpectateMessage"/> received from the server</param>
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
