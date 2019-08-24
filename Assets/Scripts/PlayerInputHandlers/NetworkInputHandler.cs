using System;
using System.Collections;
using System.Collections.Generic;

using ExitGames.Client.Photon;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

using Stopwatch = System.Diagnostics.Stopwatch;

using PlayerInputData = PlayerInputPacket.PlayerInputData;

public class NetworkInputHandler : MonoBehaviour, IOnEventCallback, IMatchmakingCallbacks
{

    #region const variables

    private const float MaxSecondsTillCheckPing = 10f;

    private const uint MillisecondsPerSecond = 1000;

    private const uint MillisecondsPerFrame = 16;

    private const int HeartbeatPingSampleCount = 30;

    #endregion

    #region main variables

    private uint PacketsSent;

    private PlayerController PlayerController;

    private CommandInterpreter CommandInterpreter;

    private List<PlayerInputData> DataSent = new List<PlayerInputData>();

    private Dictionary<uint, uint> SentPackets = new Dictionary<uint, uint>();

    /// <summary>
    /// If we do not receive an input ack, or an input from the other player within this time
    /// suspend the game until we know the state of the other player
    /// </summary>
    private uint FramesTillCheckHeartbeat;

    private uint SamplesTillUpdatePing;

    private bool PlayerPacketReceived;

    /// <summary>
    /// Set to false when delaying game due to our frame count being ahead or we haven't received a heartbeat packet.
    /// </summary>
    private bool ShouldRunGame;

    private Stopwatch HeartbeatStopwatch;

    private uint AveragePing;

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
        HeartbeatStopwatch = new Stopwatch();
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

        if (photonEvent.Code == NetworkManager.HeartbeatPacket)
        {
            NetworkManager.Instance.SendEventData(NetworkManager.HeartbeatPacketAck, 0x00, ReceiverGroup.Others);
            int frameNumber = (int)photonEvent.CustomData;
            HandleHeartbeatReceived((uint)frameNumber);
        }

        if (photonEvent.Code == NetworkManager.HeartbeatPacketAck)
        {
            HandleHeartbeatAckReceived();
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
            SentPackets.Remove(packetNumber);
        }
    }

    private void OnGameReady(bool isGameStarting)
    {
        if (isGameStarting)
        {
            UpdatePingCoroutine = StartCoroutine(UpdateHeartbeat());
            enabled = true;
        }
        else
        {
            if (UpdatingPing)
            {
                StopCoroutine(UpdatePingCoroutine);
                UpdatingPing = false;
            }
        }
    }

    private IEnumerator UpdateHeartbeat()
    {
        UpdatingPing = true;
        SamplesTillUpdatePing = HeartbeatPingSampleCount;
        SendHeartbeat();
        while (true)
        {
                if (FramesTillCheckHeartbeat <= 0)
                {
                    CheckHeartbeat();
                }
                else
                {
                    --FramesTillCheckHeartbeat;
                }
            yield return null;
        }
    }

    private void SendHeartbeat()
    {
        PlayerPacketReceived = false;
        FramesTillCheckHeartbeat = (uint)NetworkManager.Instance.TotalDelayFrames + 1;

        HeartbeatStopwatch.Reset();
        NetworkManager.Instance.SendEventData(NetworkManager.HeartbeatPacket, (int)GameStateManager.Instance.FrameCount, ReceiverGroup.Others);
        HeartbeatStopwatch.Start();
    }

    private void CheckHeartbeat()
    {
        if (!PlayerPacketReceived)
        {
            // Stop the game if it is not already stopped.
            if (Overseer.Instance.IsGameReady && ShouldRunGame)
            {
                Overseer.Instance.SetHeartbeatReceived(false);
            }
            ShouldRunGame = false;
        }
        else
        {
            PlayerPacketReceived = false;
        }
        SendHeartbeat(); 
    }

    private void HandleHeartbeatReceived(uint frameNumber)
    {
        if (!PlayerPacketReceived)
        {
            PlayerPacketReceived = true;

            if (frameNumber + NetworkManager.Instance.TotalDelayFrames < GameStateManager.Instance.FrameCount)
            {
                uint frameDeficit = GameStateManager.Instance.FrameCount - (frameNumber + (uint)NetworkManager.Instance.TotalDelayFrames);
                if (frameDeficit > 0 && Overseer.Instance.IsGameReady)
                {
                    Overseer.Instance.DelayGame(frameDeficit);
                }
            }
            // Should run game was previously false, so we have stopped the game state due
            if (!ShouldRunGame)
            {
                Overseer.Instance.SetHeartbeatReceived(true);
            }
            ShouldRunGame = true;
        }
    }

    private void HandleHeartbeatAckReceived()
    {
        HeartbeatStopwatch.Stop();
        if (!PlayerPacketReceived)
        {
            // Subtract the previous frame time in milliseconds to account for processing time.
            long ping = HeartbeatStopwatch.ElapsedMilliseconds - (long)(Time.unscaledDeltaTime * MillisecondsPerSecond);
            AveragePing += (uint)ping;
            --SamplesTillUpdatePing;
            if (SamplesTillUpdatePing <= 0)
            {
                OnHeartbeatPingCountReached();
            }
        }
    }

    private void OnHeartbeatPingCountReached()
    {
        //long pingToSet = AveragePing / HeartbeatPingSampleCount;
        //NetworkManager.Instance.SetLocalPlayerPing(pingToSet);

        AveragePing = 0;
        SamplesTillUpdatePing = HeartbeatPingSampleCount;
    }

    #endregion
}
