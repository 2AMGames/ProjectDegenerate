using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

using Debug = UnityEngine.Debug;
// Class for managing network interaction between clients
// Should monitor network connection, Average Latency, and send input data to other players in game.
// Should also accept input data from other players
public class NetworkManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks, IOnEventCallback, IInRoomCallbacks
{

    #region Custom Photon Event Codes

    public const byte RemotePlayerReady = 0x10;

    public const byte RemotePlayerReadyAck = 0x11;

    public const byte PlayerInputUpdate = 0x20;

    public const byte PlayerInputAck = 0x21;

    public const byte PingRequest = 0x30;

    public const byte PingAck = 0x31;

    public const byte EvaluateWinner = 0x40;

    public const byte RollbackRequest = 0x50;

    public const byte PlayerLeaving = 0x60;

    #endregion

    #region Player Custom Property Keys

    public const string PlayerPingKey = "Ping";

    #endregion

    #region Photon Event Codes

    // DO NOT CHANGE THESE.

    public const float OnJoinedRoomEventCode = 254;

    #endregion

    #region Const Variables

    private const string ActivePlayerKey = "ActivePlayers";

    private const int MillisecondsPerFrame = 16;

    #endregion

    #region Main Variables

    [HideInInspector]
    public string CurrentRoomId;

    public long CurrentDelayFrames
    {
        get;  private set;
    }

    public long CurrentDelayInMilliSeconds { get; private set; }

    // Hash set of players we need to ping, sorted by actor number.
    private HashSet<int> PlayersToPing = new HashSet<int>(); 

    private bool CurrentlyPingingPlayers;

    #endregion

    #region Singleton Instance

    private static NetworkManager instance;

    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NetworkManager>();
            }
            if (instance == null)
            {
                instance = new NetworkManager();
            }

            return instance;
        }
    }

    #endregion

    #region Monobehaviour Methods

    void Awake()
    {
        if (Overseer.Instance.IsNetworkedMode)
        {
            ConnectToNetwork();
        }
    }

    void OnDestroy()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    #endregion

    #region Public Interface

    public void ConnectToNetwork()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AddCallbackTarget(this);
            RegisterEventTypes();
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void DisconnectToNetwork()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void SendEventData(byte eventCode, object eventData, ReceiverGroup receivers = 0)
    {
        RaiseEventOptions receiveOptions = new RaiseEventOptions();
        receiveOptions.Receivers = receivers;
        PhotonNetwork.RaiseEvent(eventCode, eventData, receiveOptions, default);
    }

    #endregion

    #region Private Interface

    private void SetPlayerPhotonSettings()
    {
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
    }

    private void RegisterEventTypes()
    {
        PhotonPeer.RegisterType(typeof(PlayerInputPacket), PlayerInputUpdate, PlayerInputPacket.Serialize, PlayerInputPacket.Deserialize);
    }

    private void UpdateDelay()
    {

    }

    #endregion

    #region Lobby Callbacks

    public void OnJoinedLobby()
    {
        Debug.LogWarning("Joined lobby");
    }

    public void OnLeftLobby()
    {
        Debug.Log("Photon: Left Lobby");
    }

    // Room list. Automatically called after joining lobby.
    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room List Count: " + roomList.Count);
        foreach (RoomInfo room in roomList)
        {
            Debug.Log("Room Name = " + room.Name + ", Player Count:" + room.PlayerCount);
        }

        if (roomList.Count > 0)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.CreateRoom("Bitch Niggas Only", null);
        }
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        throw new System.NotImplementedException();
    }

    #endregion

    #region Matchmaking Callbacks

    public void OnCreatedRoom()
    {
        Debug.Log("Sucessfully created room: RoomID = " + PhotonNetwork.CurrentRoom.Name);
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to create room: Code=[" + returnCode + "], Error = " + message);
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        // To Do Print Friends list
    }

    public void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        CurrentRoomId = PhotonNetwork.CurrentRoom.Name;
        Overseer.Instance.HandleJoinedRoom();
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join random room: Code=[" + returnCode + "], Error = " + message);
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: Code=[" + returnCode + "], Error = " + message);
    }

    public void OnLeftRoom()
    {
        Debug.Log("Sucessfully Left Photon Room");
        CurrentRoomId = null;
    }

    #endregion

    #region Connection Callback Interface
    public void OnConnected()
    {

    }

    public void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {

    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {

    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected: " + cause.ToString());
        CurrentRoomId = null;
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    #endregion

    #region EventCallbacks

    // On photon event received callback
    public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == RemotePlayerReadyAck)
        {
            if (PhotonNetwork.LocalPlayer == PhotonNetwork.MasterClient)
            {
                int actorNumber = (int)photonEvent.CustomData;
                if (PhotonNetwork.CurrentRoom.Players.ContainsKey(actorNumber))
                {
                    Player playerToAdd = PhotonNetwork.CurrentRoom.Players[actorNumber];
                    AddPlayerToActivePlayerTable(playerToAdd);
                }

            }
        }

        if (photonEvent.Code == PingRequest)
        {
            SendEventData(PingAck, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        if (photonEvent.Code == PingAck && CurrentlyPingingPlayers)
        {
            int actorNumber = (int)photonEvent.CustomData;
            if (actorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                OnPingAckReceived(actorNumber);
            }
        }
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {

    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.LocalPlayer == PhotonNetwork.MasterClient && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ActivePlayerKey))
        {
            ExitGames.Client.Photon.Hashtable activePlayers = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[ActivePlayerKey];
            if (activePlayers.ContainsKey(otherPlayer.ActorNumber))
            {
                RemovePlayerFromActiveTable(otherPlayer);
            }
        }
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {

    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ActivePlayerKey) && targetPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            ExitGames.Client.Photon.Hashtable activePlayers = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[ActivePlayerKey];
            if (activePlayers.ContainsKey(targetPlayer.ActorNumber))
            {
                UpdatePing();
            }
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        //
    }

    #endregion

    #region Master Client Methods
    // Methods that should only be executed if the current client is designated by the server as the master client
    private void AddPlayerToActivePlayerTable(Player player)
    {

        // Add player to set found in room custom properties
        // set should contain players that are currently player (not observing).
        ExitGames.Client.Photon.Hashtable hashtable = PhotonNetwork.CurrentRoom.CustomProperties;

        ExitGames.Client.Photon.Hashtable activePlayers = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[ActivePlayerKey] ?? new ExitGames.Client.Photon.Hashtable();

        if (!activePlayers.ContainsKey(player.ActorNumber))
        {
            activePlayers.Add(player.ActorNumber, player.ActorNumber);
        }
        hashtable[ActivePlayerKey] = activePlayers;

        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);

        UpdatePing();
    }

    private void RemovePlayerFromActiveTable(Player player)
    {
        // Remove player from dictionary after leaving

        ExitGames.Client.Photon.Hashtable hashtable = PhotonNetwork.CurrentRoom.CustomProperties;
        ExitGames.Client.Photon.Hashtable activePlayers = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[ActivePlayerKey] ?? new ExitGames.Client.Photon.Hashtable();

        if (activePlayers.ContainsKey(player.ActorNumber))
        {
            activePlayers.Remove(player.ActorNumber);
        }
        hashtable[ActivePlayerKey] = activePlayers;

        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
    }

    #endregion

    #region Ping Methods

    public void PingActivePlayers()
    {
        if (!CurrentlyPingingPlayers)
        {
            StartCoroutine(CheckPlayerPing());
        }
    }

    private IEnumerator CheckPlayerPing()
    {
        CurrentlyPingingPlayers = true;

        while (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ActivePlayerKey))
        {
            yield return null;
        }

        ExitGames.Client.Photon.Hashtable activePlayers = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[ActivePlayerKey];
        foreach (int actorNumber in activePlayers.Keys)
        {
            if (PhotonNetwork.CurrentRoom.Players.ContainsKey(actorNumber) && actorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                PlayersToPing.Add(actorNumber);
            }
        }

        PingActivePlayersInternal();
        Stopwatch rtt = new Stopwatch();
        rtt.Start();

        while(PlayersToPing.Count > 0)
        {
            yield return null;
        }

        rtt.Stop();
        SetLocalPlayerPing(rtt.ElapsedMilliseconds);
        UpdatePing();
        CurrentlyPingingPlayers = false;
        
    }

    private void PingActivePlayersInternal()
    {
        SendEventData(PingRequest, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private void OnPingAckReceived(int actorNumber)
    {
       if (PlayersToPing.Contains(actorNumber))
        {
            PlayersToPing.Remove(actorNumber);
        }
    }

    public void SetLocalPlayerPing(long pingInMilliseconds)
    {
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties[PlayerPingKey] = pingInMilliseconds;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    private void UpdatePing()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ActivePlayerKey))
        {
            ExitGames.Client.Photon.Hashtable activePlayers = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[ActivePlayerKey];
            if (activePlayers != null && activePlayers.Count >= Overseer.NumberOfPlayers)
            {
                long highestPing = 0;

                foreach(int actorNumber in activePlayers.Keys)
                {
                    Player player = PhotonNetwork.CurrentRoom.Players.ContainsKey(actorNumber) ? PhotonNetwork.CurrentRoom.Players[actorNumber] : null;
                    if (player != null)
                    {
                        long playerPing = player.CustomProperties.ContainsKey(PlayerPingKey) ? (long) player.CustomProperties[PlayerPingKey]: (long) 0;
                        highestPing = player.CustomProperties.ContainsKey(PlayerPingKey) ? Math.Max((long)player.CustomProperties[PlayerPingKey], highestPing) : 0;
                    }
                }

                CurrentDelayInMilliSeconds = highestPing;

                
                float localDelayInMilliseconds = GameStateManager.Instance.LocalFrameDelay * MillisecondsPerFrame;
                float frameTimeInMilliseconds = Time.deltaTime * 1000;
                float calculatedDelay = Mathf.Ceil((highestPing + localDelayInMilliseconds) / (2.0f * frameTimeInMilliseconds));
                CurrentDelayFrames = (long)calculatedDelay;
            }
        }
    }

    #endregion

    #region Game Sync Methods

    public void RequestSynchronization(uint FrameToSync)
    {
        Debug.LogWarning("Requesting Synchronization");
    }

    #endregion
}
