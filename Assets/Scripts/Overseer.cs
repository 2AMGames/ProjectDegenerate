﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class Overseer : MonoBehaviour, IOnEventCallback, IInRoomCallbacks
{
    #region const variables

    private const int NumberOfPlayers = 2; // Change later?

    private const string PlayerControllerString = "PlayerController";

    #endregion

    #region static reference

    private static Overseer instance;
    public static Overseer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Overseer>();
            }
            if (instance == null)
            {
                //GameObject container = new GameObject("Overseer");
                //instance = container.AddComponent<Overseer>();
                //Debug.LogWarning("Make sure to assign GameOverseer to an instance of an object in the Hierarchy.");//if we got here, we should make sure to assign it ourselves
            }
            return instance;
        }
    }

    #endregion

    #region member variables

    private Coroutine WaitForGameReadyCoroutine;

    public GameObject[] PlayerObjects;

    public List<PlayerController> Players;

    public HitboxManager HitboxManager;

    public PhysicsManager ColliderManager;

    public GameType SelectedGameType;

    public bool IsGameReady
    {
        get
        {
            bool isReady = true;
            if (SelectedGameType == GameType.PlayerVsRemote)
            {
                isReady &= PhotonNetwork.IsConnected && !string.IsNullOrEmpty(NetworkManager.Instance.CurrentRoomId) && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
            }
            isReady &= Players.Count >= 2;
            return isReady;
        }
    }

    private bool NetworkedGameReady;

    #endregion

    #region Events

    public UnityAction<bool> OnGameReady;

    #endregion

    #region monobehaviour methods

    void Awake()
    {
        instance = this;
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        CreateGameType();
    }

    #endregion

    #region public interface

    public PlayerController GetCharacterByIndex(int index)
    {
        if (index >= 0 && index < Players.Count)
        {
            return Players[index];
        }
        return null;
    }

    public PlayerController GetNextCharacterByIndex(int index)
    {
        int indexToGet = (index + 1) % Players.Count;
        return Players[indexToGet];
    }

    public void CreateLocalPlayer(int playerIndex)
    {
        CreatePlayerController(playerIndex, PlayerController.PlayerType.Local);
    }

    public void CreateRemotePlayer(int playerIndex)
    {
        CreatePlayerController(playerIndex, PlayerController.PlayerType.Remote);
    }

    public void CreateAIPlayer(int playerIndex)
    {
        CreatePlayerController(playerIndex, PlayerController.PlayerType.AI);
    }

    #endregion

    #region private interface

    private void CreateGameType()
    {
        if (SelectedGameType == GameType.PlayerVsPlayer)
        {
            for (int index = 0; index < 2; ++index)
            {
                CreateLocalPlayer(index);
            }
            OnGameReady?.Invoke(true);
        }
        else if (SelectedGameType == GameType.PlayerVsAI)
        {
          
        }
        else if (SelectedGameType == GameType.PlayerVsRemote)
        {
            WaitForGameReadyCoroutine = StartCoroutine(WaitUntilNetworkedGameReady());
        }
    }

    private void CreatePlayerController(int playerIndex, PlayerController.PlayerType playerType)
    {
        GameObject associatedPlayer = PlayerObjects[playerIndex];
        GameObject playerControllerGameObject = new GameObject();
        PlayerController playerController;
        switch (playerType)
        {
            case PlayerController.PlayerType.Local:
                playerController = playerControllerGameObject.AddComponent<LocalPlayerController>();
                playerController.PlayerIndex = playerIndex;
                break;
            case PlayerController.PlayerType.Remote:
                playerController = playerControllerGameObject.AddComponent<RemotePlayerController>();
                playerController.PlayerIndex = playerIndex;
                break;
            case PlayerController.PlayerType.AI:
                playerController = playerControllerGameObject.AddComponent<LocalPlayerController>();
                playerController.PlayerIndex = playerIndex;
                // TO DO : Replace with AI Controller
                break;
            default:
                playerController = playerControllerGameObject.AddComponent<LocalPlayerController>();
                break;
        }

        playerController.CommandInterpreter = associatedPlayer.GetComponent<CommandInterpreter>();
        playerController.InteractionHandler = associatedPlayer.GetComponent<InteractionHandler>();
        playerController.CharacterStats = associatedPlayer.GetComponent<CharacterStats>();

        if (SelectedGameType == GameType.PlayerVsRemote)
        {
            playerController.gameObject.AddComponent<NetworkInputHandler>();
        }

        if (Players.Count > playerIndex + 1)
        {
            Players[playerIndex] = playerController;
        }
        else
        {
            Players.Add(playerController);
        }

        playerControllerGameObject.transform.parent = this.gameObject.transform;
        playerControllerGameObject.name = PlayerControllerString + (playerIndex + 1);
    }

    #endregion

    #region Enum

    public enum GameType
    {
        PlayerVsPlayer,
        PlayerVsRemote,
        PlayerVsAI
    }

    #endregion

    #region EventCallbacks

    // On photon event received callback
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == NetworkManager.RemotePlayerReady)
        {
            NetworkManager.Instance.SendEventData(NetworkManager.RemotePlayerReadyAck, 0x00);
        }
        else if (photonEvent.Code == NetworkManager.RemotePlayerReadyAck)
        {
            NetworkedGameReady = true;
        }
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom != null && Players.Count <= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            // ActorNumber, from what i can tell, is the order in which the room was joined.
            // Might need to be changed later.
            CreateRemotePlayer(newPlayer.ActorNumber - 1);
            Players[newPlayer.ActorNumber - 1].AssociatedPlayer = newPlayer;
        }
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

    #region Photon Room Methods

    public void HandleJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values.OrderBy((x) => x.ActorNumber))
            {
                Debug.LogWarning("Number: " + player.ActorNumber);
                if (player != PhotonNetwork.LocalPlayer)
                {
                    CreateRemotePlayer(player.ActorNumber - 1);
                }
                else
                {
                    CreateLocalPlayer(player.ActorNumber - 1);
                }
                Players[player.ActorNumber - 1].AssociatedPlayer = player;
            }
        }
    }

    private void SendGameReadyMessage()
    {
        Debug.LogWarning("Sending game ready message");
        NetworkManager.Instance.SendEventData(NetworkManager.RemotePlayerReady, 0x00);
    }

    private IEnumerator WaitUntilNetworkedGameReady()
    {
        while (!IsGameReady)
        {
            yield return new WaitForEndOfFrame();
        }

        SendGameReadyMessage();
        
        while(!NetworkedGameReady)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.LogWarning("Game ready");
        OnGameReady?.Invoke(true);
    }

    #endregion
}
