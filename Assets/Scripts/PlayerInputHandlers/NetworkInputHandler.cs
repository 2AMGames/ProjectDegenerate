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

    private const float MaxSecondsTillCheckPing = 10f;

    private const short MaxPacketsTillUpdatePing = 16;

    #endregion

    #region main variables

    private uint PacketsSent;

    private PlayerController PlayerController;

    private CommandInterpreter CommandInterpreter;

    private List<PlayerInputData> DataSent = new List<PlayerInputData>();

    private Dictionary<uint, long> SentPackets = new Dictionary<uint, long>();

    private float SecondsUntilPing;

    private short PacketsReceivedByAck;

    private long AveragePing;

    private Coroutine UpdatePingCoroutine;

    private bool UpdatingPing;

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
            HandlePlayerInputAck((uint)packetNumber);
        }
    }

    public void SendInput(PlayerInputData input)
    {
        // If we are currently synchronizing the game state by catching up to the highest frame, do not send off any inputs.
        if (Overseer.Instance.IsGameReady && !NetworkManager.Instance.IsSynchronizing)
        {
            if (input.InputPattern > 0)
            {
                PlayerInputPacket packetToSend = new PlayerInputPacket();

                input.PacketId = PacketsSent;
                DataSent.Add(input);

                packetToSend.PlayerIndex = PlayerController.PlayerIndex;
                packetToSend.PacketId = PacketsSent; ;
                packetToSend.InputData = new List<PlayerInputData>(DataSent);

                SentPackets.Add(packetToSend.PacketId, GameStateManager.Instance.FrameCount);
                NetworkManager.Instance.SendEventData(NetworkManager.PlayerInputUpdate, packetToSend);
                ++PacketsSent;
            }
        }
    }

    #endregion

    #region private interface

    private void HandlePlayerInputAck(uint packetNumber)
    {
        PlayerInputData data = DataSent.Find(x => x.PacketId == packetNumber);
        if (data.IsValid())
        {
            DataSent.Remove(data);
        }

        if (SentPackets.ContainsKey(packetNumber))
        {
            ++PacketsReceivedByAck;
            long rtt = GameStateManager.Instance.FrameCount - SentPackets[packetNumber];
            AveragePing += rtt - (long)(Time.deltaTime * 1000);
            SentPackets.Remove(packetNumber);

            if (PacketsReceivedByAck >= MaxPacketsTillUpdatePing && !NetworkManager.Instance.IsSynchronizing)
            {
                OnAckReceivedThresholdReached();
            }
        }
    }

    private void OnGameReady(bool isGameReady)
    {
        if (isGameReady)
        {
            PacketsReceivedByAck = 0;
            SecondsUntilPing = 0;
            UpdatePingCoroutine = StartCoroutine(CheckForPingUpdate());
            enabled = true;
        }
        else
        {
            if (UpdatingPing)
            {
                StopCoroutine(UpdatePingCoroutine);
            }
            PacketsReceivedByAck = 0;
            SecondsUntilPing = 0;
        }
    }

    private IEnumerator CheckForPingUpdate()
    {
        UpdatingPing = true;
        while (Overseer.Instance.IsGameReady)
        {
            if (SecondsUntilPing >= MaxSecondsTillCheckPing && !NetworkManager.Instance.IsSynchronizing)
            {
                NetworkManager.Instance.PingActivePlayers();
                SecondsUntilPing = 0;
                PacketsReceivedByAck = 0;
            }
            else
            {
                SecondsUntilPing += Time.deltaTime;
            }

            yield return null;
        }
    }

    private void OnAckReceivedThresholdReached()
    {
        long pingToSet = AveragePing / MaxPacketsTillUpdatePing;
        NetworkManager.Instance.SetLocalPlayerPing(pingToSet);
        PacketsReceivedByAck = 0;
        SecondsUntilPing = 0;
    }

    #endregion
}
