using System;
using System.Collections;
using System.Collections.Generic;

using ExitGames.Client.Photon;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

using PlayerInputData = PlayerInputPacket.PlayerInputData;

public class NetworkInputHandler : MonoBehaviour, IOnEventCallback, IMatchmakingCallbacks
{

    #region const variables
    
    // If the difference in ping we read from the custom player properties is more than this value, then we should
    // update the custom properties with the new value.
    private const int PingThreshold = 5;

    private const float SecondsToCheckForPing = 15f;

    #endregion

    #region main variables

    private uint PacketsSent;

    private PlayerController PlayerController;

    private CommandInterpreter CommandInterpreter;

    private List<PlayerInputData> DataSent = new List<PlayerInputData>();

    #endregion

    #region Callbacks
    public void OnCreatedRoom()
    {
        //
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        //
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        //
    }

    public void OnJoinedRoom()
    {

    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        //
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        //
    }

    public void OnLeftRoom()
    {
        //
    }

    #endregion

    #region monobehaviour methods

    void Awake()
    {
        PlayerController = GetComponent<PlayerController>();
        CommandInterpreter = PlayerController.CommandInterpreter;
        Overseer.Instance.OnGameReady += OnGameReady;
        PhotonNetwork.AddCallbackTarget(this);
    }

    #endregion

    #region event callback

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == NetworkManager.PlayerInputAck)
        {
            int packetNumber = (int)photonEvent.CustomData;
            HandlePlayerInputAck(packetNumber);
        }
    }

    #endregion

    #region private interface

    private void HandlePlayerInputAck(int packetNumber)
    {
        PlayerInputData data = DataSent.Find(x => x.PacketId == packetNumber);
        if (data.IsValid())
        {
            DataSent.Remove(data);
        }
    }

    private void OnGameReady(bool isGameReady)
    {
        if (isGameReady)
        {
            StartCoroutine(CheckForPingUpdate());
            StartCoroutine(SendInputIfNeccessary());
            enabled = true;
        }
    }

    private IEnumerator CheckForPingUpdate()
    {
        while (Overseer.Instance.IsGameReady)
        {
            yield return new WaitForSeconds(SecondsToCheckForPing);
            NetworkManager.Instance.PingActivePlayers();
        }
    }

    private IEnumerator SendInputIfNeccessary()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (Overseer.Instance.IsGameReady && CommandInterpreter != null)
            {
                PlayerInputData inputData = CommandInterpreter.GetPlayerInputDataIfUpdated();
                if (inputData.InputPattern > 0)
                {
                    PlayerInputPacket packetToSend = new PlayerInputPacket();

                    inputData.PacketId = PacketsSent;
                    DataSent.Add(inputData);

                    packetToSend.PlayerIndex = PlayerController.PlayerIndex;
                    packetToSend.PacketId = PacketsSent;;
                    packetToSend.InputData = new List<PlayerInputData>(DataSent);
                    NetworkManager.Instance.SendEventData(NetworkManager.PlayerInputUpdate, packetToSend);
                    ++PacketsSent;
                }
            }
        }
    }

    #endregion
}
