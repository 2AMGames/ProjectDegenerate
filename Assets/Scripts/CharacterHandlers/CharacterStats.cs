using System.Collections;
using UnityEngine;
using UnityEngine.Events;


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

    #endregion event functions

    #region const variables

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

    #endregion

    #region public interface

    public void ExecuteMove(CharacterInteractionHandler.MoveData move)
    {
        if (string.IsNullOrEmpty(move.MoveName))
            return;

        SpecialMeter = Mathf.Max(0f, SpecialMeter - move.SpecialMeterRequired);

        Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));
        OnMoveExecuted.Invoke();

    }

    // DidMoveHit == False: Move was blocked
    public void OnPlayerHitByEnemy(CharacterInteractionHandler.HitData hitData, bool didMoveHit)
    {
        if (Overseer.Instance.IsGameReady)
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

    public void OnPlayerHitEnemy(Hitbox myHitbox, CharacterInteractionHandler.HitData hit, bool didMoveHit)
    {
        if (Overseer.Instance.IsGameReady)
        {
            float meterToAdd = didMoveHit ? hit.HitMeterGain : hit.ChipMeterGain;
            SpecialMeter = Mathf.Min(MaxSpecialMeter, SpecialMeter + meterToAdd);

            OnMoveHit.Invoke();

            Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));
        }
    }

    public void OnClash(Hitbox myHitbox, Hitbox enemyHitbox,CharacterInteractionHandler.HitData hit)
    {
        print("Clash");

        Anim.SetInteger(SpecialMeterParameter, (int)(SpecialMeter / SpecialMeterStockCount));
    }

    public void OnComboFinished()
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
