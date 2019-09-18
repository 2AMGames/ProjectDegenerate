﻿using System;
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

    private const int HeartbeatPingSampleCount = 120;

    #endregion

    #region main variables

    private uint PacketsSent;

    private PlayerController PlayerController;

    private CommandInterpreter CommandInterpreter;

    private List<PlayerInputData> DataSent = new List<PlayerInputData>();

    private Dictionary<uint, uint> SentPackets = new Dictionary<uint, uint>();

    private Dictionary<uint, uint> HeartbeatPacketsSent = new Dictionary<uint, uint>();

    /// <summary>
    /// If we do not receive an input ack, or an input from the other player within this time
    /// suspend the game until we know the state of the other player
    /// </summary>
    private int FramesTillCheckHeartbeat;

    /// <summary>
    /// Frames until we send a heartbeat message to the other player. Value is independent of number of frames that we last received
    /// a heartbeat message.
    /// </summary>
    private int FramesTillSendHeartbeat;

    private uint SamplesTillUpdatePing;

    private bool PlayerPacketReceived;

    /// <summary>
    /// Set to false when delaying game due to our frame count being ahead or we haven't received a heartbeat packet.
    /// </summary>
    private bool ShouldRunGame = true;

    private Stopwatch HeartbeatStopwatch;

    private uint AveragePing;

    private Coroutine UpdatePingCoroutine;

    private bool UpdatingPing;

    private bool PacketReceivedThisFrame;

    private int FramesSinceLastPacketSent;

    private bool PacketReceivedInTime;

    private uint HighestFrameCountReceived;

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
        uint frameReceived = GameStateManager.Instance.FrameCount;

        if (photonEvent.Code == NetworkManager.PlayerInputAck)
        {
            int packetNumber = (int)photonEvent.CustomData;
            HandlePlayerInputAck((uint)packetNumber);
        }
        else if (photonEvent.Code == NetworkManager.PlayerInputUpdate)
        {
            PacketReceivedInTime = true;
            if (Overseer.Instance.GameStarted)
            {
                PacketReceivedThisFrame = true;
            }

            PlayerInputPacket packet = photonEvent.CustomData as PlayerInputPacket;
            if (packet != null)
            {
                if (packet.FrameSent > HighestFrameCountReceived)
                {
                    HighestFrameCountReceived = packet.FrameSent;
                    Debug.LogError("Frame sent: " + packet.FrameSent + ", Frame Received: " + frameReceived);
                    if (frameReceived - packet.FrameSent > NetworkManager.Instance.NetworkDelayFrames)
                    {
                        int FramesToWait = (int)frameReceived - (int)(packet.FrameSent) - NetworkManager.Instance.NetworkDelayFrames;
                        if (FramesToWait >= GameStateManager.Instance.LocalFrameDelay)
                        {
                            Overseer.Instance.DelayGame(FramesToWait);
                        }
                        else
                        {
                            RestartGameIfNeeded();
                        }
                    }
                    else
                    {
                        RestartGameIfNeeded();
                    }
                }
            }
        }
    }

    #endregion

    #region public interface
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

                packetToSend.FrameSent = GameStateManager.Instance.FrameCount;
                packetToSend.PlayerIndex = PlayerController.PlayerIndex;
                packetToSend.PacketId = PacketsSent;
                packetToSend.InputData = new List<PlayerInputData>(DataSent);

                SentPackets.Add(packetToSend.PacketId, GameStateManager.Instance.FrameCount);
                NetworkManager.Instance.SendEventData(NetworkManager.PlayerInputUpdate, packetToSend, default, true);
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
            if (GameStateManager.Instance.FrameCount - SentPackets[packetNumber] >= (NetworkManager.Instance.TotalDelayFrames * 2))
            {
                uint latency = GameStateManager.Instance.FrameCount - SentPackets[packetNumber];
            }
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
        ResetFrameWaitTime();
        ResetFramesTillSendHeartbeat();
        SendHeartbeat();

        while (true)
        {
            //Debug.LogError("Frames till check: " + FramesTillCheckHeartbeat);
            //Debug.LogError("Frames till send: " + FramesTillSendHeartbeat);
            if (FramesTillSendHeartbeat < 1)
            {
                SendHeartbeat();
                ResetFramesTillSendHeartbeat();
            }
            else
            {
                --FramesTillSendHeartbeat;
            }

            yield return new WaitForEndOfFrame();

            if (FramesTillCheckHeartbeat < 1)
            {
                if (!PacketReceivedInTime)
                {
                    HeartbeatTimerExpired();
                }
                if (ShouldRunGame)
                {
                    ResetFrameWaitTime();
                }
                PacketReceivedInTime = false;
            }

            if (!PacketReceivedThisFrame)
            {
                --FramesTillCheckHeartbeat;
            }
            PacketReceivedThisFrame = false;
            yield return null;
        }
    }

    private void ResetFrameWaitTime()
    {
        // If the timer was expired, add the time we spent waiting to the next number of frames to wait.
        // This is to keep in sync with how often the other client is sending heartbeats
        int frameBuffer = Math.Abs(FramesTillCheckHeartbeat);
        FramesTillCheckHeartbeat = NetworkManager.Instance.TotalDelayFrames;
        if (frameBuffer < NetworkManager.Instance.TotalDelayFrames)
        {
            FramesTillCheckHeartbeat += frameBuffer;
        }
    }

    private void ResetFramesTillSendHeartbeat()
    {
        FramesTillSendHeartbeat = NetworkManager.Instance.NetworkDelayFrames;
    }

    private void SendHeartbeat()
    {
        PlayerInputPacket packet = new PlayerInputPacket();
        packet.FrameSent = GameStateManager.Instance.FrameCount + 1;
        if (Overseer.Instance.IsDelayingGame)
        {
            Debug.LogError("Delaying game while sending heartbeat");
        }
        packet.PacketId = PacketsSent;
        packet.PlayerIndex = PlayerController.PlayerIndex;
        packet.InputData = new List<PlayerInputData>(DataSent);
        NetworkManager.Instance.SendEventData(NetworkManager.PlayerInputUpdate, packet, ReceiverGroup.Others, true);
    }

    private void HeartbeatTimerExpired()
    {
        // Stop the game if it is not already stopped.
        if (Overseer.Instance.IsGameReady && ShouldRunGame)
        {
            Debug.LogError("Timer expired");
            Overseer.Instance.SetHeartbeatReceived(false);
        }
        ShouldRunGame = false;
    }

    private void HandlePacketReceived(uint frameNumber)
    {
        if (frameNumber + NetworkManager.Instance.TotalDelayFrames < GameStateManager.Instance.FrameCount)
        {
            int frameDeficit = (int)GameStateManager.Instance.FrameCount - (int)(frameNumber + NetworkManager.Instance.TotalDelayFrames);
            if (frameDeficit > 0 && Overseer.Instance.IsGameReady)
            {
                Overseer.Instance.DelayGame(frameDeficit);
            }
        }

    }

    private void RestartGameIfNeeded()
    {
        // If ShouldRunGame is false at this point, than we have stopped running the game to wait for a heartbeat packet.
        if (!ShouldRunGame)
        {
            Overseer.Instance.SetHeartbeatReceived(true);
        }
        ShouldRunGame = true;
    }

    private void HandleHeartbeatAckReceived(uint frameNumber)
    {

    }

    private void OnHeartbeatPingCountReached()
    {
        AveragePing = 0;
        SamplesTillUpdatePing = HeartbeatPingSampleCount;
    }

    #endregion
}
