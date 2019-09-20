using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerInputData = PlayerInputPacket.PlayerInputData;

public class LocalPlayerController : PlayerController
{

    #region const variables

    private const ushort DefaultInputPattern = 0xF000;

    #endregion
    #region main variables

    private ushort LastSavedInputPattern;

    private NetworkInputHandler InputHandler;

    #endregion

    #region monobehaviour methods

    public void Update()
    {
        if (Overseer.Instance.IsGameReady)
        {
            PlayerInputData currentFrameInputData = new PlayerInputData();
            currentFrameInputData.FrameNumber = GameStateManager.Instance.FrameCount;
            currentFrameInputData.PlayerIndex = PlayerIndex;
            currentFrameInputData.InputPattern = DefaultInputPattern;

            // Properties cannot be passed by reference, so create a local variable
            ushort inputPattern = currentFrameInputData.InputPattern;

            UpdateButtonInput(ref inputPattern);
            UpdateJoystickInput(ref inputPattern);
            currentFrameInputData.InputPattern = inputPattern;

            bool addDataToInputHandlerList = currentFrameInputData.InputPattern != LastSavedInputPattern;
            CommandInterpreter.QueuePlayerInput(currentFrameInputData);
            LastSavedInputPattern = currentFrameInputData.InputPattern;
            if (InputHandler != null)
            {
                InputHandler.SendInput(currentFrameInputData, addDataToInputHandlerList);
            }
        }
        else if (Overseer.Instance.HasGameStarted && InputHandler != null)
        {
                InputHandler.SendHeartbeat();
        }
    }

    public void Awake()
    {
        enabled = false;
        InputHandler = GetComponent<NetworkInputHandler>();
        Overseer.Instance.OnGameReady += OnGameReady;
    }

    public void Start()
    {
        InputHandler = GetComponent<NetworkInputHandler>();
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
