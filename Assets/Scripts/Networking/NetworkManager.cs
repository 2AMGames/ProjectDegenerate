using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// Class for managing network interaction between clients
// Should monitor network connection, Average Latency, and send input data to other players in game.
// Should also accept input data from other players
public class NetworkManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks, IOnEventCallback
{

    #region Photon Event Codes

    private const byte PlayerConnected = 0x00;

    private const byte SynchronizationNeeded = 0x01;

    private const byte PlayerInputUpdate = 0x02;

    private const byte PlayerInputAck = 0x03;

    private const byte PlayerLeaving = 0x04;

    #endregion

    #region main variables

    public bool UsePhotonMatchmaking;

    public string CurrentRoomId;

    #endregion

    #region Monobehaviour Methods

    void Awake()
    {
        if (!PhotonNetwork.PhotonServerSettings.StartInOfflineMode)
        {
            PhotonNetwork.AddCallbackTarget(this);
            PhotonNetwork.ConnectUsingSettings();
        }
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
    }

    #endregion

    #region Connection Callback Interface
    public void OnConnected()
    {
        Debug.LogWarning("Connected to master server");
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
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        
    }

    #endregion

    #region EventCallbacks

    // On photon event received callback
    public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
    {
        Debug.LogWarning("Code: " + photonEvent.Code);
    }

    #endregion
}
