using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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
    private const string PlayerKey = "_P";

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

    #region member variables

    public CharacterStats CharacterStats;

    public InteractionHandler InteractionHandler;

    public CommandInterpreter CommandInterpreter;

    [HideInInspector]
    public int PlayerIndex;

    private PlayerController Opponent
    {
        get
        {
            return Overseer.Instance.GetNextCharacterByIndex(PlayerIndex);
        }
    }

    #endregion

    #region player specific input keys

    private string LightPunchKey
    {
        get
        {
            return LP_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    private string MediumPunchKey
    {
        get
        {
            return MP_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    private string HardPunchKey
    {
        get
        {
            return HP_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    private string LightKickKey
    {
        get
        {
            return LK_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    private string MediumKickKey
    {
        get
        {
            return MK_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    private string HardKickKey
    {
        get
        {
            return HK_ANIM_TRIGGER + PlayerKey + (PlayerIndex + 1);
        }
    }

    private string HorizontalInputKey
    {
        get
        {
            return MOVEMENT_HORIZONTAL + (PlayerIndex + 1);
        }
    }

    private string VerticalInputKey
    {
        get
        {
            return MOVEMENT_VERTICAL + (PlayerIndex + 1);
        }
    }

    #endregion

    #region monobehaviour methods

    private void Update()
    {
        UpdateButtons();

        CommandInterpreter.UpdateJoystickInput(GetJoystickInputAsVector2Int());

    }

    #endregion

    #region public interface

    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
        CharacterStats.PlayerIndex = index;
        CommandInterpreter.PlayerIndex = index;
    }

    #endregion

    #region private interface

    private void UpdateButtons()
    {
        if (Input.GetButtonDown(LightPunchKey))
        {
            CommandInterpreter.OnButtonEventTriggered(LP_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(LP_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(LightPunchKey))
        {
           CommandInterpreter.OnbuttonReleasedEvent?.Invoke(LP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(MediumPunchKey))
        {
            CommandInterpreter.OnButtonEventTriggered(MP_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(MP_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(MediumPunchKey))
        {
            CommandInterpreter.OnbuttonReleasedEvent?.Invoke(MP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(HardPunchKey))
        {
            CommandInterpreter.OnButtonEventTriggered(HP_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(HP_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(HardPunchKey))
        {
            CommandInterpreter.OnbuttonReleasedEvent?.Invoke(HP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(LightKickKey))
        {
            CommandInterpreter.OnButtonEventTriggered(LK_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(LK_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(LightKickKey))
        {
            CommandInterpreter.OnbuttonReleasedEvent?.Invoke(LK_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(MediumKickKey))
        {
            CommandInterpreter.OnButtonEventTriggered(MK_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(MK_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(MediumKickKey))
        {
            CommandInterpreter.OnbuttonReleasedEvent?.Invoke(MK_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(HardKickKey))
        {
            CommandInterpreter.OnButtonEventTriggered(HK_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(HK_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(HardKickKey))
        {
            CommandInterpreter.OnbuttonReleasedEvent?.Invoke(HK_ANIM_TRIGGER);
        }
    }

    private Vector2Int GetJoystickInputAsVector2Int()
    {
        float horizontal = Input.GetAxisRaw(HorizontalInputKey);
        float vertical = Input.GetAxisRaw(VerticalInputKey);

        int horizontalInputAsInt = 0;
        int verticalInputAsInt = 0;

        if (Mathf.Abs(horizontal) > PlayerController.INPUT_THRESHOLD_RUNNING)
        {
            horizontalInputAsInt = (int)Mathf.Sign(horizontal);
        }

        if (Mathf.Abs(vertical) > PlayerController.INPUT_THRESHOLD_RUNNING)
        {
            verticalInputAsInt = (int)Mathf.Sign(vertical);
        }
        return new Vector2Int(horizontalInputAsInt, verticalInputAsInt);
    }

    #endregion

}
