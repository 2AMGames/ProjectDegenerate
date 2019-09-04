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

    private bool PacketReceived;

    private int FramesSinceLastPacketSent;

    private bool PacketSentInTime;

    private bool PacketReceivedInTime;

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
        else if (photonEvent.Code == NetworkManager.HeartbeatPacket)
        {
            int frameNumber = (int)photonEvent.CustomData;
            //HandlePacketReceived((uint)frameNumber);
            NetworkManager.Instance.SendEventData(NetworkManager.HeartbeatPacketAck, (int)GameStateManager.Instance.FrameCount);
            if (Overseer.Instance.GameStarted)
            {
                PacketReceived = true;
            }
            RestartGameIfNeeded();
            if (GameStateManager.Instance.FrameCount > frameNumber + NetworkManager.Instance.NetworkDelayFrames)
            {
                uint FramesToWait = GameStateManager.Instance.FrameCount - (uint)(frameNumber + NetworkManager.Instance.NetworkDelayFrames);
                if (FramesToWait > 1)
                {
                    Overseer.Instance.DelayGame(FramesToWait);
                }
            }
            PacketReceivedInTime = true;
        }
        else if (photonEvent.Code == NetworkManager.PlayerInputUpdate)
        {
            PlayerInputPacket data = photonEvent.CustomData as PlayerInputPacket;
            if (data != null && data.PlayerIndex == PlayerController.PlayerIndex && data.InputData.Count > 0)
            {
                //HandlePacketReceived(data.InputData[0].FrameNumber);
            }
            if (Overseer.Instance.GameStarted)
            {
                PacketReceived = true;
            }
            RestartGameIfNeeded();
            PacketReceivedInTime = true;
        }
        else if (photonEvent.Code == NetworkManager.HeartbeatPacketAck)
        {
            int frameNumber = (int)photonEvent.CustomData;
            HandleHeartbeatAckReceived((uint)frameNumber);
            RestartGameIfNeeded();
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

                packetToSend.PlayerIndex = PlayerController.PlayerIndex;
                packetToSend.PacketId = PacketsSent;
                packetToSend.InputData = new List<PlayerInputData>(DataSent);

                SentPackets.Add(packetToSend.PacketId, GameStateManager.Instance.FrameCount);
                NetworkManager.Instance.SendEventData(NetworkManager.PlayerInputUpdate, packetToSend);
                ++PacketsSent;
                PacketSentInTime = true;
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
                Debug.LogError("Latency: " + latency);
                Debug.LogError("Took too long for frame: " + SentPackets[packetNumber]);
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
            if (FramesTillCheckHeartbeat < 1)
            {
                if (!PacketReceivedInTime)
                {
                    HeartbeatTimerExpired();
                }
                ResetFrameWaitTime();
                PacketReceivedInTime = false;
            }

            if (FramesTillSendHeartbeat < 1)
            {
                if (!PacketSentInTime)
                {
                    SendHeartbeat();
                }
                PacketSentInTime = false;
                ResetFramesTillSendHeartbeat();
            }
            else
            {
                --FramesTillSendHeartbeat;
            }

            if (!PacketReceived)
            {
                --FramesTillCheckHeartbeat;
            }
            PacketReceived = false;
            yield return null;
        }
    }

    private void ResetFrameWaitTime()
    {
        FramesTillCheckHeartbeat = NetworkManager.Instance.TotalDelayFrames;
    }

    private void ResetFramesTillSendHeartbeat()
    {
        FramesTillSendHeartbeat = NetworkManager.Instance.NetworkDelayFrames;
    }

    private void SendHeartbeat()
    {
        //Debug.LogError("Send heartbeat");
        NetworkManager.Instance.SendEventData(NetworkManager.HeartbeatPacket, (int)GameStateManager.Instance.FrameCount, ReceiverGroup.Others);
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
            uint frameDeficit = GameStateManager.Instance.FrameCount - (frameNumber + (uint)NetworkManager.Instance.TotalDelayFrames);
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
        //HeartbeatStopwatch.Stop();
        //RestartGameIfNeeded();
        ////float elapsedFrames = (float)HeartbeatStopwatch.ElapsedMilliseconds / MillisecondsPerFrame;
        ////uint pingFrames = (uint)Mathf.Ceil(elapsedFrames / 2.0f);
        ////uint probableFrameCount = frameNumber + pingFrames;
        ////Debug.LogError("Probable frame count: " + probableFrameCount);
        ////Debug.LogError("My frame count: " + GameStateManager.Instance.FrameCount);
        //if (probableFrameCount < GameStateManager.Instance.FrameCount)
        //{
        //    uint framesToWait = GameStateManager.Instance.FrameCount - probableFrameCount;
        //    //Debug.LogError("Delay game for: " + framesToWait);
        //    Overseer.Instance.DelayGame(framesToWait);
        //}
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
