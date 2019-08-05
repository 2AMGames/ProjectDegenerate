﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerController : PlayerController
{
    #region main variables

    public ushort LastSavedInputPattern;

    #endregion

    #region monobehaviour methods

    public void Update()
    {
        PlayerInputData currentFrameInputData = new PlayerInputData();
        currentFrameInputData.FrameNumber =(uint)GameStateManager.Instance.FrameCount;
        currentFrameInputData.PlayerIndex = PlayerIndex;

        UpdateButtonInput(ref currentFrameInputData.InputPattern);
        UpdateJoystickInput(ref currentFrameInputData.InputPattern);

        if (currentFrameInputData.InputPattern != LastSavedInputPattern)
        {
            CommandInterpreter.QueuePlayerInput(currentFrameInputData);
            LastSavedInputPattern = currentFrameInputData.InputPattern;
        }
    }

    public void Awake()
    {
        enabled = false;
        Overseer.Instance.OnGameReady += OnGameReady;
    }

    #endregion

    #region override methods
    protected override void UpdateButtonInput(ref ushort input)
    {

        input |= (ushort) IsButtonPressed(LightPunchKey);
        input |= (ushort) (IsButtonPressed(MediumPunchKey) << 1);
        input |= (ushort) (IsButtonPressed(HardPunchKey) << 2);

        input |= (ushort) (IsButtonPressed(LightKickKey) << 3);
        input |= (ushort) (IsButtonPressed(MediumKickKey) << 4);
        input |= (ushort) (IsButtonPressed(HardKickKey) << 5);
    }

    protected override void UpdateJoystickInput(ref ushort input)
    {
        float horizontal = Input.GetAxisRaw(HorizontalInputKey);
        float vertical = Input.GetAxisRaw(VerticalInputKey);

        input |= (ushort) ((horizontal < 0f ? 1 : 0) << 6);
        input |= (ushort) ((horizontal > 0f ? 1 : 0) << 7);
        input |= (ushort) ((vertical > 0f ? 1 : 0) << 8);
        input |= (ushort) ((vertical < 0f ? 1 : 0) << 9);
    }

    private int IsButtonPressed(string buttonTrigger)
    {
        return Input.GetButton(buttonTrigger) ? 1 : 0;
    }

    #endregion
}
