using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Custom Input Manager. This allows players to set their own button controls for their character
/// </summary>
public static class CustomInput
{
    #region const variables
    private const string LightHitID = "LP_BUTTON";
    private const string MediumHitID = "MP_BUTTON";
    private const string HeavyHitID = "HP_BUTTON";
    private const string SpecialHitID = "SP_BUTTON";

    private const string KeyboardKey = "_Keyboard";
    private static readonly string LightHitID_Keyboard = LightHitID + KeyboardKey;
    private static readonly string MediumHitID_Keyboard = MediumHitID + KeyboardKey;
    private static readonly string HeavyHitID_Keyboard = HeavyHitID + KeyboardKey;
    private static readonly string SpecialHitID_Keyboard = SpecialHitID + KeyboardKey;
    private const string VerticalNeg_Keyboard = "VERTICAL_NEG_KEYBOARD";
    private const string VerticalPos_Keybaord = "VERTICAL_POS_KEYBOARD";
    private const string HorizontallNeg_Keyboard = "HORIZONTAL_NEG_KEYBOARD";
    private const string HorizontalPos_Keybaord = "HORIZONTAL_POS_KEYBOARD";

    private const string Player1Key = "_Player1";
    private static readonly string LightHitID_Player1 = LightHitID + Player1Key;
    private static readonly string MediumHitID_Player1 = MediumHitID + Player1Key;
    private static readonly string HeavyHitID_Player1 = HeavyHitID + Player1Key;
    private static readonly string SpecialHitID_Player1 = SpecialHitID + Player1Key;

    private const string Player2Key = "_Player2";
    private static readonly string LightHitID_Player2 = LightHitID + Player2Key;
    private static readonly string MediumeHitID_Player2 = MediumHitID + Player2Key;
    private static readonly string HeavyHitID_Player2 = HeavyHitID + Player2Key;
    private static readonly string SpecialHitID_Player2 = SpecialHitID + Player2Key;

    #endregion const variables

    #region static variables


    public static KeyCode LightHitKey_Player1
    {
        get
        {
            return InGamePlayerInputDictionary[LightHitID_Player1];
        }
        set
        {
            InGamePlayerInputDictionary[LightHitID_Player1] = value;
        }
    }
    public static KeyCode MediumKey_Player1
    {
        get
        {
            return InGamePlayerInputDictionary[MediumHitID_Player1];
        }
        set
        {
            InGamePlayerInputDictionary[MediumHitID_Player1] = value;
        }
    }
    public static KeyCode HeavyKey_Player1
    {
        get
        {
            return InGamePlayerInputDictionary[HeavyHitID_Player1];
        }
        set
        {
            InGamePlayerInputDictionary[HeavyHitID_Player1] = value;
        }
    }
    public static KeyCode SpecialKey_Player1
    {
        get
        {
            return InGamePlayerInputDictionary[SpecialHitID_Player1];
        }
        set
        {
            InGamePlayerInputDictionary[SpecialHitID_Player1] = value;
        }
    }


    public static KeyCode LightHitKey_Player2
    {
        get
        {
            return InGamePlayerInputDictionary[LightHitID_Player2];
        }
        set
        {
            InGamePlayerInputDictionary[LightHitID_Player2] = value;
        }
    }
    public static KeyCode MediumKey_Player2
    {
        get
        {
            return InGamePlayerInputDictionary[MediumeHitID_Player2];
        }
        set
        {
            InGamePlayerInputDictionary[LightHitID_Player2] = value;
        }
    }
    public static KeyCode HeavyKey_Player2
    {
        get
        {
            return InGamePlayerInputDictionary[HeavyHitID_Player2];
        }
        set
        {
            InGamePlayerInputDictionary[LightHitID_Player2] = value;
        }
    }
    public static KeyCode SpecialKey_Player2
    {
        get
        {
            return InGamePlayerInputDictionary[SpecialHitID_Player2];
        }
        set
        {
            InGamePlayerInputDictionary[LightHitID_Player2] = value;
        }
    }


    private static int[] JoystickIndexArray = new int[] {-1, 0 };

    /// <summary>
    /// This will contain a list of all the inputs that will be used while you are match
    /// </summary>
    private static Dictionary<string, KeyCode> InGamePlayerInputDictionary = new Dictionary<string, KeyCode>()
    {
        [LightHitID_Player1] = KeyCode.JoystickButton2,
        [MediumHitID_Player1] = KeyCode.JoystickButton3,
        [HeavyHitID_Player1] = KeyCode.JoystickButton1,
        [SpecialHitID_Player1] = KeyCode.JoystickButton0,

        [LightHitID_Player2] = KeyCode.JoystickButton2,
        [MediumeHitID_Player2] = KeyCode.JoystickButton3,
        [HeavyHitID_Player2] = KeyCode.JoystickButton1,
        [SpecialHitID_Player2] = KeyCode.JoystickButton0,
    };

    /// <summary>
    /// This will contain a list of all the input values that we will use for our game.
    /// </summary>
    private static Dictionary<string, KeyCode> PersistedInputDictioanry = new Dictionary<string, KeyCode>()
    {
        [LightHitID_Keyboard] = KeyCode.Y,
        [MediumHitID_Keyboard] = KeyCode.U,
        [HeavyHitID_Keyboard] = KeyCode.I,
        [SpecialHitID_Keyboard] = KeyCode.H,
        [VerticalNeg_Keyboard] = KeyCode.S,
        [VerticalPos_Keybaord] = KeyCode.W,
        [HorizontalPos_Keybaord] = KeyCode.D,
        [HorizontallNeg_Keyboard] = KeyCode.A,

        [LightHitID_Player1] = KeyCode.JoystickButton2,
        [MediumHitID_Player1] = KeyCode.JoystickButton3,
        [HeavyHitID_Player1] = KeyCode.JoystickButton1,
        [SpecialHitID_Player1] = KeyCode.JoystickButton0,

        [LightHitID_Player2] = KeyCode.JoystickButton2,
        [MediumeHitID_Player2] = KeyCode.JoystickButton3,
        [HeavyHitID_Player2] = KeyCode.JoystickButton1,
        [SpecialHitID_Player2] = KeyCode.JoystickButton0,
    };
    #endregion static variables

    #region monobehaviour methods

    #endregion monobehaviour methods
    /// <summary>
    /// This will reassign all the player 
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="joystickIndex"></param>
    public static void AssignPlayerIndexJoystickIndex(int playerIndex, int joystickIndex)
    {
        if (playerIndex < 0 || playerIndex > JoystickIndexArray.Length)
        {
            Debug.LogWarning("The player index that was passed through was out of range.");
            return;
        }


        string playerIndexKey = "";
        if (playerIndex == 0)
        {
            playerIndexKey = Player1Key;
        }
        else if (playerIndex == 1)
        {
            playerIndexKey = Player2Key;
        }
        else
        {
            Debug.LogWarning("We did not account for this many players, be sure to add more keys");
            return;
        }
        JoystickIndexArray[playerIndex] = joystickIndex;

        KeyCode assignedLightHitKey = (joystickIndex < 0) ? PersistedInputDictioanry[LightHitID_Keyboard] : JoystickInputToPlayerControllerInput(PersistedInputDictioanry[LightHitID + playerIndexKey], joystickIndex);
        KeyCode assignedMediumtHitKey = (joystickIndex < 0) ? PersistedInputDictioanry[MediumHitID_Keyboard] : JoystickInputToPlayerControllerInput(PersistedInputDictioanry[MediumHitID + playerIndexKey], joystickIndex);
        KeyCode assignedHeavyHitKey = (joystickIndex < 0) ? PersistedInputDictioanry[HeavyHitID_Keyboard] : JoystickInputToPlayerControllerInput(PersistedInputDictioanry[HeavyHitID + playerIndexKey], joystickIndex);
        KeyCode assignedSpecialHitKey = (joystickIndex < 0) ? PersistedInputDictioanry[SpecialHitID_Keyboard] : JoystickInputToPlayerControllerInput(PersistedInputDictioanry[SpecialHitID + playerIndexKey], joystickIndex);


        InGamePlayerInputDictionary[LightHitID + playerIndexKey] = assignedLightHitKey;
        InGamePlayerInputDictionary[MediumHitID + playerIndexKey] = assignedMediumtHitKey;
        InGamePlayerInputDictionary[HeavyHitID + playerIndexKey] = assignedHeavyHitKey;
        InGamePlayerInputDictionary[SpecialHitID + playerIndexKey] = assignedSpecialHitKey;
    }

    /// <summary>
    /// Returns the horizontal axis of our connected controller
    /// </summary>
    /// <param name="controllerIndex"></param>
    /// <returns></returns>
    public static float GetHorizontalAxis(int playerIndex)
    {
        int controllerIndex = JoystickIndexArray[playerIndex];
        if (controllerIndex >= 0)
            return Input.GetAxisRaw("Horizontal_Joy" + controllerIndex.ToString());

        return (Input.GetKey(PersistedInputDictioanry[HorizontalPos_Keybaord]) ? 1 : 0) + (Input.GetKey(PersistedInputDictioanry[HorizontallNeg_Keyboard]) ? -1 : 0);
    }

    /// <summary>
    /// Returns the vertical axis of our connected controller
    /// </summary>
    /// <param name="controllerIndex"></param>
    /// <returns></returns>
    public static float GetVerticalAxis(int playerIndex)
    {
        int controllerIndex = JoystickIndexArray[playerIndex];
        if (controllerIndex >= 0)
        {
            return Input.GetAxisRaw("Vertical_Joy" + controllerIndex.ToString());

        }

        if (Input.GetKey(PersistedInputDictioanry[VerticalPos_Keybaord])) return 1;
        if (Input.GetKey(PersistedInputDictioanry[VerticalNeg_Keyboard])) return -1;
        return 0;
    }

    #region load/save methods
    public static void LoadControlsFromPlayerPrefs()
    {
        foreach (string controllerInput in PersistedInputDictioanry.Keys)
        {
            if (PlayerPrefs.HasKey(controllerInput))
            {
                PersistedInputDictioanry[controllerInput] = (KeyCode)PlayerPrefs.GetInt(controllerInput);
            }
        }
    }

    /// <summary>
    /// Saves the current keycodes 
    /// </summary>
    public static void SaveControlsToPlayerPrefs()
    {
        foreach(string controllerInput in PersistedInputDictioanry.Keys)
        {
            PlayerPrefs.SetInt(controllerInput, (int)PersistedInputDictioanry[controllerInput]);
        }
    }
    #endregion load/save methods


    /// <summary>
    /// Helper method that collexts the button offset to account for various player controllers being plugged in
    /// </summary>
    /// <param name="joystickKeycode"></param>
    /// <param name="joystickIndex"></param>
    /// <returns></returns>
    private static KeyCode JoystickInputToPlayerControllerInput(KeyCode joystickKeycode, int joystickIndex)
    {
        if (joystickIndex < 0)
        {
            return KeyCode.None;
        }
        int playerOffsetAmount = (int)(KeyCode.Joystick1Button0) - (int)KeyCode.JoystickButton0;
        int newKeycodeValue = (int)joystickKeycode + playerOffsetAmount * (joystickIndex + 1);
        return (KeyCode)newKeycodeValue;
    }

    /// <summary>
    /// Converts a keycode to the universal keycode value. This way we can apply the set inputs regardless of the conroller index that is plugged in
    /// </summary>
    /// <param name="joystickKeycode"></param>
    /// <param name="joystickIndex"></param>
    /// <returns></returns>
    private static KeyCode PlayerControllerInputToGenericControllerInput(KeyCode playerJoystickKeycode, int joystickIndex)
    {
        if (joystickIndex < 0)
        {
            return KeyCode.None;
        }
        int playerOffsetAmount = (int)(KeyCode.Joystick1Button0) - (int)KeyCode.JoystickButton0;
        int newKeycodeValue = (int)playerJoystickKeycode - playerOffsetAmount * (joystickIndex + 1);
        return (KeyCode)newKeycodeValue;
    }
}
