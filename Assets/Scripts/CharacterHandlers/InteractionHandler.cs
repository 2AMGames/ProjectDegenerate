using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This is the base class that handles any interaction with the enemy player or environment (hitboxes, projectiles, environmental traps, etc)
/// </summary>
public class InteractionHandler : MonoBehaviour
{

    #region const variables

    private const string HITSTUN_TRIGGER = "Hitstun";

    private const string GUARD_TRIGGER = "Guard";

    /// <summary>
    /// Frames 
    /// </summary>
    private const int PushbackFrames = 16;

    private const int WakeupDelayFrames = 30;

    #endregion

    #region main variables

    public Animator Animator { get; private set; }

    public CharacterStats CharacterStats { get; set; }

    public MovementMechanics MovementMechanics { get; private set; }

    public HashSet<InteractionHandler> CharactersHit { get; private set; }

    [SerializeField]
    private List<MoveData> CharacterMoves;

    private Dictionary<string, MoveData> CharacterMoveDict;

    private string AnimatorClipName
    {
        get
        {
            return Animator != null ? Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name : "idle";
        }
    }

    public MoveData CurrentMove
    {
        get
        {
            return Animator != null && CharacterMoveDict.ContainsKey(AnimatorClipName) ? CharacterMoveDict[AnimatorClipName] : default;
        }
    }

    public HitData CurrentHitFromMove
    {
        get
        {
            return CurrentMove.Hits.Length > HitIndex ? CurrentMove.Hits[HitIndex] : default;
        }
    }

    public int HitIndex;

    private IEnumerator HitConfirmCoroutine;

    private IEnumerator HitstunCoroutine;

    private IEnumerator PushbackCoroutine;

    private IEnumerator ComboTrackingCoroutine;

    private IEnumerator WakeupCoroutine;

    #region Interaction Data

    public int CurrentComboCount;

    /// <summary>
    /// True if the player can block any incoming attacks. Should be set by the animator.
    /// </summary>
    public bool CanPlayerBlock;

    /// <summary>
    /// Has the last move we activate already hit a player.
    /// </summary>
    public bool MoveHitPlayer;

    public int Hitstun;

    public bool IsKnockedDown;

    #endregion

    #endregion

    #region monobehaviour methods

    public void OnMoveBegin()
    {
        if (PushbackCoroutine != null)
        {
            StopCoroutine(PushbackCoroutine);
        }
        CharactersHit.Clear();
        MoveHitPlayer = false;
        HitIndex = 0;
        CharacterStats.ExecuteMove(CurrentMove);
    }

    /// <summary>
    /// Reset the move hit player bool, which should allow the currrent move to hit more than once.
    /// </summary>
    public void ResetMoveHit()
    {
        MoveHitPlayer = false;
    }

    /// <summary>
    /// If a move has more than one hit, increment the hit index. We should have code handle this variable, instead of having the animator set it.
    /// This is in case we want to do extra crazy stuff (ex. On hit, spend meter to reset the hit index).
    /// </summary>
    public void IncrementHitIndex()
    {
        ++HitIndex;
    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        MovementMechanics = GetComponent<MovementMechanics>();

        CharactersHit = new HashSet<InteractionHandler>();
        CharacterMoveDict = new Dictionary<string, MoveData>();
        foreach (MoveData move in CharacterMoves)
        {
            CharacterMoveDict.Add(move.MoveName, move);
        }

        CustomPhysics2D rigid = GetComponent<CustomPhysics2D>();
    }

    private void OnValidate()
    {
        CharacterMoveDict = new Dictionary<string, MoveData>();
        foreach (MoveData move in CharacterMoves)
        {
            CharacterMoveDict.Add(move.MoveName, move);
        }
    }

    #endregion

    #region public methods

    public void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, HitData hitData, bool didMoveLand)
    {
        int frames = didMoveLand ? hitData.OnHitFrames : hitData.OnGuardFrames;
        string triggerToSet = didMoveLand ? hitData.Magnitude.ToString() : GUARD_TRIGGER;
        Animator.SetTrigger(triggerToSet);
        Animator.SetBool(HITSTUN_TRIGGER, true);

        Hitstun = frames;

        CharacterStats.OnPlayerHitByEnemy(hitData, didMoveLand);

        if (HitstunCoroutine != null)
        {
            StopCoroutine(HitstunCoroutine);
        }
        if (HitConfirmCoroutine != null)
        {
            StopCoroutine(HitConfirmCoroutine);
        }
        if (WakeupCoroutine != null)
        {
            StopCoroutine(WakeupCoroutine);
        }

        UnityAction onPauseComplete = () =>
        {

            CustomPhysics2D enemyPhysics = enemyPhysics = enemyHitbox.InteractionHandler.GetComponent<CustomPhysics2D>();

            int direction = enemyHitbox.InteractionHandler.transform.position.x > transform.position.x ? -1 : 1;
            Vector2 destinationVelocity = didMoveLand ? hitData.OnHitKnockback : hitData.OnGuardKnockback;
            destinationVelocity.x *= direction;

            if (didMoveLand && hitData.Knockdown)
            {
                IsKnockedDown = true;
                Animator.SetBool("KnockedDown", true);
            }

            if (!MovementMechanics.IsInAir)
            {
                if (PushbackCoroutine != null)
                {
                    StopCoroutine(PushbackCoroutine);
                }
                PushbackCoroutine = HandlePushback(destinationVelocity);
                StartCoroutine(PushbackCoroutine);
            }
            else
            {
                MovementMechanics.TranslateForcedMovement(Vector2.zero, destinationVelocity, 1);
            }

            HitstunCoroutine = HandleHitstun();
            StartCoroutine(HitstunCoroutine);

            if (ComboTrackingCoroutine == null)
            {
                ComboTrackingCoroutine = HandleCurrentCombo(enemyHitbox.InteractionHandler);
                StartCoroutine(ComboTrackingCoroutine);
            }
        };

        if (didMoveLand)
        {
            HitConfirmCoroutine = PauseCharacterOnHit(onPauseComplete);
            StartCoroutine(HitConfirmCoroutine);
        }
        else
        {
            onPauseComplete();
        }

    }

    public void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox, bool didMoveLand)
    {
        MoveHitPlayer = true;
        CharactersHit.Add(enemyHurtbox.InteractionHandler);
        ++CurrentComboCount;
        HitData hitData = CurrentHitFromMove;
        CharacterStats.OnPlayerHitEnemy(myHitbox, CurrentHitFromMove, didMoveLand);
        if (HitConfirmCoroutine != null)
        {
            StopCoroutine(HitConfirmCoroutine);
        }

        UnityAction onPauseComplete = () =>
        {

            // Handle case where player hit an enemy that is against a wall / static collider.
            // We should push this player away in this case.
            CustomPhysics2D enemyPhysics = enemyHurtbox.InteractionHandler.GetComponent<CustomPhysics2D>();

            if (Mathf.Abs(enemyPhysics.isTouchingSide.x) > 0 && !MovementMechanics.IsInAir)
            {
                Vector2 destinationVector = new Vector2(didMoveLand ? hitData.OnHitKnockback.x : hitData.OnGuardKnockback.x, 0);
                destinationVector.x *= Mathf.Sign(enemyPhysics.isTouchingSide.x) * -1;
                PushbackCoroutine = HandlePushback(destinationVector);
                StartCoroutine(PushbackCoroutine);
            }
        };

        if (didMoveLand)
        {
            HitConfirmCoroutine = PauseCharacterOnHit(onPauseComplete);
            StartCoroutine(HitConfirmCoroutine);
        }
        else
        {
            onPauseComplete();
        }

    }

    public void OnClash(Hitbox enemyHitbox)
    {

    }

    public void OnKnockedDown()
    {
        Hitstun = 0;
        Animator.SetBool("Hitstun", false);
        WakeupCoroutine = HandleWakeup();
        StartCoroutine(WakeupCoroutine);
    }

    public void OnComboEnded()
    {
        CurrentComboCount = 0;
    }

    #endregion

    #region private methods

    private IEnumerator PauseCharacterOnHit(UnityAction onPauseComplete)
    {
        yield return new WaitForEndOfFrame();
        int framesToPause = GameStateManager.Instance.HitConfirmFrameDelay;
        while (framesToPause > 0)
        {
            yield return new WaitForEndOfFrame();
            CharacterStats.ShouldCharacterMove = false;
            framesToPause -= Overseer.Instance.IsGameReady ? 1 : 0;
        }
        CharacterStats.ShouldCharacterMove = true;
        onPauseComplete?.Invoke();
    }

    private IEnumerator HandleHitstun()
    {
        Animator.SetBool(HITSTUN_TRIGGER, true);
        while (Hitstun > 0)
        {
            MovementMechanics.ignoreJumpButton = true;
            yield return null;
            if (Overseer.Instance.IsGameReady)
            {
                --Hitstun;
            }
        }
        Animator.SetBool(HITSTUN_TRIGGER, false);
        HitstunCoroutine = null;
    }

    private IEnumerator HandlePushback(Vector2 knockback)
    {
        int framesToPushback = PushbackFrames;
        MovementMechanics.TranslateForcedMovement(Vector3.zero, knockback, 1);
        yield return new WaitForEndOfFrame();
        while (framesToPushback >= 0 && !MovementMechanics.IsInAir)
        {
            if (Overseer.Instance.IsGameReady)
            {
                MovementMechanics.TranslateForcedMovement(knockback, Vector3.zero, (float)(PushbackFrames - framesToPushback) / PushbackFrames);
                --framesToPushback;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator HandleWakeup()
    {
        int framesToWait = WakeupDelayFrames;
        yield return new WaitForEndOfFrame();
        while (framesToWait > 0)
        {
            if (Overseer.Instance.IsGameReady)
            {
                --framesToWait;
            }
            yield return new WaitForEndOfFrame();
        }
        Animator.SetBool("KnockedDown", false);
        WakeupCoroutine = null;
    }

    private IEnumerator HandleCurrentCombo(InteractionHandler handlerThatHitMe)
    {
        yield return new WaitForEndOfFrame();
        while (!CanPlayerBlock)
        {
            yield return new WaitForEndOfFrame();
        }
        StopCoroutine(PushbackCoroutine);

        CharacterStats.OnComboFinished();

        handlerThatHitMe.OnComboEnded();

        ComboTrackingCoroutine = null;
    }

    #endregion

    #region structs

    public enum HitHeight
    {
        Low,
        Mid,
        High,
        Air
    }
    public enum HitMagnitude
    {
        LightHit,
        MediumHit,
        HeavyHit,
    };

    [System.Serializable]
    /// <summary>
    /// Info about move. Contains array of hits to apply upon connecting with opposing player.
    /// </summary>
    public struct MoveData
    {
        [Header("Move Info")]
        public string MoveName;

        public float SpecialMeterRequired;

        public HitHeight Height;

        public HitData[] Hits;

    }

    [System.Serializable]
    /// <summary>
    ///  Data to read when a move makes contact.
    /// </summary>
    public struct HitData
    {

        public HitMagnitude Magnitude;
        public bool Knockdown;

        [Header("On Hit Parameters")]
        public float HitDamage;
        public int OnHitFrames;
        public float HitMeterGain;
        public Vector2 OnHitKnockback;

        [Header("On Guard Parameters")]
        public float ChipDamage;
        public int OnGuardFrames;
        public float ChipMeterGain;
        public Vector2 OnGuardKnockback;
        public bool GuardBreak;
    }
    #endregion
}
