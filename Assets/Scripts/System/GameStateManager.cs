using System;
using System.Collections;
using System.Collections.Generic;

using Photon.Realtime;
using UnityEngine;

using PlayerInputData = PlayerInputPacket.PlayerInputData;

// Class responsible for maintaining the gamestate
public class GameStateManager : MonoBehaviour
{

    #region const variables

    private const int MaxStackSize = 60;

    private const short MillisecondsPerFrame = 16;

    private const short MaxPacketFrameDelay = 30;

    #endregion

    #region static reference

    private static GameStateManager instance;

    public static GameStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameStateManager>();
            }
            if (instance == null)
            {
                instance = new GameStateManager();
            }
            return instance;
        }
    }

    #endregion

    #region main variables

    public short LocalFrameDelay = 3;

    public uint FrameCount;

    public float RoundTime { get; private set; }

    private Stack<GameState> FrameStack;

    private bool SaveGameCoroutineRunning;

    private Coroutine SaveGameCoroutine;

    #endregion

    #region monobehaviour methods

    private void Awake()
    {
        FrameStack = new Stack<GameState>();
        LocalFrameDelay = Math.Min(MaxPacketFrameDelay, LocalFrameDelay);
        Overseer.Instance.OnGameReady += OnGameReady;
    }

    private void Update()
    {

    }

    private void OnValidate()
    {
        LocalFrameDelay = Math.Min(MaxPacketFrameDelay, LocalFrameDelay);
    }

    #endregion

    #region public interface
    public void RequestRollback(uint frameToRollbackTo)
    {
        StopSaveGameCoroutine();
        StartCoroutine(EvaluateRollbackRequest(frameToRollbackTo));
    }

    #endregion

    #region private interface

    private void OnGameReady(bool isGameReady)
    {
        enabled = isGameReady;
        if (isGameReady)
        {
            StartSaveGameCoroutine();
        }
        else
        {
            StopSaveGameCoroutine();
        }
    }

    private GameState CreateNewGameState()
    {
        GameState NewGameState = new GameState();

        NewGameState.FrameCount = (ushort)FrameCount;
        NewGameState.RoundTime = (ushort)RoundTime;
        NewGameState.PlayerStates = new List<GameState.PlayerState>();

        foreach (PlayerController player in Overseer.Instance.Players)
        {
            GameState.PlayerState state = new GameState.PlayerState();
            CommandInterpreter interpreter = player.CommandInterpreter;

            state.PlayerPosition = player.CommandInterpreter.gameObject.transform.position;
            state.PlayerIndex = player.PlayerIndex;

            PlayerInputData inputData = new PlayerInputData();
            inputData.FrameNumber = FrameCount;
            inputData.InputPattern = interpreter.GetPlayerInputByte();
        }

        return NewGameState;
    }

    private IEnumerator EvaluateRollbackRequest(uint frameRequested)
    {
        Debug.LogWarning("Evaluating rollback: " + frameRequested);
        GameState targetGameState = null;
        while (FrameStack.Count > 0)
        {
            Debug.LogWarning("Peek Game State Count: " + FrameStack.Peek().FrameCount);
            if (FrameStack.Peek().FrameCount == frameRequested)
            {
                targetGameState = FrameStack.Peek();
                break;
            }
            FrameStack.Pop();
            yield return null;
        }

        if (targetGameState != null)
        {
            RollbackGameState(targetGameState);
        }
        else
        {
            Debug.LogError("Game state to rollback to not found");
        }
    }

    private void RollbackGameState(GameState gameState)
    {
        Debug.LogWarning("Applying game state");
        foreach (GameState.PlayerState playerState in gameState.PlayerStates)
        {
            PlayerController player = Overseer.Instance.Players[playerState.PlayerIndex];
            if (player != null)
            {
                player.transform.position = playerState.PlayerPosition;
                player.CommandInterpreter.ClearPlayerInputQueue();
            }
        }
        FrameCount = gameState.FrameCount;
        RoundTime = gameState.RoundTime;
    }

    #endregion

    #region Save Game State Methods

    private IEnumerator SaveGameState()
    {
        SaveGameCoroutineRunning = true;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (Overseer.Instance.IsGameReady)
            {
                if (Application.isPlaying && Overseer.Instance.IsNetworkedMode)
                {
                    if (FrameStack.Count > MaxStackSize)
                    {
                        FrameStack = new Stack<GameState>();
                    }
                    //GameState gameStateToPush = CreateNewGameState();
                    //FrameStack.Push(gameStateToPush);
                }
                ++FrameCount;
            }

            yield return null;
        }
    }

    private void StartSaveGameCoroutine()
    {
        if (!SaveGameCoroutineRunning)
        {
            SaveGameCoroutine = StartCoroutine(SaveGameState());
            SaveGameCoroutineRunning = true;
        }
    }
    private void StopSaveGameCoroutine()
    {
        if (SaveGameCoroutineRunning)
        {
            StopCoroutine(SaveGameCoroutine);
            SaveGameCoroutineRunning = false;
        }
    }

    #endregion

    #region Callbacks

    #endregion

}
