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

    #endregion

    #region main variables

    public MovementMechanics MovementMechanics { get; private set; }

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
