using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    #region main variables
    /// <summary>
    /// Movement Mechanics Reference
    /// </summary>
    public MovementMechanics MovementMechanics { get; private set; }

    /// <summary>
    /// Character State
    /// </summary>
    public CharacterState currentCharacterState { get; set; }

    public Animator Anim;

    [HideInInspector]
    public int PlayerIndex;

    public float Health { get; private set; }

    public float SpecialMeter { get; private set; }

    #endregion

    #region monobehaviour methods

    private void Awake()
    {
        MovementMechanics = GetComponent<MovementMechanics>();
        Anim = GetComponent<Animator>();

        Health = 100f;
        SpecialMeter = 0f;
    }

    #endregion

    #region public interface

    // DidMoveHit == False: Move was blocked
    public void OnPlayerHitByEnemy(InteractionHandler.MoveData move, bool didMoveHit)
    {

        if (Overseer.Instance.IsGameReady)
        {
            float healthToDeduct = didMoveHit ? move.HitDamage : move.ChipDamage;
            Health -= healthToDeduct;
        }
    }

    public void OnPlayerHitEnemy(Hitbox myHitbox, InteractionHandler.MoveData move, bool didMoveHit)
    {
        if (Overseer.Instance.IsGameReady)
        {
            float meterToAdd = didMoveHit ? move.HitMeter : move.ChipMeter;
            SpecialMeter += meterToAdd;
        }
    }

    public void OnClash(Hitbox myHitbox, Hitbox enemyHitbox,InteractionHandler.MoveData move)
    {
        print("Clash");
    }

    #endregion

    #region private interface

    #endregion

}
