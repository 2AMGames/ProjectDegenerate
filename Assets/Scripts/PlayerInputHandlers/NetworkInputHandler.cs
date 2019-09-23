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

    private Dictionary<uint, uint> HeartbeatPacketsSent = new Dictionary<uint, uint>();

    /// <summary>
    /// If we do not receive an input ack, or an input from the other player within this time
    /// suspend the game until we know the state of the other player
    /// </summary>
    private uint FramesTillWait;

    private uint FrameReceivedFromPlayerOnUpdate;

    private bool PlayerPacketReceived;

    /// <summary>
    /// Set to false when delaying game due to our frame count being ahead or we haven't received a heartbeat packet.
    /// </summary>
    private bool ShouldRunGame = true;

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
        Overseer.Instance.OnGameReady += OnGameReady;
        PhotonNetwork.AddCallbackTarget(this);

        Random.InitState(System.DateTime.Now.Second);
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
            if (Overseer.Instance.HasGameStarted)
            {
                PacketReceivedThisFrame = true;
            }

            PlayerInputPacket packet = photonEvent.CustomData as PlayerInputPacket;
            if (packet != null)
            {
                if (packet.FrameSent > HighestFrameCountReceived)
                {
                    HighestFrameCountReceived = packet.FrameSent;
                    FrameReceivedFromPlayerOnUpdate = packet.FrameSent;
                    uint highestFrame = FrameReceivedFromPlayerOnUpdate + (uint)NetworkManager.Instance.TotalDelayFrames;
                    ResetFrameWaitTime(highestFrame);
                }
            }
        }
    }

    #endregion

    #region public interface
    public void SendInput(PlayerInputData input, bool addDataToList)
    {
        // If we are currently synchronizing the game state by catching up to the highest frame, do not send off any inputs.
        if (Overseer.Instance.IsGameReady && !NetworkManager.Instance.IsSynchronizing)
        {
            PlayerInputPacket packetToSend = new PlayerInputPacket();

            packetToSend.FrameSent = GameStateManager.Instance.FrameCount;
            packetToSend.PlayerIndex = PlayerController.PlayerIndex;
            if (addDataToList)
            {
                input.PacketId = PacketsSent;
                DataSent.Add(input);
            }
            packetToSend.InputData = DataSent;

            ++PacketsSent;

            float rand = Random.Range(0.0f, 1.0f);
            if (rand >= NetworkManager.Instance.SendPercentage)
            {
                NetworkManager.Instance.SendEventData(NetworkManager.PlayerInputUpdate, packetToSend, ReceiverGroup.Others, true);
            }

        }
    }

    public void SendHeartbeat()
    {
        PlayerInputPacket packet = new PlayerInputPacket();
        packet.FrameSent = GameStateManager.Instance.FrameCount;
        packet.PlayerIndex = PlayerController.PlayerIndex;
        packet.InputData = DataSent;
        float rand = Random.Range(0.0f, 1.0f);
        if (rand >= NetworkManager.Instance.SendPercentage)
        {
            NetworkManager.Instance.SendEventData(NetworkManager.PlayerInputUpdate, packet, ReceiverGroup.Others, true);
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
        ResetFrameWaitTime((uint)NetworkManager.Instance.TotalDelayFrames);
        ResetFramesTillSendHeartbeat();
        yield return null;
        while (true)
        {
            uint currentFrame = GameStateManager.Instance.FrameCount;
            yield return new WaitForEndOfFrame();
            //Debug.LogWarning("Frame limit: " + FramesTillWait + ", Frame count: " + GameStateManager.Instance.FrameCount + ", Frame received " + FrameReceivedFromPlayerOnUpdate);
            if (FramesTillWait <= GameStateManager.Instance.FrameCount)
            {
                OnFrameLimitReached();
                PacketReceivedInTime = false;
            }
            else if (FrameReceivedFromPlayerOnUpdate + NetworkManager.Instance.NetworkDelayFrames >= currentFrame)
            {
                if (!ShouldRunGame && FramesTillWait > GameStateManager.Instance.FrameCount)
                {
                    Debug.LogError("Restart Frame Received: " + FrameReceivedFromPlayerOnUpdate);
                    Overseer.Instance.SetShouldRunGame(true);
                    ShouldRunGame = true;
                }
            }
            PacketReceivedThisFrame = false;
            yield return null;
        }
    }

    private void ResetFrameWaitTime(uint frameLimit)
    {
        FramesTillWait = frameLimit;
    }

    private void ResetFramesTillSendHeartbeat()
    {
    }

    private void OnFrameLimitReached()
    {
        // Stop the game if it is not already stopped.
        if (Overseer.Instance.IsGameReady && ShouldRunGame)
        {
            Debug.LogError("Frame limit reached at: " + GameStateManager.Instance.FrameCount + ", Last received frame: " + FrameReceivedFromPlayerOnUpdate);
            Overseer.Instance.SetShouldRunGame(false);
            ShouldRunGame = false;
        }
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
            Overseer.Instance.SetShouldRunGame(true);
        }
        ShouldRunGame = true;
    }

    private void HandleHeartbeatAckReceived(uint frameNumber)
    {

    }

    private void OnHeartbeatPingCountReached()
    {
        AveragePing = 0;
    }

    #endregion
}
