using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerController : PlayerController
{
    #region monobehaviour methods

    public void Update()
    {
        UpdateButtonInput();

        CommandInterpreter.UpdateJoystickInput(UpdateJoystickInput());
    }

    public void Awake()
    {
        Overseer.Instance.OnGameReady += OnGameReady;
        enabled = false;
    }

    #endregion

    #region override methods
    protected override void UpdateButtonInput()
    {
        if (Input.GetButtonDown(LightPunchKey))
        {
            CommandInterpreter.OnButtonEventTriggered(LP_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(LP_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(LightPunchKey))
        {
            CommandInterpreter.OnButtonReleased(LP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(MediumPunchKey))
        {
            CommandInterpreter.OnButtonEventTriggered(MP_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(MP_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(MediumPunchKey))
        {
            CommandInterpreter.OnButtonReleased(MP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(HardPunchKey))
        {
            CommandInterpreter.OnButtonEventTriggered(HP_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(HP_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(HardPunchKey))
        {
            CommandInterpreter.OnButtonReleased(HP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(LightKickKey))
        {
            CommandInterpreter.OnButtonEventTriggered(LK_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(LK_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(LightKickKey))
        {
            CommandInterpreter.OnButtonReleased(LK_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(MediumKickKey))
        {
            CommandInterpreter.OnButtonEventTriggered(MK_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(MK_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(MediumKickKey))
        {
            CommandInterpreter.OnButtonReleased(MK_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(HardKickKey))
        {
            CommandInterpreter.OnButtonEventTriggered(HK_ANIM_TRIGGER);
            CommandInterpreter.OnButtonPressedEvent?.Invoke(HK_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(HardKickKey))
        {
            CommandInterpreter.OnButtonReleased(HK_ANIM_TRIGGER);
        }
    }

    protected override Vector2Int UpdateJoystickInput()
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
