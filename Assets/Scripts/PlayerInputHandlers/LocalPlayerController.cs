using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerInputData = PlayerInputPacket.PlayerInputData;

public class LocalPlayerController : PlayerController
{

    #region const variables
    private const float JOYSTICK_DEADZONE = .5F;
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

        //REMOVE THIS LATER
        CustomInput.AssignPlayerIndexJoystickIndex(PlayerIndex, PlayerIndex - 1);
    }

    public void Start()
    {
        InputHandler = GetComponent<NetworkInputHandler>();
    }

    #endregion

    #region override methods
    protected override void UpdateButtonInput(ref ushort input)
    {

        input |= (ushort) IsButtonPressed(LightHitKey);
        input |= (ushort) (IsButtonPressed(MediumHitKey) << 1);
        input |= (ushort) (IsButtonPressed(HardHitKey) << 2);

        input |= (ushort) (IsButtonPressed(SpecialHitKey) << 3);
        
    }

    protected override void UpdateJoystickInput(ref ushort input)
    {
        float horizontal = HorizontalInputValue;
        float vertical = VerticalInputValue;

        input |= (ushort) ((horizontal < JOYSTICK_DEADZONE ? 1 : 0) << 6);
        input |= (ushort) ((horizontal > -JOYSTICK_DEADZONE ? 1 : 0) << 7);
        input |= (ushort) ((vertical > JOYSTICK_DEADZONE ? 1 : 0) << 8);
        input |= (ushort) ((vertical < -JOYSTICK_DEADZONE ? 1 : 0) << 9);
    }

    private int IsButtonPressed(KeyCode playerKeycode)
    {
        return Input.GetKey(playerKeycode) ? 1 : 0;
    }

    #endregion
}
