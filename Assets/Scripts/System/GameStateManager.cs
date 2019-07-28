using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class responsible for keeping the gamestate
public class GameStateManager : MonoBehaviour
{

    #region const variables

    private const int MaxStackSize = 500;

    #endregion

    #region main variables

    public int FrameCount
    {
        get
        {
            return Time.frameCount;
        }
    }

    public float RoundTime { get; private set; }

    private Stack<GameState> FrameStack;

    #endregion

    #region monobehaviour methods

    private void Awake()
    {
        FrameStack = new Stack<GameState>();
        StartCoroutine(SaveGameState());
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

    #endregion

    #region Coroutines

    private IEnumerator SaveGameState()
    {
        while (true)
        {
            if (FrameStack.Count > MaxStackSize)
            {
                FrameStack = new Stack<GameState>();
            }
            yield return new WaitForEndOfFrame();
            GameState gameStateToPush = CreateNewGameState();
            FrameStack.Push(gameStateToPush);
        }
    }

    private GameState CreateNewGameState()
    {
        GameState NewGameState = new GameState();

        NewGameState.FrameCount = (ushort)FrameCount;
        NewGameState.RoundTimeElapsed = (ushort)RoundTime;
        NewGameState.PlayerStates = new List<GameState.PlayerState>();

        foreach(PlayerController player in Overseer.Instance.Players)
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

    private void RollbackGameState(uint FrameCount)
    {

    }

    #endregion


}
