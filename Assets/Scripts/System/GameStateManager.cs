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
        yield return new WaitForEndOfFrame();
    }

    private GameState CreateNewGameState()
    {
        GameState NewGameState = new GameState();

        NewGameState.FrameCount = FrameCount;
        NewGameState.RoundTimeElapsed = (ushort)RoundTime;
        NewGameState.PlayerStates = new List<GameState.PlayerState>();

        foreach(PlayerController player in Overseer.Instance.Players)
        {
            GameState.PlayerState state = new GameState.PlayerState();
            CommandInterpreter interpreter = player.CommandInterpreter;
            state.PlayerPosition = player.CommandInterpreter.gameObject.transform.position;
            // TO DO: Save buttons pressed in command interpreter, translate to frame data.
        }

        return NewGameState;
    }

    #endregion


}
