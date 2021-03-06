﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This is the base class that handles any interaction with the enemy player or environment (hitboxes, projectiles, environmental traps, etc)
/// </summary>
public class CharacterInteractionHandler : InteractionHandler
{

    #region const variables

    public readonly int WasHitTrigger = Animator.StringToHash("WasHit");

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

    public int CurrentComboCount { get; set; }

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
        Animator.ResetTrigger(DID_MOVE_LAND);
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
        SetCharacterMoveDictionary(true);
    }

    private void OnValidate()
    {
        SetCharacterMoveDictionary(false);
    }

    private void SetCharacterMoveDictionary(bool addDictionary)
    {
        for(int index = 0; index < CharacterMoves.Count; ++index)
        {
            MoveData move = CharacterMoves[index];
            if (move.AnimationClip != null)
            {
                move.MoveName = move.AnimationClip.name;
                if (addDictionary)
                {
                    CharacterMoveDict.Add(move.MoveName, move);
                }
                CharacterMoves[index] = move;
            }
        }
    }

    public void OnKnockedDown()
    {
        Hitstun = 0;
        Animator.SetBool(HitstunTrigger, false);
        Animator.SetBool(WasHitTrigger, false);
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
        Animator.SetBool(WasHitTrigger, false);
        Animator.SetBool(KnockdownKey, false);
        StopInteractionCoroutine(WakeupCoroutine);
        StopInteractionCoroutine(HitstunCoroutine);
        StopInteractionCoroutine(PushbackCoroutine);
    }

    #endregion

    #region public methods

    public override void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, HitData hitData, bool didMoveLand)
    {
        int frames = didMoveLand ? hitData.OnHitFrames : hitData.OnGuardFrames;
        HitType type = hitData.HitType;
        if (frames > 0 || hitData.HitType == HitType.Knockdown)
        {

            AssociatedCharacterStats.OnPlayerHitByEnemy(hitData, didMoveLand);

            if (didMoveLand)
            {
                IsKnockedDown = (didMoveLand && (type == HitType.Knockdown || type == HitType.Crumple))|| AssociatedCharacterStats.CurrentHealth <= 0;

                Animator.SetBool(KnockdownKey, IsKnockedDown);
                Animator.SetInteger(HitTypeKey, (int)hitData.HitType);
            }
            else
            {
                Animator.SetTrigger(GUARD_TRIGGER);
                Animator.SetInteger(HitTypeKey, -1);
            }

            Animator.SetBool(HitstunTrigger, true);
            Animator.SetTrigger(WasHitTrigger);

            float hitPosition = enemyHitbox.InteractionHandler.transform.position.x;
            int direction = hitPosition > transform.position.x ? -1 : 1;
            Vector2 destinationVelocity = didMoveLand ? hitData.OnHitKnockback : hitData.OnGuardKnockback;
            destinationVelocity.x *= direction;
            if (type == HitType.AirCombo && didMoveLand && MovementMechanics.IsInAir && enemyHitbox.AssociatedCharacterStats.MovementMechanics.IsInAir)
            {
                destinationVelocity = enemyHitbox.AssociatedCharacterStats.MovementMechanics.GetVelocity();
            }
            MovementMechanics.TranslateForcedMovement(Vector2.zero, destinationVelocity, 1);

            Hitstun = frames;
            if (HitConfirmCoroutine != null)
            {
                StopCoroutine(HitConfirmCoroutine);
            }
            if (WakeupCoroutine != null)
            {
                StopCoroutine(WakeupCoroutine);
            }

            MovementMechanics.FlipSpriteBasedOnOpponentDirection(enemyHitbox.AssociatedCharacterStats.transform, true);

            UnityAction onPauseComplete = () =>
            {

                bool forceMovement = MovementMechanics.IsInAir || type == HitType.Launch || type == HitType.Knockdown || type == HitType.HitToWall;

                if (!forceMovement)
                {
                    if (PushbackCoroutine != null)
                    {
                        StopCoroutine(PushbackCoroutine);
                    }
                    PushbackCoroutine = HandlePushback(destinationVelocity);
                    StartCoroutine(PushbackCoroutine);
                }

                if (HitstunCoroutine == null)
                {
                    HitstunCoroutine = HandleHitstun();
                    StartCoroutine(HitstunCoroutine);
                }

                if (ComboTrackingCoroutine == null)
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

        CustomPhysics2D enemyPhysics = enemyHurtbox.InteractionHandler.GetComponent<CustomPhysics2D>();
        Vector2 destinationVector = Vector2.zero;
        if (Mathf.Abs(enemyPhysics.isTouchingSide.x) > 0 && !MovementMechanics.IsInAir)
        {
            destinationVector = new Vector2(didMoveLand ? hitData.OnHitKnockback.x : hitData.OnGuardKnockback.x, 0);
            destinationVector.x *= Mathf.Sign(enemyPhysics.isTouchingSide.x) * -1;
        }
        MovementMechanics.TranslateForcedMovement(Vector2.zero, destinationVector, 1);

        UnityAction onPauseComplete = () =>
        {
            // Handle case where player hit an enemy that is against a wall / static collider.
            // We should push this player away in this case.
            if (Mathf.Abs(enemyPhysics.isTouchingSide.x) > 0 && !MovementMechanics.IsInAir)
            {
                PushbackCoroutine = HandlePushback(destinationVector);
                StartCoroutine(PushbackCoroutine);
            }
        };

        if (didMoveLand)
        {
            TriggerDidMoveLandInAnimator();
            HitConfirmCoroutine = PauseHandlerOnHit(onPauseComplete);
            StartCoroutine(HitConfirmCoroutine);
        }
        else
        {
            onPauseComplete();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void TriggerDidMoveLandInAnimator()
    {
        Animator.SetTrigger(DID_MOVE_LAND);
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
        AssociatedCharacterStats.SetCharacterShouldMove(false);

        while (framesToPause > 0)
        {
            yield return new WaitForEndOfFrame();
            framesToPause -= Overseer.Instance.GameReady ? 1 : 0;
        }

        AssociatedCharacterStats.SetCharacterShouldMove(true);
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
        Animator.SetBool(WasHitTrigger, false);
        Animator.SetInteger(HitTypeKey, -1);
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
        while (Hitstun > 0)
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
