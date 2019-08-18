using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class Overseer : MonoBehaviour, IOnEventCallback, IInRoomCallbacks
{
    #region const variables

    public const int NumberOfPlayers = 2; // Change later?

    private const string PlayerControllerString = "PlayerController";

    public const float TIME_STEP = 1f / 60f;

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

    public bool IsNetworkedMode
    {
        get
        {
            return SelectedGameType == GameType.PlayerVsRemote;
        }
    }

    public bool IsGameReady
    {
        get
        {
            bool isReady = true;
            if (IsNetworkedMode)
            {
                isReady &= PhotonNetwork.IsConnected && !string.IsNullOrEmpty(NetworkManager.Instance.CurrentRoomId) && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
            }
            isReady &= Players.Count >= 2;
            return isReady;
        }
    }

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
        Application.targetFrameRate = 55;
        CreateGameType();
    }

    private void Update()
    {
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
        if (associatedPlayer == null)
        {
            Debug.LogWarning("The Associated Player is null. Perhaps no characters were added to the game");
            return;
        }
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

        if (IsNetworkedMode && playerType == PlayerController.PlayerType.Local)
        {
            playerController.gameObject.AddComponent<NetworkInputHandler>();
        }

        playerControllerGameObject.transform.parent = this.gameObject.transform;
        playerControllerGameObject.name = PlayerControllerString + (playerIndex + 1);

        if (Players.Count > playerIndex + 1)
        {
            Players[playerIndex] = playerController;
        }
        else
        {
            Players.Add(playerController);
        }
    }

    #endregion

    #region Enum

    public enum GameType
    {
        PlayerVsPlayer,
        PlayerVsRemote,
        PlayerVsAI,
        Observer
    }

    #endregion

    #region EventCallbacks

    // On photon event received callback
    public void OnEvent(EventData photonEvent)
    {

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
        foreach(PlayerController playerController in Players)
        {
            if (playerController.AssociatedPlayer == otherPlayer)
            {
                Players.Remove(playerController);
                break;
            }
        }

        if (Players.Count < NumberOfPlayers)
        {
            OnGameReady(false);
        }
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
                if (player.ActorNumber <= NumberOfPlayers)
                {
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
    }

    private IEnumerator WaitUntilNetworkedGameReady()
    {
        while (!IsGameReady)
        {
            yield return null;
        }

        NetworkManager.Instance.SetPlayerReady(true);

        NetworkManager.Instance.SynchronizeGame();

        while(NetworkManager.Instance.IsSynchronizing)
        {
            yield return null;
        }

        Debug.LogWarning("Starting game");
        yield return new WaitForEndOfFrame();
        OnGameReady(true);

    }

    public void HandleSynchronizationRequest(uint FrameToSync)
    {
        OnGameReady(false);
        StartCoroutine(SynchronizeGameState(FrameToSync));
    }

    private IEnumerator SynchronizeGameState(uint FrameToSync)
    {
        Debug.LogWarning("Synchronize game state coroutine");
        if (GameStateManager.Instance.FrameCount < FrameToSync)
        {
            Debug.LogWarning("?");
            // Run the game until we catch up to the desired frame.
            OnGameReady(true);
            while (GameStateManager.Instance.FrameCount <= FrameToSync)
            {
                Debug.LogWarning("Catch up frames: " + GameStateManager.Instance.FrameCount);
                yield return new WaitForEndOfFrame();
            }
            OnGameReady(false);
        }
        else
        {
            // Roll back the game state frames to match the intended frame.
            GameStateManager.Instance.RequestRollback(FrameToSync);
            while(GameStateManager.Instance.FrameCount > FrameToSync)
            {
                Debug.LogWarning("Rollback frames: " + GameStateManager.Instance.FrameCount);
                yield return new WaitForEndOfFrame();
            }
        }

        NetworkManager.Instance.SetPlayerReady(true);

        while (NetworkManager.Instance.IsSynchronizing)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.LogWarning("Starting game again");
        OnGameReady(true);
    }

    #endregion
}
