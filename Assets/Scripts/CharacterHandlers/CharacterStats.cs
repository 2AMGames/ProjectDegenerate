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

    /// <summary>
    /// 
    /// </summary>
    public enum CharacterState
    {
        FreeMovement, //Player has full control of their movement
        NoPlayerControlledMovement,//If the player is in hit stun or some other even, this movement type may be set
        Dashing,//Player is dashing
    }
    #endregion

    #region main variables
    /// <summary>
    /// 
    /// </summary>
    public MovementMechanics MovementMechanics { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public CharacterState currentCharacterState { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Animator Anim;

    [HideInInspector]
    public int PlayerIndex;

    #endregion

    #region monobehaviour methods

    private void Awake()
    {
        MovementMechanics = GetComponent<MovementMechanics>();
        Anim = GetComponent<Animator>();
    }

    #endregion

    #region public interface

    public void OnPlayerHitByEnemy(InteractionHandler.MoveData move, bool didMoveHit)
    {
        UpdateCharacterStats(true, move);
    }

    public void OnPlayerHitEnemy(Hitbox myHitbox, InteractionHandler.MoveData move, bool didMoveHit)
    {
        UpdateCharacterStats(false, move);
    }

    public void OnClash(Hitbox myHitbox, Hitbox enemyHitbox,InteractionHandler.MoveData move)
    {
        print("Clash");
    }

    #endregion

    #region private interface

    private void UpdateCharacterStats(bool wasHit, InteractionHandler.MoveData move)
    {

    }

    #endregion

}
