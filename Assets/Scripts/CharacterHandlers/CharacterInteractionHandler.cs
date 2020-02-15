using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This is the base class that handles any interaction with the enemy player or environment (hitboxes, projectiles, environmental traps, etc)
/// </summary>
public class CharacterInteractionHandler : InteractionHandler
{

    #region const variables

    private const int DefaultWakeupDelayFrames = 15;

    #endregion

    #region main variables

    public MovementMechanics MovementMechanics { get; private set; }

    [SerializeField]
    private List<MoveData> CharacterMoves = null;

    private Dictionary<string, MoveData> CharacterMoveDict;

    public override MoveData CurrentMove
    {
        get
        {
            return Animator != null && CharacterMoveDict.ContainsKey(AnimatorClipName) ? CharacterMoveDict[AnimatorClipName] : default;
        }
    }

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

    public int Hitstun;

    public bool IsKnockedDown;

    #endregion

    #endregion

    #region monobehaviour methods

    public override void OnMoveBegin()
    {
        base.OnMoveBegin();
        StopInteractionCoroutine(PushbackCoroutine);
        Animator.SetBool(DID_MOVE_LAND, false);
        if (!MovementMechanics.IsInAir)
        {
            MovementMechanics.TranslateForcedMovement(Vector3.zero, Vector3.zero, 0);
        }
        AssociatedCharacterStats.ExecuteMove(CurrentMove);
    }

    public override void Awake()
    {
        base.Awake();

        AssociatedCharacterStats = GetComponent<CharacterStats>();
        MovementMechanics = GetComponent<MovementMechanics>();
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

    public void OnKnockedDown()
    {
        Hitstun = 0;
        Animator.SetBool(HitstunTrigger, false);
        Animator.SetBool(KnockdownKey, true);
        if (AssociatedCharacterStats.CurrentHealth >= 0)
        {
            WakeupCoroutine = HandleWakeup();
            StartCoroutine(WakeupCoroutine);
        }
    }

    public void OnRecovery()
    {
        Hitstun = 0;
        Animator.SetBool(HitstunTrigger, false);
        Animator.SetBool(KnockdownKey, true);
        StopInteractionCoroutine(WakeupCoroutine);
        StopInteractionCoroutine(HitstunCoroutine);
        StopInteractionCoroutine(PushbackCoroutine);
    }

    #endregion

    #region public methods

    public override void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, HitData hitData, HitType height, bool didMoveLand)
    {
        int frames = didMoveLand ? hitData.OnHitFrames : hitData.OnGuardFrames;
        if (frames > 0 || hitData.Knockdown)
        {

            AssociatedCharacterStats.OnPlayerHitByEnemy(hitData, didMoveLand);

            if (didMoveLand)
            {
                IsKnockedDown = (didMoveLand && (hitData.Knockdown || height == HitType.Crumple)) || AssociatedCharacterStats.CurrentHealth <= 0;

                Animator.SetBool(KnockdownKey, IsKnockedDown);
                Animator.SetInteger(HitHeightKey, (int)height);

            }
            else
            {
                Animator.SetTrigger(GUARD_TRIGGER);
                Animator.SetInteger(HitHeightKey, -1);
            }

            Animator.SetBool(HitstunTrigger, true);
            Hitstun = frames;

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

            MovementMechanics.FlipSpriteBasedOnOpponentDirection(enemyHitbox.AssociatedCharacterStats.transform, true);
            float hitPosition = enemyHitbox.InteractionHandler.transform.position.x;

            UnityAction onPauseComplete = () =>
            {
                int direction = hitPosition > transform.position.x ? -1 : 1;
                Vector2 destinationVelocity = didMoveLand ? hitData.OnHitKnockback : hitData.OnGuardKnockback;
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
                    MovementMechanics.TranslateForcedMovement(Vector2.zero, destinationVelocity, 1);
                }

                HitstunCoroutine = HandleHitstun();
                StartCoroutine(HitstunCoroutine);

                if (ComboTrackingCoroutine == null && didMoveLand)
                {
                    InteractionHandler handler = enemyHitbox.InteractionHandler;
                    if (handler is ProjectileInteractionHandler)
                    {
                        handler = handler.AssociatedCharacterStats.gameObject.GetComponent<InteractionHandler>();
                    }
                    ComboTrackingCoroutine = HandleCurrentCombo(handler);
                    StartCoroutine(ComboTrackingCoroutine);
                }
            };

            if (didMoveLand)
            {
                HitConfirmCoroutine = PauseHandlerOnHit(onPauseComplete);
                StartCoroutine(HitConfirmCoroutine);
            }
            else
            {
                onPauseComplete();
            }
        }
    }

    public override void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox, bool didMoveLand)
    {
        base.OnHitEnemy(myHitbox, enemyHurtbox, didMoveLand);
        CurrentComboCount += didMoveLand ? 1 : 0;

        HitData hitData = CurrentHitFromMove;
        AssociatedCharacterStats.OnPlayerHitEnemy(myHitbox, CurrentHitFromMove, didMoveLand);

        if (HitConfirmCoroutine != null)
        {
            StopCoroutine(HitConfirmCoroutine);
        }

        if (PushbackCoroutine != null)
        {
            StopCoroutine(PushbackCoroutine);
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
            Animator.SetTrigger(DID_MOVE_LAND);
            HitConfirmCoroutine = PauseHandlerOnHit(onPauseComplete);
            StartCoroutine(HitConfirmCoroutine);
        }
        else
        {
            onPauseComplete();
        }
    }

    public override void OnClash(Hitbox enemyHitbox)
    {

    }

    public override void OnComboEnded()
    {
        AssociatedCharacterStats.OnComboFinished.Invoke();
        CurrentComboCount = 0;
    }

    public override void ResetInteractionHandler()
    {
        base.ResetInteractionHandler();
        StopInteractionCoroutine(HitConfirmCoroutine);
        StopInteractionCoroutine(HitstunCoroutine);
        StopInteractionCoroutine(PushbackCoroutine);
        StopInteractionCoroutine(ComboTrackingCoroutine);
        StopInteractionCoroutine(WakeupCoroutine);

    }

    #endregion

    #region private methods

    protected override IEnumerator PauseHandlerOnHit(UnityAction onPauseComplete)
    {
        yield return new WaitForEndOfFrame();
        int framesToPause = GameStateManager.Instance.HitConfirmFrameDelay;

        while (framesToPause > 0)
        {
            yield return new WaitForEndOfFrame();
            AssociatedCharacterStats.ShouldCharacterMove = false;
            framesToPause -= Overseer.Instance.GameReady ? 1 : 0;
        }
        AssociatedCharacterStats.ShouldCharacterMove = true;
        onPauseComplete?.Invoke();
    }

    private IEnumerator HandleHitstun()
    {
        Animator.SetBool(HitstunTrigger, true);
        while (Hitstun > 0)
        {
            yield return null;
            if (Overseer.Instance.GameReady)
            {
                --Hitstun;
            }
        }
        Animator.SetBool(HitstunTrigger, false);
        Animator.SetInteger(HitHeightKey, -1);
        HitstunCoroutine = null;
    }
    private IEnumerator HandlePushback(Vector2 knockback)
    {
        int framesToPushback = PushbackFrames - 1;
        MovementMechanics.TranslateForcedMovement(Vector3.zero, knockback, 1);
        yield return new WaitForEndOfFrame();
        while (framesToPushback >= 0 && !MovementMechanics.IsInAir)
        {
            if (Overseer.Instance.GameReady)
            {
                MovementMechanics.TranslateForcedMovement(knockback, Vector3.zero, (float)(PushbackFrames - framesToPushback) / PushbackFrames);
                --framesToPushback;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator HandleWakeup()
    {
        int framesToWait = DefaultWakeupDelayFrames;
        while (framesToWait >= 0)
        {
            if (Overseer.Instance.GameReady)
            {
                --framesToWait;
            }
            yield return new WaitForEndOfFrame();
        }
        Animator.SetBool(KnockdownKey, false);
        WakeupCoroutine = null;
    }

    private IEnumerator HandleCurrentCombo(InteractionHandler handlerThatHitMe)
    {
        yield return new WaitForEndOfFrame();
        while (!CanPlayerBlock)
        {
            yield return new WaitForEndOfFrame();
        }
        if (PushbackCoroutine != null)
        {
            StopCoroutine(PushbackCoroutine);
        }

        AssociatedCharacterStats.ComboFinished();

        handlerThatHitMe.OnComboEnded();

        ComboTrackingCoroutine = null;
    }

    #endregion
}
