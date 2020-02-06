using System.Collections;
using System.Collections.Generic;

using ExitGames.Client.Photon;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

using PlayerInputData = PlayerInputPacket.PlayerInputData;

public class NetworkInputHandler : MonoBehaviour, IOnEventCallback
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

    private List<PlayerInputData> DataSent = new List<PlayerInputData>();

    /// <summary>
    /// If we do not receive an input ack, or an input from the other player within this time
    /// suspend the game until we know the state of the other player
    /// </summary>
    private uint FramesTillWait;

    /// <summary>
    /// Set to false when delaying game due to our frame count being ahead or we haven't received a heartbeat packet.
    /// </summary>
    private bool ShouldRunGame = true;

    private Coroutine UpdatePingCoroutine;

    private uint HighestFrameCountReceived;

    #endregion

    #region monobehaviour methods

    void Awake()
    {
        PlayerController = GetComponent<PlayerController>();
        Overseer.Instance.OnGameReady += OnGameReady;
        PhotonNetwork.AddCallbackTarget(this);

        Random.InitState(System.DateTime.Now.Second);
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
        else if (photonEvent.Code == NetworkManager.PlayerInputUpdate)
        {

            PlayerInputPacket packet = photonEvent.CustomData as PlayerInputPacket;
            if (packet != null)
            {
                if (packet.FrameSent > HighestFrameCountReceived)
                {
                    HighestFrameCountReceived = packet.FrameSent;
                    uint newFrameLimit = packet.FrameSent + (uint)NetworkManager.Instance.TotalDelayFrames;
                    ResetFrameWaitTime(newFrameLimit);
                }
            }
        }
    }

    #endregion

    #region public interface

    public void ResetHandler()
    {
        PacketsSent = 0;
        DataSent.Clear();
    }

    public void SendInput(PlayerInputData input, bool addDataToList)
    {
        // If we are currently synchronizing the game state by catching up to the highest frame, do not send off any inputs.
        if (Overseer.Instance.GameReady && !NetworkManager.Instance.IsSynchronizing)
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
            if (rand <= NetworkManager.Instance.SendPercentage)
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
        if (rand <= NetworkManager.Instance.SendPercentage)
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
        else if (UpdatePingCoroutine != null)
        {
            StopCoroutine(UpdatePingCoroutine);
            UpdatePingCoroutine = null;
        }
    }

    private IEnumerator UpdateHeartbeat()
    {
        ResetFrameWaitTime((uint)NetworkManager.Instance.TotalDelayFrames);
        yield return null;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (FramesTillWait <= GameStateManager.Instance.FrameCount)
            {
                OnFrameLimitReached();
            }
            else if (!ShouldRunGame && FramesTillWait > GameStateManager.Instance.FrameCount)
            {
                Debug.LogWarning("Restart Frame Received: " + FramesTillWait);
                Overseer.Instance.SetShouldRunGame(true);
                ShouldRunGame = true;
            }
        }

    }

    private void ResetFrameWaitTime(uint frameLimit)
    {
        FramesTillWait = frameLimit;
    }

    private void OnFrameLimitReached()
    {
        // Stop the game if it is not already stopped.
        if (Overseer.Instance.GameReady && ShouldRunGame)
        {
            Debug.LogError("Frame limit reached at: " + GameStateManager.Instance.FrameCount + ", Last received frame: " + FramesTillWait);
            Overseer.Instance.SetShouldRunGame(false);
            ShouldRunGame = false;
        }
    }

    #endregion
}
