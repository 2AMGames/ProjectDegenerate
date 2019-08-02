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

    public void UpdateButtonsFromInputData(PlayerInputData inputData)
    {

        if ((inputData.InputPattern & 1) == 1)
        {
            CommandInterpreter.OnButtonEventTriggered(LP_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(LP_ANIM_TRIGGER);
        }
        else
        {
            CommandInterpreter.OnButtonReleased(LP_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 1) & 1) == 1)
        {
            CommandInterpreter.OnButtonEventTriggered(MP_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(MP_ANIM_TRIGGER);
        }
        else
        {
            CommandInterpreter.OnButtonReleased(MP_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 2) & 1) == 1)
        {
            CommandInterpreter.OnButtonEventTriggered(HP_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(HP_ANIM_TRIGGER);
        }
        else
        {
            CommandInterpreter.OnButtonReleased(HP_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 3) & 1) == 1)
        {
            CommandInterpreter.OnButtonEventTriggered(LK_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(LK_ANIM_TRIGGER);
        }
        else
        {
            CommandInterpreter.OnButtonReleased(LK_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 4) & 1) == 1)
        {
            CommandInterpreter.OnButtonEventTriggered(MK_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(MK_ANIM_TRIGGER);
        }
        else
        {
            CommandInterpreter.OnButtonReleased(MK_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 5) & 1) == 1)
        {
            CommandInterpreter.OnButtonEventTriggered(HK_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(HK_ANIM_TRIGGER);
        }
        else
        {
            CommandInterpreter.OnButtonReleased(HK_ANIM_TRIGGER);
        }
    }

    public Vector2Int GetJoystickInputFromData(PlayerInputData inputData)
    {
        Vector2Int joystickVector = new Vector2Int();

        joystickVector.x -= ((inputData.InputPattern >> 6) & 1) == 1 ? 1 : 0;
        joystickVector.x += ((inputData.InputPattern >> 7) & 1) == 1 ? 1 : 0;

        joystickVector.y += ((inputData.InputPattern >> 8) & 1) == 1 ? 1 : 0;
        joystickVector.y -= ((inputData.InputPattern >> 9) & 1) == 1 ? 1 : 0;

        return joystickVector;

    }

    #endregion

    #region virtual interface

    protected abstract void UpdateButtonInput();

    protected abstract Vector2Int UpdateJoystickInput();

    protected void OnGameReady(bool isReady)
    {
        enabled = true;
    }

    #endregion

}
