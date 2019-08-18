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

    private const long MaxPacketMillisecondDelay = MillisecondsPerFrame * MaxPacketFrameDelay;

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

    public long FrameCount;

    public float RoundTime { get; private set; }

    private Stack<GameState> FrameStack;

    private bool SaveGameCoroutineRunning;

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
        StopCoroutine(SaveGameState());
        StartCoroutine(EvaluateRollbackRequest(frameToRollbackTo));
    }

    #endregion

    #region private interface

    private void OnGameReady(bool isGameReady)
    {
        enabled = isGameReady;
        if (isGameReady)
        {
            if (!SaveGameCoroutineRunning)
            {
                StartCoroutine(SaveGameState());
            }
        }
        else
        {
            StopCoroutine(SaveGameState());
            SaveGameCoroutineRunning = false;
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
            inputData.FrameNumber = (ushort)FrameCount;
            inputData.InputPattern = interpreter.GetPlayerInputByte();
        }

        return NewGameState;
    }

    private IEnumerator EvaluateRollbackRequest(uint frameRequested)
    {
        Debug.LogWarning("Evaluating rollback: " + frameRequested);
        GameState targetGameState = null;
        while(FrameStack.Count > 0)
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

    #region Coroutines

    private IEnumerator SaveGameState()
    {
        SaveGameCoroutineRunning = true;
        while (Overseer.Instance.IsGameReady)
        { 
            if (FrameStack.Count > MaxStackSize)
            {
                FrameStack = new Stack<GameState>();
            }

            // We only want to save the gamestate if we need to roll back (Network mode).
            if (Application.isPlaying && Overseer.Instance.IsNetworkedMode)
            {
                GameState gameStateToPush = CreateNewGameState();
                FrameStack.Push(gameStateToPush);
                ++FrameCount;
            }

            yield return null;
        }
    }

    #endregion

    #region Callbacks

    #endregion

}
