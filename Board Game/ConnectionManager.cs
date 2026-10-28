//-----------------------------------------------------------------------
// <copyright file="ConnectionManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OGT;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Scripting.LifecycleManagement;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

/// <summary>
/// Handles the connection to the Distributed Authority Backend Service.
/// If you want to modify this Script please copy it into your own project and add it to your Player Prefab.
/// </summary>
public partial class ConnectionManager : GameBehavior, IAwake
{
    [AutoStaticsCleanup]
    public static ConnectionManager Instance { get; private set; }

    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private UnityTransport transport;
    [SerializeField] private int maxPlayers = 10;

    private ISession m_Session;

    /// <summary>
    /// Raised whenever <see cref="State"/> changes.
    /// </summary>
    public event Action<ConnectionState> StateChanged;

    /// <summary>
    /// Status of the Connection.
    /// </summary>
    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    /// <summary>
    /// The session we're currently connected to (null if not connected).
    /// </summary>
    public ISession Session => m_Session;

    /// <summary>
    /// The NetworkManager the session drives (falls back to the singleton if none was assigned).
    /// </summary>
    public NetworkManager NetworkManager => networkManager != null ? networkManager : NetworkManager.Singleton;

    /// <summary>
    /// The different states the connection can be in.
    /// </summary>
    public enum ConnectionState
    {
        Disconnected,
        SigningIn,
        Connecting,
        Connected,
    }

    /// <summary>
    /// Leaves the current session and sets the <see cref="State"/> to Disconnected.
    /// </summary>
    public async Task Disconnect()
    {
        var session = m_Session;
        m_Session = null;

        if (session != null)
        {
            try
            {
                await session.LeaveAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        SetState(ConnectionState.Disconnected);
    }

    /// <summary>
    /// Creates or joins an existing session.
    /// </summary>
    /// <param name="sessionName">The name of the session to join or create</param>
    /// <param name="profileName">The name of the player</param>
    /// <param name="playerProperties">Optional properties (display name, etc) to share with everyone in the session</param>
    public async Task CreateOrJoinSessionAsync(string sessionName, string profileName, Dictionary<string, PlayerProperty> playerProperties = null)
    {
        if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(sessionName))
        {
            Debug.LogError("Please provide a player and session name, to login.");
            return;
        }

        SetState(ConnectionState.SigningIn);

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            // Only sign in if not already signed in.
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SwitchProfile(profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            SetState(ConnectionState.Connecting);

            // Make sure the transport matches the relay protocol we're about to request.
            ConfigureTransport();

            // Set the session options.
            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = maxPlayers
            }
            .WithDistributedAuthorityNetwork()
            .WithNetworkOptions(new NetworkOptions { RelayProtocol = RelayProtocol });

            if (playerProperties != null)
            {
                options.PlayerProperties = playerProperties;
            }

            // Join a session if it already exists, or create a new one.
            m_Session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);
            SetState(ConnectionState.Connected);
        }
        catch (Exception e)
        {
            m_Session = null;
            SetState(ConnectionState.Disconnected);
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// The relay protocol for the current platform: WebSockets (WSS) on WebGL, UDP everywhere else.
    /// </summary>
    /// <remarks>
    /// This must be a compile-time check (not <see cref="Application.platform"/>) because the Multiplayer SDK
    /// uses <c>UNITY_WEBGL</c> to decide which protocols are valid. In the Editor with the WebGL build target active,
    /// <see cref="Application.platform"/> reports WindowsEditor, but the SDK only accepts WSS and silently swaps
    /// UDP for WSS - leaving the transport with UseWebSockets off while the relay data says WebSockets.
    /// </remarks>
#if UNITY_WEBGL
    private static RelayProtocol RelayProtocol => RelayProtocol.WSS;
#else
    private static RelayProtocol RelayProtocol => RelayProtocol.UDP;
#endif

    /// <summary>
    /// Points the NetworkManager at our transport and configures the transport for the platform's relay protocol.
    /// The relay server data supplied by the session (address, keys, secure flag) is applied by the Multiplayer SDK.
    /// </summary>
    void ConfigureTransport()
    {
        var manager = NetworkManager;
        if (manager == null)
        {
            Debug.LogError($"{nameof(ConnectionManager)} has no {nameof(Unity.Netcode.NetworkManager)} to configure.");
            return;
        }

        if (transport == null)
        {
            transport = manager.NetworkConfig.NetworkTransport as UnityTransport;
            if (transport == null)
            {
                Debug.LogError($"{nameof(ConnectionManager)} requires a {nameof(UnityTransport)} on the {nameof(Unity.Netcode.NetworkManager)}.");
                return;
            }
        }

        // The Multiplayer SDK reuses the NetworkManager's assigned transport (when it's a DistributedAuthorityTransport),
        // so make sure that's the one we're configuring.
        manager.NetworkConfig.NetworkTransport = transport;

        // Must match the relay protocol, otherwise UnityTransport logs an error and/or picks the wrong network interface.
        transport.UseWebSockets = RelayProtocol == RelayProtocol.WSS;

        // UseEncryption only applies to direct (non-relay) connections; relay security comes from the relay server data.
        transport.UseEncryption = false;
    }

    void SetState(ConnectionState state)
    {
        if (State != state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }

    // Just for logging.
    void OnClientConnectedCallback(ulong clientId)
    {
        if (networkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
        }
    }

    // Just for logging.
    void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (networkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{networkManager.LocalClientId} is the session owner!");
        }
    }

    public void OnAwake(Bootloader bootloader)
    {
        Instance = this;
        Platform.AddShutdownTask(this.Disconnect);
        ConfigureTransport();
    }
}
