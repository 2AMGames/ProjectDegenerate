using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

// Class responsible for maintaining the gamestate
public class GameStateManager : MonoBehaviour
{

    #region const variables

    private const int MaxStackSize = 60;

    private const short MillisecondsPerFrame = 16;

    private const short MaxPacketFrameDelay = 15;

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

    /// <summary>
    /// Frames to pause each character after a hit is registered.
    /// </summary>
    public int HitConfirmFrameDelay = 3;

    public short LocalFrameDelay = 2;

    /// <summary>
    /// How long each round should last. 
    /// Probably should be set by the defined game type when creating the scene.
    /// Null value means unlimited time.
    /// </summary>
    public float? RoundTimeLimit = null;

    public short RoundCount = 1;

    public short RoundCountLimit;

    public uint FrameCount;

    public float RoundTime { get; private set; }

    public bool RoundStarted { get; private set; }

    public bool RollbackEnabled;

    /// <summary>
    /// Initial state of the at the start of each round.
    /// We should set this at the start of the game and only update the round count.
    /// Use this to reset the state and positions of all characters at the beginning of each round.
    /// </summary>
    private GameState InitialRoundState;

    private Stack<GameState> FrameStack;

    private bool RunGameCoroutineRunning;

    private Coroutine RunGameCoroutine;

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
        RoundCountLimit = Math.Max((short)1, RoundCountLimit);
    }

    #endregion

    #region public interface
    public void RequestRollback(uint frameToRollbackTo)
    {
        StopRunGameCoroutine();
        StartCoroutine(EvaluateRollbackRequest(frameToRollbackTo));
    }

    public void StartGame()
    {
        InitialRoundState = CreateNewGameState();
    }

    public void PrepareNextRound()
    {
        InitialRoundState.RoundCount = ++RoundCount;
        ApplyGameState(InitialRoundState);
        foreach(PlayerController controller in Overseer.Instance.Players)
        {
            controller.CharacterStats.ResetCharacterState();
            controller.InteractionHandler.ResetInteractionHandler();
        }
    }

    public void StartRound()
    {
        RoundStarted = true;
    }

    #endregion

    #region private interface

    private void OnGameReady(bool isGameReady)
    {
        enabled = isGameReady;
        if (isGameReady)
        {
            StartRunGameCoroutine();
        }
        else
        {
            StopRunGameCoroutine();
        }
    }

    private GameState CreateNewGameState()
    {
        GameState NewGameState = new GameState();

        NewGameState.RoundCount = RoundCount;
        NewGameState.FrameCount = (ushort)FrameCount;
        NewGameState.RoundTime = (ushort)RoundTime;
        NewGameState.PlayerStates = new List<GameState.PlayerState>();

        foreach (PlayerController player in Overseer.Instance.Players)
        {
            GameState.PlayerState state = player.CharacterStats.CreatePlayerState();

            NewGameState.PlayerStates.Add(state);
        }

        return NewGameState;
    }

    private IEnumerator EvaluateRollbackRequest(uint frameRequested)
    {
        GameState targetGameState = null;
        while (FrameStack.Count > 0)
        {
            yield return new WaitForEndOfFrame();
            Debug.LogWarning("Peek Game State Count: " + FrameStack.Peek().FrameCount);
            if (FrameStack.Peek().FrameCount == frameRequested)
            {
                targetGameState = FrameStack.Peek();
                break;
            }
            FrameStack.Pop();
        }

        if (targetGameState != null)
        {
            ApplyGameState(targetGameState);
        }
        else
        {
            Debug.LogError("Game state to rollback to not found");
        }
    }

    private void ApplyGameState(GameState gameState)
    {
        foreach (GameState.PlayerState playerState in gameState.PlayerStates)
        {
            PlayerController player = Overseer.Instance.Players[playerState.PlayerIndex];
            if (player != null)
            {
                player.CharacterStats.ApplyPlayerState(playerState);
            }
        }
        FrameCount = gameState.FrameCount;
        RoundTime = gameState.RoundTime;
    }

    // Check game state for round over, game over, etc
    private void EvaluateGameState()
    {
        List<PlayerController> playersThatWon = new List<PlayerController>(Overseer.Instance.Players);
        foreach(PlayerController player in Overseer.Instance.Players)
        {
            if (player.CharacterStats.CurrentHealth <= 0)
            {
                playersThatWon.Remove(player);
            }
        }
        
        // If we haven't determined a winner when the round timer ends, choose the player with the greatest amount of health.
        if (playersThatWon.Count == Overseer.NumberOfPlayers && RoundTimeLimit != null && RoundTime >= RoundTimeLimit)
        {
            EvaluateTimeOver(playersThatWon);
        }
        if (playersThatWon.Count < Overseer.NumberOfPlayers || RoundTimeLimit != null && RoundTime >= RoundTimeLimit)
        {
            RoundStarted = false;

            EvaluateRoundWinner(playersThatWon);
        }
    }

    private void EvaluateTimeOver(List<PlayerController> playersThatWon)
    {
        float player1Health = (playersThatWon[0].CharacterStats.CurrentHealth + playersThatWon[0].CharacterStats.CurrentChipDamage) / playersThatWon[0].CharacterStats.MaxHealth;
        float player2Health = (playersThatWon[1].CharacterStats.CurrentHealth + playersThatWon[1].CharacterStats.CurrentChipDamage) / playersThatWon[1].CharacterStats.MaxHealth;
        if (player1Health.Equals(player2Health))
        {
            return;
        }
        if (player1Health > player2Health)
        {
            playersThatWon.RemoveAt(1);
        }
        else
        {
            playersThatWon.RemoveAt(0);
        }
    }

    private void EvaluateRoundWinner(List<PlayerController> roundWinners)
    {
        PlayerController matchWinner = null;
        foreach (PlayerController controller in roundWinners)
        {
            GameState.PlayerMatchStatistics matchStats = Overseer.Instance.PlayerMatchStats[controller.PlayerIndex];
            matchStats.RoundsWon += 1;
            Overseer.Instance.PlayerMatchStats[controller.PlayerIndex] = matchStats;

            if (matchStats.RoundsWon >= Mathf.RoundToInt(RoundCountLimit / 2f))
            {
                if (matchWinner != null)
                {
                    // If we've already declared a winner, the game is a draw since both players have the needed number of wins
                    Overseer.Instance.OnMatchEnd(null);
                    return;
                }
                else
                {
                    matchWinner = controller;
                }
            }
        }

        if (matchWinner != null)
        {
            Overseer.Instance.OnMatchEnd(matchWinner);
        }
        else if (RoundCount >= RoundCountLimit)
        {
            Overseer.Instance.OnMatchEnd(null);
        }
        else
        {
            Overseer.Instance.OnRoundEnd(roundWinners);
        }
    }

    #endregion

    #region Run Game Methods

    private IEnumerator RunGame()
    {
        RunGameCoroutineRunning = true;
        yield return null;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (Overseer.Instance.GameReady && RoundStarted)
            {
                if (Application.isPlaying && Overseer.Instance.IsNetworkedMode && RollbackEnabled)
                {
                    if (FrameStack.Count > MaxStackSize)
                    {
                        FrameStack = new Stack<GameState>();
                    }
                    GameState gameStateToPush = CreateNewGameState();
                    FrameStack.Push(gameStateToPush);
                }

                ++FrameCount;
                RoundTime += Overseer.DELTA_TIME;

                EvaluateGameState();
            }

            yield return null;
        }
    }

    private void StartRunGameCoroutine()
    {
        if (!RunGameCoroutineRunning)
        {
            RunGameCoroutine = StartCoroutine(RunGame());
            RunGameCoroutineRunning = true;
        }
    }
    private void StopRunGameCoroutine()
    {
        if (RunGameCoroutineRunning)
        {
            StopCoroutine(RunGameCoroutine);
            RunGameCoroutineRunning = false;
        }
    }

    #endregion

    #region Callbacks

    #endregion

}
