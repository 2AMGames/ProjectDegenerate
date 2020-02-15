using System.Collections;
using UnityEngine;
using UnityEngine.Events;


using MoveData = InteractionHandler.MoveData;
using HitData = InteractionHandler.HitData;

/// <summary>
/// A reference to all important variables in our character. Such as the 
/// </summary>
public class CharacterStats : MonoBehaviour
{
    #region enum

    /// <summary>
    /// 
    /// </summary>
    public enum CharacterTeam
    {
        PLAYER_1,
        PLAYER_2,
        NEUTRAL,
    }

    // Shouldn't this be in movement mechanics or the animator?
    public enum CharacterState
    {
        FreeMovement, //Player has full control of their movement
        NoPlayerControlledMovement,//If the player is in hit stun or some other even, this movement type may be set
        Dashing,//Player is dashing
    }
    #endregion

    #region event functions

    [System.NonSerialized]
    public UnityEvent OnCharacterHealthChanged = new UnityEvent();

    [System.NonSerialized]
    public UnityEvent OnMoveExecuted = new UnityEvent();

    [System.NonSerialized]
    public UnityEvent OnMoveHit = new UnityEvent();

    [System.NonSerialized]
    public UnityEvent OnComboFinished = new UnityEvent();

    #endregion event functions

    #region const variables

    public static readonly int Win_State = Animator.StringToHash("WinState");

    private const string SpecialMeterParameter = "SpecialMeter";

    /// <summary>
    /// Special meter needed per "stock"
    /// </summary>
    public const float SpecialMeterStockCount = 100f;

    /// <summary>
    /// Delay before starting chip damage recovery.
    /// </summary>
    private const float ChipDamageRecoveryDelay = 2f;

    /// <summary>
    /// Chip damage to recover per second.
    /// </summary>
    private const float ChipDamageRecoveryRate = 1.5f;

    #endregion

    #region fields

    /// <summary>
    /// Movement Mechanics Reference
    /// </summary>
    public MovementMechanics MovementMechanics { get; private set; }

    public CommandInterpreter CommandInterpreter { get; private set;}

    /// <summary>
    /// Character State
    /// </summary>
    public CharacterState currentCharacterState { get; set; }

    private Animator Anim;

    [HideInInspector]
    public int PlayerIndex;

    #endregion

    #region main variables

    [System.NonSerialized]
    /// <summary>
    /// If we are pausing the game due to the character being hit, this should be false.
    /// </summary>
    public bool ShouldCharacterMove = true;

    private IEnumerator ChipDamageCoroutine;

    #endregion

    #region Health Variables

    public float TotalHealth {
        get
        {
            return CurrentHealth - CurrentChipDamage - ComboDamage;
        }
    }

    public float CurrentHealth { get; private set; }

    public float MaxHealth = 100;

    public float CurrentChipDamage { get; private set; }

    public float ComboDamage { get; private set; }

    #endregion

    #region Special Meter Variables

    public float MaxSpecialMeter = 300f;

    public float SpecialMeter;

    #endregion

    #region monobehaviour methods

    private void Awake()
    {
        MovementMechanics = GetComponent<MovementMechanics>();
        CommandInterpreter = GetComponent<CommandInterpreter>();
        Anim = GetComponent<Animator>();

        CurrentHealth = MaxHealth;

        SpecialMeter = MaxSpecialMeter / 2f;
        Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));

        OnCharacterHealthChanged.Invoke();
    }

    private void OnValidate()
    {
        SpecialMeter = Mathf.Max(0f, SpecialMeter);
        if (Anim)
        {
            Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));
        }
        OnCharacterHealthChanged.Invoke();
    }

    private void Update()
    {
        Anim.SetBool("IsCrouching", MovementMechanics.IsCrouching);
    }

    #endregion

    #region public interface

    public void ExecuteMove(MoveData move)
    {
        if (string.IsNullOrEmpty(move.MoveName))
            return;

        SpecialMeter = Mathf.Max(0f, SpecialMeter - move.SpecialMeterRequired);

        Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));
        OnMoveExecuted.Invoke();

    }

    // DidMoveHit == False: Move was blocked
    public void OnPlayerHitByEnemy(HitData hitData, bool didMoveHit)
    {
        if (Overseer.Instance.GameReady)
        {
            if (ChipDamageCoroutine != null)
            {
                StopCoroutine(ChipDamageCoroutine);
            }
            
            if (didMoveHit)
            {
                // "Cash In" built up chip damage that has not been recovered yet.
                CurrentHealth -= CurrentChipDamage;

                ComboDamage += hitData.HitDamage;
                CurrentHealth -= hitData.HitDamage;

                CurrentChipDamage = 0;
            }
            else
            {
                CurrentChipDamage += hitData.ChipDamage;
            }

            OnCharacterHealthChanged.Invoke();

            Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));
        }
    }

    public void OnPlayerHitEnemy(Hitbox myHitbox, HitData hit, bool didMoveHit)
    {
        if (Overseer.Instance.GameReady)
        {
            float meterToAdd = didMoveHit ? hit.HitMeterGain : hit.ChipMeterGain;
            SpecialMeter = Mathf.Min(MaxSpecialMeter, SpecialMeter + meterToAdd);

            OnMoveHit.Invoke();

            Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));
        }
    }

    public void OnClash(Hitbox myHitbox, Hitbox enemyHitbox,HitData hit)
    {
        print("Clash");

        Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));
    }

    public void ComboFinished()
    {
        ComboDamage = 0;

        if (ChipDamageCoroutine != null)
        {
            StopCoroutine(ChipDamageCoroutine);
        }

        if (CurrentChipDamage > 0)
        {
            ChipDamageCoroutine = RecoverChipDamage();
            StartCoroutine(ChipDamageCoroutine);
        }
        OnCharacterHealthChanged.Invoke();
    }

    public void ApplyPlayerState(GameState.PlayerState playerState)
    {
        this.transform.position = playerState.PlayerPosition;

        CurrentHealth = playerState.Health;
        CurrentChipDamage = playerState.ChipDamage;
        ComboDamage = playerState.ComboDamage;
        SpecialMeter = playerState.SpecialMeter;

        Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));

        OnCharacterHealthChanged.Invoke();
        OnMoveExecuted.Invoke();

        CommandInterpreter.ClearPlayerInputQueue();

    }

    public GameState.PlayerState CreatePlayerState()
    {
        GameState.PlayerState newPlayerState = new GameState.PlayerState();
        newPlayerState.PlayerIndex = PlayerIndex;

        newPlayerState.PlayerPosition = this.transform.position;
        newPlayerState.Health = CurrentHealth;
        newPlayerState.ChipDamage = CurrentChipDamage;
        newPlayerState.ComboDamage = ComboDamage;
        newPlayerState.SpecialMeter = SpecialMeter;

        newPlayerState.InputData = new PlayerInputPacket.PlayerInputData();
        newPlayerState.InputData.InputPattern = CommandInterpreter.GetPlayerInputByte();

        return newPlayerState;
    }

    public void ResetCharacterState()
    {

        // Cycling the gameobject on and off resets the animator.
        gameObject.SetActive(false);
        gameObject.SetActive(true);

        MovementMechanics.TranslateForcedMovement(Vector2.zero, Vector2.zero, 1);
        CommandInterpreter.ResetInterpreter();
        Anim.SetInteger(Win_State, -1);

        gameObject.GetComponent<AnimationSpeedController>().Start();
    }

    public void OnPlayerWin(bool matchWon)
    {
        Anim.SetInteger(Win_State, matchWon ? 1 : 0);
    }

    #endregion

    #region private interface

    private IEnumerator RecoverChipDamage()
    {

        float secondsToWait = ChipDamageRecoveryDelay;
        
        while (secondsToWait > 0.0f)
        {
            yield return new WaitForEndOfFrame();
            secondsToWait -= Overseer.DELTA_TIME;
        }

        while (CurrentChipDamage > 0.0f)
        {
            float newChipDamage = CurrentChipDamage - (Overseer.DELTA_TIME * ChipDamageRecoveryRate);
            CurrentChipDamage = Mathf.Max(newChipDamage, 0.0f);
            OnCharacterHealthChanged.Invoke();
            yield return new WaitForEndOfFrame();
        }
    }

    #endregion

}
