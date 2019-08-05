using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterStats))]
public class CommandInterpreter : MonoBehaviour
{
    #region enum

    public enum DIRECTION
    {
        FORWARD_UP = 9,
        UP = 8,
        BACK_UP = 7,
        FORWARD = 6,
        NEUTRAL = 5,
        BACK = 4,
        FORWARD_DOWN = 3,
        DOWN = 2,
        BACK_DOWN = 1,
    }


    #endregion enum

    #region const variabes
    private const int FRAMES_TO_BUFFER = 50;
    private const int DIRECTIONAL_INPUT_LENIENCY = 50;

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

    /// <summary>
    /// Quarter Circle Forward
    /// </summary>
    private const string QCF_ANIM_TRIGGER = "QCF";

    /// <summary>
    /// Quartercircle Back
    /// </summary>
    private const string QCB_ANIM_TRIGGER = "QCB";

    /// <summary>
    /// Dragon punch input
    /// </summary>
    private const string DP_ANIM_TRIGGER = "DP";

    private readonly DIRECTION[] QCF_INPUT = new DIRECTION[]
    {
        DIRECTION.DOWN,
        DIRECTION.FORWARD_DOWN,
        DIRECTION.FORWARD,
    };

    private readonly DIRECTION[] QCB_INPUT = new DIRECTION[]
    {
        DIRECTION.DOWN,
        DIRECTION.BACK_DOWN,
        DIRECTION.BACK
    };

    private readonly DIRECTION[] DP_INPUT = new DIRECTION[]
    {
        DIRECTION.FORWARD,
        DIRECTION.DOWN,
        DIRECTION.FORWARD_DOWN,
    };

    private List<DirectionalinputStruct> directionalInputRecordList = new List<DirectionalinputStruct>();

    private const string BUTTON_ACTION_TRIGGER = "ButtonAction";
    #endregion const variables

    #region action methods
    public UnityAction<string> OnButtonPressedEvent;
    public UnityAction<string> OnButtonReleasedEvent;
    public UnityAction<DIRECTION, Vector2Int> OnDirectionSetEvent;

    #endregion 

    #region main variables

    private Animator Anim
    {
        get
        {
            return characterStats.Anim;
        }
    }
    public CharacterStats characterStats { get; private set; }

    #endregion

    #region input variables

    public DIRECTION CurrentDirection { get { return currentDirectionalInputStruct.direction; } }
    private DirectionalinputStruct currentDirectionalInputStruct;
    private Vector2Int lastJoystickInput { get { return currentDirectionalInputStruct.directionInput; } }

    private ushort lastButtonPattern;
    private ushort currentButtonPattern;

    private Dictionary<string, int> FramesRemainingUntilRemoveFromBuffer = new Dictionary<string, int>();
    public Dictionary<string, bool> ButtonsPressed = new Dictionary<string, bool>();

    private Queue<PlayerInputData> InputBuffer = new Queue<PlayerInputData>();

    #endregion

    #region monobehaviour methods
    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();

        ResetInputData();

        currentDirectionalInputStruct.direction = DIRECTION.NEUTRAL;
        currentDirectionalInputStruct.directionInput = Vector2Int.zero;
    }

    private void Start()
    {
        if (characterStats)
        {
            characterStats.MovementMechanics.OnDirectionChanged += this.OnDirectionChanged;
        }
    }

    private void Update()
    {
        if (InputBuffer.Count > 0)
        {
            ExecuteInput(InputBuffer.Dequeue());
        }
    }

    #endregion

    #region public interface

    public void QueuePlayerInput(PlayerInputData dataToQueue)
    {
        if (dataToQueue != null)
        {
            InputBuffer.Enqueue(dataToQueue);
        }
    }

    public ushort GetPlayerInputByte()
    {
        // Pattern: 
        // Bit 0: LP
        // Bit 1: MP
        // Bit 2: HP
        // Bit 3: LK
        // Bit 4: MK
        // Bit 5: HK
        // Bit 6: Left Directional Input
        // Bit 7: Right Directional Input
        // Bit 8: Up Directional Input
        // Bit 9: Down Directional Input

        var inputData = 0xf000;

        inputData |= IsButtonPressed(LP_ANIM_TRIGGER);
        inputData |= IsButtonPressed(MP_ANIM_TRIGGER) << 1;
        inputData |= IsButtonPressed(HP_ANIM_TRIGGER) << 2;
        inputData |= IsButtonPressed(LK_ANIM_TRIGGER) << 3;
        inputData |= IsButtonPressed(MK_ANIM_TRIGGER) << 4;
        inputData |= IsButtonPressed(HK_ANIM_TRIGGER) << 5;

        inputData |= (currentDirectionalInputStruct.directionInput.x < 0f ? 1 : 0) << 6;
        inputData |= (currentDirectionalInputStruct.directionInput.x > 0f ? 1 : 0) << 7;
        inputData |= (currentDirectionalInputStruct.directionInput.y > 0f ? 1 : 0) << 8;
        inputData |= (currentDirectionalInputStruct.directionInput.y < 0f ? 1 : 0) << 9;

        return (ushort)inputData;

    }

    public PlayerInputData GetPlayerInputDataIfUpdated()
    {
        // If player button pattern has been updated, return input data
        // else return null
        if (lastButtonPattern != currentButtonPattern)
        {
            PlayerInputData data = new PlayerInputData();

            data.FrameNumber = (uint)GameStateManager.Instance.FrameCount;
            data.InputPattern = GetPlayerInputByte();

            lastButtonPattern = currentButtonPattern;

            return data;
        }
        return null;
    }

    #endregion

    #region private interface

    private void ExecuteInput(PlayerInputData inputData)
    {
        if ((inputData.InputPattern & 1) == 1)
        {
            OnButtonEventTriggered(LP_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(LP_ANIM_TRIGGER);
        }
        else
        {
            OnButtonReleased(LP_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 1) & 1) == 1)
        {
            OnButtonEventTriggered(MP_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(MP_ANIM_TRIGGER);
        }
        else
        {
            OnButtonReleased(MP_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 2) & 1) == 1)
        {
            OnButtonEventTriggered(HP_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(HP_ANIM_TRIGGER);
        }
        else
        {
            OnButtonReleased(HP_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 3) & 1) == 1)
        {
            OnButtonEventTriggered(LK_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(LK_ANIM_TRIGGER);
        }
        else
        {
            OnButtonReleased(LK_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 4) & 1) == 1)
        {
            OnButtonEventTriggered(MK_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(MK_ANIM_TRIGGER);
        }
        else
        {
            OnButtonReleased(MK_ANIM_TRIGGER);
        }

        if (((inputData.InputPattern >> 5) & 1) == 1)
        {
            OnButtonEventTriggered(HK_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(HK_ANIM_TRIGGER);
        }
        else
        {
            OnButtonReleased(HK_ANIM_TRIGGER);
        }

        UpdateJoystickInput(GetJoystickInputFromData(inputData));

        currentButtonPattern = inputData.InputPattern;
    }

    public void OnButtonEventTriggered(string buttonEventName)
    {
        if (!ButtonsPressed[buttonEventName])
        {
            Anim.SetTrigger(buttonEventName);
            Anim.SetTrigger(BUTTON_ACTION_TRIGGER);

            if (FramesRemainingUntilRemoveFromBuffer[buttonEventName] <= 0)
            {
                StartCoroutine(DisableButtonTriggerAfterTime(buttonEventName));
            }
            if (FramesRemainingUntilRemoveFromBuffer[BUTTON_ACTION_TRIGGER] <= 0)
            {
                StartCoroutine(DisableButtonTriggerAfterTime(BUTTON_ACTION_TRIGGER));
            }

            FramesRemainingUntilRemoveFromBuffer[buttonEventName] = FRAMES_TO_BUFFER;
            FramesRemainingUntilRemoveFromBuffer[BUTTON_ACTION_TRIGGER] = FRAMES_TO_BUFFER;

            ButtonsPressed[buttonEventName] = true;

            CheckDirectionalInputCommands();

        }
    }

    private void UpdateJoystickInput(Vector2Int currentJoystickVec)
    {
        if (lastJoystickInput != currentJoystickVec)
        {
            currentDirectionalInputStruct.direction = InterpretJoystickAsDirection(currentJoystickVec);
            OnDirectionSetEvent?.Invoke(CurrentDirection, currentJoystickVec);
            DirectionalinputStruct dInput = new DirectionalinputStruct();
            dInput.direction = CurrentDirection;
            dInput.directionInput = currentJoystickVec;
            directionalInputRecordList.Add(dInput);
            StartCoroutine(RemoveDirectionalInputAfterTime());

            CheckForJumpInput(lastJoystickInput, currentJoystickVec);
            currentDirectionalInputStruct.directionInput = currentJoystickVec;
        }
    }

    private int IsButtonPressed(string buttonTrigger)
    {
        return ButtonsPressed.ContainsKey(buttonTrigger) && ButtonsPressed[buttonTrigger] == true ? 1 : 0;
    }

    private Vector2Int GetJoystickInputFromData(PlayerInputData inputData)
    {
        Vector2Int joystickVector = new Vector2Int();

        joystickVector.x -= ((inputData.InputPattern >> 6) & 1) == 1 ? 1 : 0;
        joystickVector.x += ((inputData.InputPattern >> 7) & 1) == 1 ? 1 : 0;

        joystickVector.y += ((inputData.InputPattern >> 8) & 1) == 1 ? 1 : 0;
        joystickVector.y -= ((inputData.InputPattern >> 9) & 1) == 1 ? 1 : 0;

        return joystickVector;

    }

    private void CheckForJumpInput(Vector2 prevInput, Vector2 currentInput)
    {
        if (prevInput.y < 1 && currentInput.y >= 1)
        {
            characterStats.MovementMechanics.Jump();
        }
    }

    /// <summary>
    /// Returns the current axis as a direction
    /// </summary>
    /// <returns></returns>
    private DIRECTION InterpretJoystickAsDirection(Vector2Int joystickInput)
    {
        int x = joystickInput.x * (characterStats.MovementMechanics.isFacingRight ? 1 : -1);
        int y = joystickInput.y;

        if (x == 0)
        {
            if (y == 0)
                return DIRECTION.NEUTRAL;
            else if (y == 1)
                return DIRECTION.UP;
            else
                return DIRECTION.DOWN;
        }
        else if (x == 1)
        {
            if (y == 0)
                return DIRECTION.FORWARD;
            else if (y == 1)
                return DIRECTION.FORWARD_UP;
            else
                return DIRECTION.FORWARD_DOWN;
        }
        else
        {
            if (y == 0)
                return DIRECTION.BACK;
            else if (y == 1)
                return DIRECTION.BACK_UP;
            else
                return DIRECTION.BACK_DOWN;
        }
    }

    private void CheckDirectionalInputCommands()
    {
        if (CheckIfDirectionalArrayMatches(QCB_INPUT) > 0)
        {
            Anim.SetTrigger(QCB_ANIM_TRIGGER);
            if (FramesRemainingUntilRemoveFromBuffer[QCB_ANIM_TRIGGER] <= 0)
            {
                Debug.LogWarning("QCB Successful");
                StartCoroutine(DisableButtonTriggerAfterTime(QCB_ANIM_TRIGGER));
            }

        }
        if (CheckIfDirectionalArrayMatches(QCF_INPUT) > 0)
        {
            Anim.SetTrigger(QCF_ANIM_TRIGGER);
            if (FramesRemainingUntilRemoveFromBuffer[QCF_ANIM_TRIGGER] <= 0)
            {
                Debug.LogWarning("QCF successful");
                StartCoroutine(DisableButtonTriggerAfterTime(QCF_ANIM_TRIGGER));
            }
        }
    }

    /// <summary>
    /// Checks to see if the array that is passed in matches anywhere in our 
    /// </summary>
    private int CheckIfDirectionalArrayMatches(DIRECTION[] inputArray)
    {
        if (inputArray.Length > directionalInputRecordList.Count)
        {
            return -1;
        }

        bool passedInput;
        for (int i = directionalInputRecordList.Count - inputArray.Length; i >= 0; i--)
        {
            passedInput = true;
            for (int j = 0; j < inputArray.Length; j++)
            {
                if (directionalInputRecordList[i + j].direction != inputArray[j])
                {
                    passedInput = false;
                    break;
                }
            }
            if (passedInput)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Whenever our direction is flipped this method should be called to adjust the 
    /// inputs to appropriately match the direction of the player
    /// </summary>
    /// <param name="isFacingRight"></param>
    private void OnDirectionChanged(bool isFacingRight)
    {
        currentDirectionalInputStruct.direction = InterpretJoystickAsDirection(currentDirectionalInputStruct.directionInput);
        DirectionalinputStruct dInput;
        for (int i = 0; i < directionalInputRecordList.Count; i++)
        {
            dInput = directionalInputRecordList[i];
            dInput.direction = InterpretJoystickAsDirection(dInput.directionInput);
            directionalInputRecordList[i] = dInput;
        }
    }

    public void OnButtonReleased(string buttonEventName)
    {
        if (ButtonsPressed[buttonEventName] == true)
        {
            OnButtonReleasedEvent?.Invoke(buttonEventName);
            ButtonsPressed[buttonEventName] = false;
        }
    }

    private void ResetInputData()
    {
        FramesRemainingUntilRemoveFromBuffer.Add(BUTTON_ACTION_TRIGGER, 0);

        FramesRemainingUntilRemoveFromBuffer.Add(LP_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(MP_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(HP_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(LK_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(MK_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(HK_ANIM_TRIGGER, 0);

        ButtonsPressed.Add(LP_ANIM_TRIGGER, false);
        ButtonsPressed.Add(MP_ANIM_TRIGGER, false);
        ButtonsPressed.Add(HP_ANIM_TRIGGER, false);
        ButtonsPressed.Add(LK_ANIM_TRIGGER, false);
        ButtonsPressed.Add(MK_ANIM_TRIGGER, false);
        ButtonsPressed.Add(HK_ANIM_TRIGGER, false);

        FramesRemainingUntilRemoveFromBuffer.Add(QCB_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(QCF_ANIM_TRIGGER, 0);
    }

    #endregion

    #region Coroutines

    private IEnumerator DisableButtonTriggerAfterTime(string buttonEventName)
    {
        yield return null;

        while (FramesRemainingUntilRemoveFromBuffer[buttonEventName] > 0)
        {
            yield return new WaitForFixedUpdate();
            //if (buttonEventName == BUTTON_ACTION_TRIGGER)
            //{
            //    print(framesRemainingUntilRemoveFromBuffer[buttonEventName]);
            //}
            --FramesRemainingUntilRemoveFromBuffer[buttonEventName];
        }

        Anim.ResetTrigger(buttonEventName);
    }

    private IEnumerator RemoveDirectionalInputAfterTime()
    {
        int framesThatHavePassed = 0;
        while (framesThatHavePassed < DIRECTIONAL_INPUT_LENIENCY)
        {
            yield return new WaitForFixedUpdate();
            ++framesThatHavePassed;
        }

        directionalInputRecordList.RemoveAt(0);
    }

    #endregion

    #region structs

    public struct DirectionalinputStruct
    {
        public Vector2Int directionInput;
        public DIRECTION direction;
    }

    #endregion
}
