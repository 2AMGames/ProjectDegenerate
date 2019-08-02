using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

// Class for managing network interaction between clients
// Should monitor network connection, Average Latency, and send input data to other players in game.
// Should also accept input data from other players
public class NetworkManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks, IOnEventCallback, IInRoomCallbacks
{

    #region Custom Photon Event Codes

    public const byte PlayerConnected = 0x00;

    public const byte RemotePlayerReady = 0x01;

    public const byte RemotePlayerReadyAck = 0x02;

    public const byte PlayerInputUpdate = 0x03;

    public const byte PlayerInputAck = 0x04;

    public const byte EvaluateWinner = 0x05;

    public const byte RollbackRequest = 0x06;

    public const byte PlayerLeaving = 0x07;

    #endregion

    #region Photon Event Codes

    // DO NOT CHANGE THESE.

    public const float OnJoinedRoomEventCode = 254;

    #endregion

    #region Main Variables

    public bool UsePhotonMatchmaking;

    [HideInInspector]
    public string CurrentRoomId;

    public int CurrentDelayFrames;

    public int CurrentPing
    {
        get
        {
            return 0;
        }
    }

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
       if (Overseer.Instance.SelectedGameType == Overseer.GameType.PlayerVsRemote)
        {
            ConnectToNetwork();
        }
    }

    void OnValidate()
    {
        if(!UsePhotonMatchmaking)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
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

    public void SendEventData(byte eventCode, object eventData)
    {
        RaiseEventOptions receiveOptions = new RaiseEventOptions();
        receiveOptions.Receivers = ReceiverGroup.Others;
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
        PhotonPeer.RegisterType(typeof(PlayerInputData), PlayerInputUpdate, PlayerInputData.Serialize, PlayerInputData.Deserialize);
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
        foreach(RoomInfo room in roomList)
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
        Debug.LogWarning("Connected to Master Server. Joining Lobby");
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

    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {

    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        //
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        //
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
       //
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
       //
    }

    #endregion

}
