using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using PlayerInputData = PlayerInputPacket.PlayerInputData;
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

    public enum BUTTON_COMMAND
    {

    }
    #endregion enum

    #region const variabes

    private const int FRAMES_TO_BUFFER = 8;
    private const int DIRECTIONAL_INPUT_LENIENCY = 8;

    public const string Light = "Light";

    public const string Medium = "Medium";

    public const string Heavy = "Heavy";

    public const string Special = "Special";

    private const string Button = "Button";

    private const string Pressed = "Pressed";

    private const string Release = "Release";

    /// <summary>
    /// Light Punch trigger
    /// </summary>
    public static readonly int LP_ANIM_TRIGGER = Animator.StringToHash(Light);

    public static readonly int LP_PRESSED_ANIM_TRIGGER = Animator.StringToHash(Light + Pressed);

    public static readonly int LP_RELEASED_ANIM_TRIGGER = Animator.StringToHash(Light + Release);

    /// <summary>
    /// Medium Punch trigger
    /// </summary>
    public static readonly int MP_ANIM_TRIGGER = Animator.StringToHash(Medium);

    public static readonly int MP_PRESSED_ANIM_TRIGGER = Animator.StringToHash(Medium + Pressed);

    public static readonly int MP_RELEASED_ANIM_TRIGGER = Animator.StringToHash(Medium + Release);

    /// <summary>
    /// Heavy punch trigger
    /// </summary>
    public static readonly int HP_ANIM_TRIGGER = Animator.StringToHash(Heavy);

    public static readonly int HP_PRESSED_ANIM_TRIGGER = Animator.StringToHash(Heavy + Pressed);

    public static readonly int HP_RELEASED_ANIM_TRIGGER = Animator.StringToHash(Heavy + Release);

    /// <summary>
    /// Medium Kick Trigger
    /// </summary>
    public static readonly int SPECIAL_ANIM_TRIGGER = Animator.StringToHash(Special);

    public static readonly int SPECIAL_PRESSED_ANIM_TRIGGER = Animator.StringToHash(Special + Pressed);

    public static readonly int SPECIAL_RELEASED_ANIM_TRIGGER = Animator.StringToHash(Special + Release);

    public static readonly int BUTTON_ACTION_TRIGGER = Animator.StringToHash(Button + "Action");

    public static readonly int BUTTON_RELEASE_TRIGGER = Animator.StringToHash(Button + Release);

    /// <summary>
    /// Quarter Circle Forward
    /// </summary>
    private static readonly int QCF_ANIM_TRIGGER = Animator.StringToHash("QCF");

    /// <summary>
    /// Quartercircle Back
    /// </summary>
    private static readonly int QCB_ANIM_TRIGGER = Animator.StringToHash("QCB");

    /// <summary>
    /// Dragon punch input
    /// </summary>
    private static readonly int DP_ANIM_TRIGGER = Animator.StringToHash("DP");

    /// <summary>
    /// Super jump input
    /// </summary>
    private static readonly int S_JUMP_ANIM_TRIGGER = Animator.StringToHash("S_Jump");

    /// <summary>
    /// Forward dash input
    /// </summary>
    private static readonly int F_DASH_ANIM_TRIGGER = Animator.StringToHash("F_Dash");

    /// <summary>
    /// Back dash input
    /// </summary>
    private static readonly int B_DASH_ANIM_TRIGGER = Animator.StringToHash("B_Dash");

    /// <summary>
    /// Double down input (Down, Neutral, Down)
    /// </summary>
    private static readonly int DOUBLE_DOWN_ANIM_TRIGGER = Animator.StringToHash("Double_Down");

    // Attack inputs that require a button trigger complement for them to be valid.

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
        DIRECTION.FORWARD_DOWN,
        DIRECTION.DOWN,
        DIRECTION.FORWARD_DOWN
    };

    // Movement inputs that do not require a button trigger to be activated.

    private readonly DIRECTION[] F_DASH_INPUT = new DIRECTION[]
    {
        DIRECTION.NEUTRAL,
        DIRECTION.FORWARD,
        DIRECTION.NEUTRAL,
        DIRECTION.FORWARD
    };

    private readonly DIRECTION[] B_DASH_INPUT = new DIRECTION[]
    {
        DIRECTION.NEUTRAL,
        DIRECTION.BACK,
        DIRECTION.NEUTRAL,
        DIRECTION.BACK
    };

    private readonly DIRECTION[] S_JUMP_INPUT = new DIRECTION[]
    {
        DIRECTION.DOWN,
        DIRECTION.NEUTRAL,
        DIRECTION.UP
    };

    private readonly DIRECTION[] DOUBLE_DOWN_INPUT = new DIRECTION[]
    {
        DIRECTION.DOWN,
        DIRECTION.NEUTRAL,
        DIRECTION.DOWN
    };

    private readonly DirectionalinputStruct StartingDirection = new DirectionalinputStruct
    {
        direction = DIRECTION.UP,
        directionInput = new Vector2Int(1, 1)
    };

    #endregion const variables

    #region action methods
    public UnityAction<int> OnButtonPressedEvent;
    public UnityAction<int> OnButtonReleasedEvent;
    public UnityAction<DIRECTION, Vector2Int> OnDirectionSetEvent;

    #endregion 

    #region main variables

    private Animator Anim;
    public CharacterStats characterStats { get; private set; }

    #endregion

    #region input variables

    public DIRECTION CurrentDirection { get { return currentDirectionalInputStruct.direction; } }
    private DirectionalinputStruct currentDirectionalInputStruct;
    private Vector2Int lastJoystickInput { get { return currentDirectionalInputStruct.directionInput; } }

    private ushort lastInputPattern = ushort.MaxValue;

    private Dictionary<int, int> FramesRemainingUntilRemoveFromBuffer = new Dictionary<int, int>();
    public Dictionary<int, bool> ButtonsPressed = new Dictionary<int, bool>();

    /// <summary>
    /// Input queue. Delays input front of line input data until frame to execute is >= Current frame number.
    /// </summary>
    private Queue<PlayerInputData> InputBuffer = new Queue<PlayerInputData>();

    /// <summary>
    /// HighestReceivedFrame Added to prevent queuing a frame number less than the current one. This should also prevent spoofing inputs.
    /// </summary>
    private uint HighestReceivedFrameNumber;

    private int FramesSinceLastDirectionalInput;

    private int FrameDelay
    {
        get
        {
            if (Overseer.Instance == null || NetworkManager.Instance == null)
            {
                return 0;
            }
            return Overseer.Instance.IsNetworkedMode ? (int)NetworkManager.Instance.TotalDelayFrames : (int)GameStateManager.Instance.LocalFrameDelay;
        }
    }

    private List<DirectionalinputStruct> DirectionalInputRecordList = new List<DirectionalinputStruct>();

    #endregion

    #region monobehaviour methods
    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();

        Anim = GetComponent<Animator>();

        ResetInputData();

        StartCoroutine(CheckFramesSinceLastDirectionalInput());
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

        if (InputBuffer.Count > 0 && Overseer.Instance.GameReady)
        {
            PlayerInputData dataToExecute = InputBuffer.Peek();
            uint currentFrame = GameStateManager.Instance.FrameCount;
            uint frameToExecute = dataToExecute.FrameNumber + (uint)FrameDelay;
            if (frameToExecute - currentFrame <= 0)
            { 
                InputBuffer.Dequeue();
                ExecuteInput(dataToExecute);
            }
        }
    }

    #endregion

    #region public interface

    public bool QueuePlayerInput(PlayerInputData dataToQueue)
    {
        if (dataToQueue.FrameNumber > HighestReceivedFrameNumber)
        {
            HighestReceivedFrameNumber = dataToQueue.FrameNumber;
            if (dataToQueue.InputPattern > 0)
            {
                if (GameStateManager.Instance.FrameCount >= dataToQueue.FrameNumber + FrameDelay)
                {
                    // Execute immediately if queue the frame to execute on the frame it was supposed to be executed.
                    // Ex. Frame 57 was sent with 7 frames of delay, but we receive it on frame 64.
                    if (Overseer.Instance.GameReady && GameStateManager.Instance.FrameCount == dataToQueue.FrameNumber + FrameDelay)
                    {
                        ExecuteInput(dataToQueue);
                        return true;
                    }
                }
                InputBuffer.Enqueue(dataToQueue);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ResetInterpreter()
    {
        lastInputPattern = ushort.MaxValue;
        HighestReceivedFrameNumber = 0;
        FramesSinceLastDirectionalInput = 0;
        ResetMovementInputBuffers();
        characterStats.MovementMechanics.SetHorizontalInput(0.0f);
        characterStats.MovementMechanics.SetVerticalInput(0.0f);
    }

    public void ClearPlayerInputQueue()
    {
        InputBuffer.Clear();
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
        inputData |= IsButtonPressed(SPECIAL_ANIM_TRIGGER) << 4;

        inputData |= (currentDirectionalInputStruct.directionInput.x < 0f ? 1 : 0) << 6;
        inputData |= (currentDirectionalInputStruct.directionInput.x > 0f ? 1 : 0) << 7;
        inputData |= (currentDirectionalInputStruct.directionInput.y > 0f ? 1 : 0) << 8;
        inputData |= (currentDirectionalInputStruct.directionInput.y < 0f ? 1 : 0) << 9;

        return (ushort)inputData;

    }

    public int IsButtonPressed(int buttonTrigger)
    {
        return ButtonsPressed.ContainsKey(buttonTrigger) && ButtonsPressed[buttonTrigger] == true ? 1 : 0;
    }

    #endregion

    #region private interface

    private void ExecuteInput(PlayerInputData inputData)
    {
        if (lastInputPattern != inputData.InputPattern)
        {
            //Debug.LogWarning("Executing frame: " + inputData.FrameNumber + ", Executing on frame: " + GameStateManager.Instance.FrameCount + ", Anim State: " + clipName + ", Time = " + state.normalizedTime + ", RigidVelocity: " + characterStats.MovementMechanics.Velocity + ", X pos: " + gameObject.transform.position.x +  ", Y pos: " + gameObject.transform.position.y + ", ButtonTrigger: " + Anim.GetBool("ButtonAction"));
            if ((inputData.InputPattern & 1) == 1)
            {
                OnButtonEventTriggered(LP_ANIM_TRIGGER, LP_PRESSED_ANIM_TRIGGER);
                OnButtonPressedEvent?.Invoke(LP_ANIM_TRIGGER);
            }
            else
            {
                OnButtonReleased(LP_ANIM_TRIGGER, LP_RELEASED_ANIM_TRIGGER);
            }

            if (((inputData.InputPattern >> 1) & 1) == 1)
            {
                OnButtonEventTriggered(MP_ANIM_TRIGGER, MP_PRESSED_ANIM_TRIGGER);
                OnButtonPressedEvent?.Invoke(MP_ANIM_TRIGGER);
            }
            else
            {
                OnButtonReleased(MP_ANIM_TRIGGER, MP_RELEASED_ANIM_TRIGGER);
            }

            if (((inputData.InputPattern >> 2) & 1) == 1)
            {
                OnButtonEventTriggered(HP_ANIM_TRIGGER, HP_PRESSED_ANIM_TRIGGER);
                OnButtonPressedEvent?.Invoke(HP_ANIM_TRIGGER);
            }
            else
            {
                OnButtonReleased(HP_ANIM_TRIGGER, HP_RELEASED_ANIM_TRIGGER);
            }

            if (((inputData.InputPattern >> 3) & 1) == 1)
            {
                OnButtonEventTriggered(SPECIAL_ANIM_TRIGGER, SPECIAL_PRESSED_ANIM_TRIGGER);
                OnButtonPressedEvent?.Invoke(SPECIAL_ANIM_TRIGGER);
            }
            else
            {
                OnButtonReleased(SPECIAL_ANIM_TRIGGER, SPECIAL_RELEASED_ANIM_TRIGGER);
            }

            UpdateJoystickInput(GetJoystickInputFromData(inputData));
            
            lastInputPattern = inputData.InputPattern;

            CheckForMovementInputCommands();
        }   
    }

    public void OnButtonEventTriggered(int buttonEventName, int buttonPressed)
    {
        if (!ButtonsPressed[buttonEventName])
        {
            Anim.SetBool(buttonEventName, true);

            Anim.SetTrigger(buttonPressed);
            Anim.SetTrigger(BUTTON_ACTION_TRIGGER);

            FramesRemainingUntilRemoveFromBuffer[buttonPressed] = FRAMES_TO_BUFFER;
            StartCoroutine(DisableButtonTriggerAfterTime(buttonPressed));

            FramesRemainingUntilRemoveFromBuffer[BUTTON_ACTION_TRIGGER] = FRAMES_TO_BUFFER;
            StartCoroutine(DisableButtonTriggerAfterTime(BUTTON_ACTION_TRIGGER));


            ButtonsPressed[buttonEventName] = true;

            CheckDirectionalInputCommands();
        }
    }

    private void UpdateJoystickInput(Vector2Int currentJoystickVec)
    {
        if (currentJoystickVec == currentDirectionalInputStruct.directionInput)
            return;

        if (FramesSinceLastDirectionalInput >= DIRECTIONAL_INPUT_LENIENCY && DirectionalInputRecordList.Count > 1)
        {
            DirectionalInputRecordList.RemoveRange(0, DirectionalInputRecordList.Count - 1);
        }
        currentDirectionalInputStruct.direction = InterpretJoystickAsDirection(currentJoystickVec);
        OnDirectionSetEvent?.Invoke(CurrentDirection, currentJoystickVec);
        DirectionalinputStruct dInput = new DirectionalinputStruct();
        dInput.direction = CurrentDirection;
        dInput.directionInput = currentJoystickVec;
        DirectionalInputRecordList.Add(dInput);

        CheckForJumpInput(lastJoystickInput, currentJoystickVec);
        currentDirectionalInputStruct.directionInput = currentJoystickVec;
        FramesSinceLastDirectionalInput = -1;
    }

    private Vector2Int GetJoystickInputFromData(PlayerInputPacket.PlayerInputData inputData)
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
            bool jumpSuccessful = characterStats.MovementMechanics.Jump();
            if (jumpSuccessful)
            {
                FramesRemainingUntilRemoveFromBuffer[MovementMechanics.JumpTrigger] = FRAMES_TO_BUFFER;
                StartCoroutine(DisableButtonTriggerAfterTime(MovementMechanics.JumpTrigger));
            }
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

    /// <summary>
    /// Check the input for attacks using special motions. Should be checked after trigger a button animation parameter (LP,MP,HP, etc.)
    /// </summary>
    private void CheckDirectionalInputCommands()
    {

        int dpIndex = CheckIfDirectionalArrayMatches(DP_INPUT);
        if (dpIndex >= 0)
        {
            OnDirectionalInputExecuted(dpIndex, DP_INPUT, DP_ANIM_TRIGGER);
            Debug.LogWarning("DP Successful");
            return;
        }

        int qcbIndex = CheckIfDirectionalArrayMatches(QCB_INPUT);
        if (qcbIndex >= 0)
        {
            OnDirectionalInputExecuted(qcbIndex, QCB_INPUT, QCB_ANIM_TRIGGER);
            Debug.LogWarning("QCB Successful");
            return;
        }

        int qcfIndex = CheckIfDirectionalArrayMatches(QCF_INPUT);
        if (qcfIndex >= 0)
        {
            OnDirectionalInputExecuted(qcfIndex, QCF_INPUT, QCF_ANIM_TRIGGER);
            Debug.LogWarning("QCF successful");
            return;
        }
    }

    /// <summary>
    /// Check for movement related motions (Forward Dash, Back Dash, etc.) These can be checked independent from pressing an attack button.
    /// </summary>
    private void CheckForMovementInputCommands()
    {
        int fDashIndex = CheckIfDirectionalArrayMatches(F_DASH_INPUT);
        if (fDashIndex >= 0)
        {
            OnDirectionalInputExecuted(fDashIndex, F_DASH_INPUT, F_DASH_ANIM_TRIGGER);
            return;
        }

        int bDashIndex = CheckIfDirectionalArrayMatches(B_DASH_INPUT);
        if (bDashIndex >= 0)
        {
            OnDirectionalInputExecuted(bDashIndex, B_DASH_INPUT, B_DASH_ANIM_TRIGGER);
            return;
        }

        int sJumpIndex = CheckIfDirectionalArrayMatches(S_JUMP_INPUT);
        if (sJumpIndex >= 0)
        {
            OnDirectionalInputExecuted(sJumpIndex, S_JUMP_INPUT, S_JUMP_ANIM_TRIGGER);
            Debug.LogWarning("Super Jump Successful");
            return;
        }

        int doubleDownIndex = CheckIfDirectionalArrayMatches(DOUBLE_DOWN_INPUT);
        if (doubleDownIndex >= 0)
        {
            OnDirectionalInputExecuted(doubleDownIndex, DOUBLE_DOWN_INPUT, DOUBLE_DOWN_ANIM_TRIGGER);
            Debug.LogWarning("Double Down Successful");
            return;
        }
    }

    private void OnDirectionalInputExecuted(int startingIndex, DIRECTION[] moveExecuted, int animTrigger)
    {
        Anim.SetTrigger(animTrigger);
        DirectionalInputRecordList.RemoveRange(startingIndex, moveExecuted.Length);
        FramesRemainingUntilRemoveFromBuffer[animTrigger] = FRAMES_TO_BUFFER;
        StartCoroutine(DisableButtonTriggerAfterTime(animTrigger));

    }


    /// <summary>
    /// Checks to see if the array that is passed in matches anywhere in our 
    /// </summary>
    private int CheckIfDirectionalArrayMatches(DIRECTION[] inputArray)
    {
        if (inputArray.Length > DirectionalInputRecordList.Count)
        {
            return -1;
        }

        bool passedInput;
        for (int i = DirectionalInputRecordList.Count - inputArray.Length; i >= 0; i--)
        {
            passedInput = true;
            for (int j = 0; j < inputArray.Length; j++)
            {
                if (DirectionalInputRecordList[i + j].direction != inputArray[j])
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
        // In the case that character has buffered a movement direction input in the air, upon landing their direction will change, cause it to be flipped.
        // We should clear these just to make sure we don't accidentaly buffer a forward dash in the air, change direction and execute a dash toward the opposing player.
        // In the future, we could disable these animation triggers and flip the opposite trigger (if applicable).
        ResetMovementInputBuffers();
        currentDirectionalInputStruct.direction = InterpretJoystickAsDirection(currentDirectionalInputStruct.directionInput);
        DirectionalinputStruct dInput;
        for (int i = 0; i < DirectionalInputRecordList.Count; i++)
        {
            dInput = DirectionalInputRecordList[i];
            dInput.direction = InterpretJoystickAsDirection(dInput.directionInput);
            DirectionalInputRecordList[i] = dInput;
        }
    }

    public void OnButtonReleased(int buttonEventName, int releaseTrigger)
    {
        if (ButtonsPressed[buttonEventName] == true)
        {
            OnButtonReleasedEvent?.Invoke(buttonEventName);
            ButtonsPressed[buttonEventName] = false;
            Anim.SetBool(buttonEventName, false);

            Anim.SetTrigger(releaseTrigger);
            FramesRemainingUntilRemoveFromBuffer[releaseTrigger] = FRAMES_TO_BUFFER;
            StartCoroutine(DisableButtonTriggerAfterTime(releaseTrigger));

            Anim.SetTrigger(BUTTON_RELEASE_TRIGGER);
            FramesRemainingUntilRemoveFromBuffer[BUTTON_RELEASE_TRIGGER] = FRAMES_TO_BUFFER;
            StartCoroutine(DisableButtonTriggerAfterTime(BUTTON_RELEASE_TRIGGER));
        }
    }

    private void ResetInputData()
    {
        FramesRemainingUntilRemoveFromBuffer.Add(BUTTON_ACTION_TRIGGER, 0);

        FramesRemainingUntilRemoveFromBuffer.Add(LP_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(MP_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(HP_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(SPECIAL_ANIM_TRIGGER, 0);

        ButtonsPressed.Add(LP_ANIM_TRIGGER, false);
        ButtonsPressed.Add(MP_ANIM_TRIGGER, false);
        ButtonsPressed.Add(HP_ANIM_TRIGGER, false);
        ButtonsPressed.Add(SPECIAL_ANIM_TRIGGER, false);

        FramesRemainingUntilRemoveFromBuffer.Add(QCB_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(QCF_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(DP_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(F_DASH_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(B_DASH_ANIM_TRIGGER, 0);
        FramesRemainingUntilRemoveFromBuffer.Add(S_JUMP_ANIM_TRIGGER, 0);

        lastInputPattern = ushort.MaxValue;
        currentDirectionalInputStruct = StartingDirection;
    }

    /// <summary>
    /// Reset animation triggers associate with movement inputs.
    /// </summary>
    private void ResetMovementInputBuffers()
    {
        FramesRemainingUntilRemoveFromBuffer[QCB_ANIM_TRIGGER] = 0;
        FramesRemainingUntilRemoveFromBuffer[QCF_ANIM_TRIGGER] = 0;
        FramesRemainingUntilRemoveFromBuffer[DP_ANIM_TRIGGER] = 0;
        FramesRemainingUntilRemoveFromBuffer[F_DASH_ANIM_TRIGGER] = 0;
        FramesRemainingUntilRemoveFromBuffer[B_DASH_ANIM_TRIGGER] = 0;
        FramesRemainingUntilRemoveFromBuffer[S_JUMP_ANIM_TRIGGER] = 0;
    }

    #endregion

    #region Game Events

    #endregion

    #region Coroutines

    private IEnumerator DisableButtonTriggerAfterTime(int buttonEventName)
    {
        yield return null;
        while (FramesRemainingUntilRemoveFromBuffer[buttonEventName] > 0)
        {
            yield return new WaitForEndOfFrame();
            if (Overseer.Instance.GameReady)
            {
                if (!Anim.GetBool(buttonEventName))
                {
                    FramesRemainingUntilRemoveFromBuffer[buttonEventName] = 0;
                    yield break;
                }
                --FramesRemainingUntilRemoveFromBuffer[buttonEventName];
            }
        }

        Anim.ResetTrigger(buttonEventName);
    }

    private IEnumerator CheckFramesSinceLastDirectionalInput()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (Overseer.Instance.GameReady)
            {
                ++FramesSinceLastDirectionalInput;
            }
        }
    }

    #endregion

    #region structs
    [Serializable]
    public struct DirectionalinputStruct
    {
        public Vector2Int directionInput;
        public DIRECTION direction;
    }

    #endregion
}
