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

    private const long MillisecondsPerSecond = 1000;

    private const long MillisecondsPerFrame = 16;

    private const long HeartbeatPingSampleCount = 30;

    #endregion

    #region main variables

    private uint PacketsSent;

    private PlayerController PlayerController;

    private CommandInterpreter CommandInterpreter;

    private List<PlayerInputData> DataSent = new List<PlayerInputData>();

    private Dictionary<uint, long> SentPackets = new Dictionary<uint, long>();
    
    private long HeartbeatInterval
    {
        get
        {
            return NetworkManager.Instance.TotalDelayFrames + 1;
        }
    }

    /// <summary>
    /// If we do not receive an input ack, or an input from the other player within this time
    /// suspend the game until we know the state of the other player
    /// </summary>
    private long FramesTillCheckHeartbeat;

    private long SamplesTillUpdatePing;

    private bool HeartbeatReceived;

    private bool ShouldRunGame;

    private Stopwatch HeartbeatStopwatch;

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
        HeartbeatReceived = false;
        FramesTillCheckHeartbeat = NetworkManager.Instance.TotalDelayFrames * 3;

        HeartbeatStopwatch.Reset();
        Debug.LogWarning("Send heartbeat");
        NetworkManager.Instance.SendEventData(NetworkManager.HeartbeatPacket, (int)GameStateManager.Instance.FrameCount, ReceiverGroup.Others);
        HeartbeatStopwatch.Start();
    }

    private void CheckHeartbeat()
    {
        if (!HeartbeatReceived)
        {
            Debug.LogWarning("Heartbeat not received in time");
            HeartbeatReceived = false;
            if (ShouldRunGame)
            {
                Overseer.Instance.SetHeartbeatReceived(false);
            }
            ShouldRunGame = false;
        }
        else
        {
            HeartbeatReceived = false;
            FramesTillCheckHeartbeat = NetworkManager.Instance.TotalDelayFrames * 3;
        }
        SendHeartbeat(); 
    }

    private void HandleHeartbeatReceived(uint frameNumber)
    {
        Debug.LogWarning("Heartbeat received");
        if (frameNumber >= GameStateManager.Instance.FrameCount + NetworkManager.Instance.TotalDelayFrames)
        {
            //TODO: Catch up to game state frame number, but don't queue inputs from local player
        }
        else if (frameNumber + NetworkManager.Instance.TotalDelayFrames < GameStateManager.Instance.FrameCount)
        {
            long frameDeficit = GameStateManager.Instance.FrameCount - (frameNumber + NetworkManager.Instance.TotalDelayFrames);
            if (frameDeficit > 0 && Overseer.Instance.IsGameReady)
            {
                Overseer.Instance.DelayGame(frameDeficit);
            }
        }
        if (!ShouldRunGame)
        {
            Debug.LogWarning("Should run game");
            Overseer.Instance.SetHeartbeatReceived(true);
            ShouldRunGame = true;
        }
    }

    private void HandleHeartbeatAckReceived()
    {
        HeartbeatStopwatch.Stop();
        if (!HeartbeatReceived)
        {
            HeartbeatReceived = true;
            // Subtract the previous frame time in milliseconds to account for processing time.
            long ping = HeartbeatStopwatch.ElapsedMilliseconds - (long)(Time.unscaledDeltaTime * MillisecondsPerSecond);
            AveragePing += ping;
            --SamplesTillUpdatePing;
            if (SamplesTillUpdatePing <= 0)
            {
                OnHeartbeatPingCountReached();
            }
        }
    }

    private void OnHeartbeatPingCountReached()
    {
        long pingToSet = AveragePing / HeartbeatPingSampleCount;
        NetworkManager.Instance.SetLocalPlayerPing(pingToSet);

        AveragePing = 0;
        SamplesTillUpdatePing = HeartbeatPingSampleCount;
    }

    #endregion
}
