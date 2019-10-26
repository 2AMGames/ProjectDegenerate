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
    private const int PushbackFrames = 6;

    #endregion

    #region main variables

    public Animator Animator { get; private set; }

    public CharacterStats CharacterStats { get; private set; }

    public CommandInterpreter CommandInterpreter { get; private set; }

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

    private IEnumerator HitConfirmCoroutine;

    private IEnumerator HitstunCoroutine;

    private IEnumerator PushbackCoroutine;

    #region Interaction Data

    /// <summary>
    /// True if the player can block any incoming attacks. Should be set by the animator.
    /// </summary>
    public bool CanPlayerBlock;

    // Has the last move we activated already hit a player
    public bool MoveHitPlayer;

    public int Hitstun;

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
        CharacterStats.ExecuteMove(CurrentMove);
    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        CommandInterpreter = GetComponent<CommandInterpreter>();
        MovementMechanics = GetComponent<MovementMechanics>();

        CharactersHit = new HashSet<InteractionHandler>();
        CharacterMoveDict = new Dictionary<string, MoveData>();
        foreach (MoveData move in CharacterMoves)
        {
            CharacterMoveDict.Add(move.MoveName, move);
        }
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

    public void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, MoveData moveHitBy, bool didMoveLand)
    {

        int frames = didMoveLand ? moveHitBy.OnHitFrames : moveHitBy.OnGuardFrames;
        if (frames > 0)
        {
            string triggerToSet = didMoveLand ? moveHitBy.Magnitude.ToString() : GUARD_TRIGGER;
            Animator.SetTrigger(triggerToSet);
            Animator.SetBool(HITSTUN_TRIGGER, true);

            Hitstun = frames;

            CharacterStats.OnPlayerHitByEnemy(moveHitBy, didMoveLand);

            if (HitstunCoroutine != null)
            {
                StopCoroutine(HitstunCoroutine);
            }
            if (HitConfirmCoroutine != null)
            {
                StopCoroutine(HitConfirmCoroutine);
            }

            UnityAction onPauseComplete = () =>
            {

                int direction = enemyHitbox.InteractionHandler.transform.position.x > transform.position.x ? -1 : 1;
                Vector2 destinationVelocity = didMoveLand ? moveHitBy.OnHitKnockback : moveHitBy.OnGuardKnockback;
                destinationVelocity.x *= direction;

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
                    GetComponent<CustomPhysics2D>().Velocity = destinationVelocity;
                }

                HitstunCoroutine = HandleHitstun();
                StartCoroutine(HitstunCoroutine);
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
    }

    public void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox, bool didMoveLand)
    {
        CharacterStats.OnPlayerHitEnemy(myHitbox, CurrentMove, didMoveLand);
        MoveHitPlayer = true;
        CharactersHit.Add(enemyHurtbox.InteractionHandler);

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
                Vector2 destinationVector = new Vector2(didMoveLand ? CurrentMove.OnHitKnockback.x : CurrentMove.OnGuardKnockback.x, 0);
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
        CharacterStats.OnHitstunFinished();
        Animator.SetBool(HITSTUN_TRIGGER, false);
        HitstunCoroutine = null;
    }

    private IEnumerator HandlePushback(Vector2 knockback)
    {
        int framesToPushback = PushbackFrames;
        MovementMechanics.TranslateForcedMovement(Vector3.zero, knockback, 1);
        yield return new WaitForEndOfFrame();
        while (framesToPushback > 0 && !MovementMechanics.IsInAir)
        {
            if (Overseer.Instance.IsGameReady)
            {
                MovementMechanics.TranslateForcedMovement(knockback, Vector3.zero, (float) (PushbackFrames - framesToPushback) / PushbackFrames);
                --framesToPushback;
            }
            yield return new WaitForEndOfFrame();
        }
    }
    
    #endregion

    #region structs

    [System.Serializable]
    public struct MoveData
    {
        public string MoveName;

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

        public HitHeight Height;

        public HitMagnitude Magnitude;

        public float SpecialMeterRequired;

        // Damage to apply if hit or blocked.
        public float HitDamage;
        public int OnHitFrames;
        public float HitMeterGain;
        public Vector2 OnHitKnockback;

        public float ChipDamage;
        public int OnGuardFrames;
        public float ChipMeterGain;
        public Vector2 OnGuardKnockback;
        public bool GuardBreak;

    }
    
    #endregion
}
