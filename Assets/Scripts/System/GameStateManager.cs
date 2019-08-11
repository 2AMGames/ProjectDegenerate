using System;
using System.Collections;
using System.Collections.Generic;

using Photon.Realtime;
using UnityEngine;

using PlayerInputData = PlayerInputPacket.PlayerInputData;

// Class responsible for maintaining the gamestate
public class GameStateManager : MonoBehaviour, IOnEventCallback
{

    #region const variables

    private const int MaxStackSize = 60;

    private const int MillisecondsPerFrame = 16;

    private const int MaxPacketFrameDelay = 11;

    private const int MaxPacketMillisecondDelay = MillisecondsPerFrame * MaxPacketFrameDelay;

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

    public int FrameCount;

    public float RoundTime { get; private set; }

    private Stack<GameState> FrameStack;

    #endregion

    #region monobehaviour methods

    private void Awake()
    {
        FrameStack = new Stack<GameState>();
        Overseer.Instance.OnGameReady += OnGameReady;
    }

    private void Update()
    {
        RoundTime += Time.deltaTime;
    }

    #endregion

    #region public interface
    public GameState GetMostRecentGameStateFrame()
    {
        return FrameStack.Peek();
    }

    #endregion

    #region private interface

    private void OnGameReady(bool isGameReady)
    {
        if (isGameReady && Overseer.Instance.SelectedGameType == Overseer.GameType.PlayerVsRemote)
        {
            StartCoroutine(SaveGameState());
        }
        else
        {
            enabled = false;
        }
    }

    private GameState CreateNewGameState()
    {
        GameState NewGameState = new GameState();

        NewGameState.FrameCount = (ushort)FrameCount;
        NewGameState.RoundTimeElapsed = (ushort)RoundTime;
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

    private void EvaluateRollbackRequest(int frameRequested)
    {
        Debug.LogWarning("Evaluating rollback: " + frameRequested);
    }

    private void RollbackGameState(uint FrameCount)
    {

    }

    #endregion

    #region Coroutines

    private IEnumerator SaveGameState()
    {
        while (Overseer.Instance.IsGameReady)
        {
            if (FrameStack.Count > MaxStackSize)
            {
                FrameStack = new Stack<GameState>();
            }
            GameState gameStateToPush = CreateNewGameState();
            FrameStack.Push(gameStateToPush);
            ++FrameCount;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator TestFrameRate()
    {
        float Seconds = 15f;
        while (Seconds >= 0.0f)
        {
            Seconds -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        UnityEditor.EditorApplication.isPaused = true; 
    }

    #endregion

    #region Callbacks

    public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == NetworkManager.RollbackRequest)
        {
            EvaluateRollbackRequest((int)photonEvent.CustomData);   
        }
    }

    #endregion

}
