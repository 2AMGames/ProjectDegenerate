using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{

    #region const variables

    public const string MOVEMENT_HORIZONTAL = "Horizontal_P";
    public const string MOVEMENT_VERTICAL = "Vertical_P";
    public const string MOVEMENT_JUMP = "Jump_";
    public const float INPUT_THRESHOLD_RUNNING = .6f;

    /// <summary>
    /// Player Key. Append this to the end of an input key to get the specific player that pressed the button.
    /// Ex. LP_P1 = Player one light punch
    /// </summary>
    protected const string PlayerKey = "_P";

    /// <summary>
    /// Light Punch trigger
    /// </summary>
    public const string LP_ANIM_TRIGGER = "LP";
    /// <summary>
    /// Medium Punch trigger
    /// </summary>
    public const string MP_ANIM_TRIGGER = "MP";

    /// <summary>
    /// Heavy punch trigger
    /// </summary>
    public const string HP_ANIM_TRIGGER = "HP";

    /// <summary>
    /// Light Kick Trigger
    /// </summary>
    public const string LK_ANIM_TRIGGER = "LK";

    /// <summary>
    /// Medium Kick Trigger
    /// </summary>
    public const string MK_ANIM_TRIGGER = "MK";

    /// <summary>
    /// Heavy Kick trigger
    /// </summary>
    public const string HK_ANIM_TRIGGER = "HK";

    #endregion

    #region enum

    public enum PlayerType
    {
        Local,
        Remote,
        AI
    }

    #endregion

    #region member variables

    private PlayerController Opponent
    {
        get
        {
            return Overseer.Instance.GetNextCharacterByIndex(PlayerIndex);
        }
    }

    public Player AssociatedPlayer;

    public CharacterStats CharacterStats;

    public InteractionHandler InteractionHandler;

    public CommandInterpreter CommandInterpreter;

    public int PlayerIndex;

    #endregion

    #region player specific input keys

    protected string LightPunchKey
    {
        get
        {
            return LP_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    protected string MediumPunchKey
    {
        get
        {
            return MP_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    protected string HardPunchKey
    {
        get
        {
            return HP_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    protected string LightKickKey
    {
        get
        {
            return LK_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    protected string MediumKickKey
    {
        get
        {
            return MK_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    protected string HardKickKey
    {
        get
        {
            return HK_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    protected string HorizontalInputKey
    {
        get
        {
            return MOVEMENT_HORIZONTAL + (PlayerIndex + 1);
        }
    }

    protected string VerticalInputKey
    {
        get
        {
            return MOVEMENT_VERTICAL + (PlayerIndex + 1);
        }
    }

    #endregion

    #region public interface

    public void SetPhotonPlayer(Player player)
    {
        AssociatedPlayer = player;
    }

    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
        CharacterStats.PlayerIndex = index;
    }

    #endregion

    #region virtual interface

    protected virtual void UpdateButtonInput(ref ushort inputPattern)
    {

    }

    protected virtual void UpdateJoystickInput(ref ushort inputPattern)
    {

    }

    protected void OnGameReady(bool isReady)
    {
        enabled = true;
    }

    #endregion

}
