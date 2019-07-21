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
    private const bool DEFAULT_DIRECTION_RIGHT = true;

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
            return PlayerController.MOVEMENT_HORIZONTAL + (PlayerIndex + 1);
        }
    }

    private string VerticalInputKey
    {
        get
        {
            return PlayerController.MOVEMENT_VERTICAL + (PlayerIndex + 1);
        }
    }

    #endregion

    #region action methods
    public UnityAction<string> OnButtonPressedEvent;
    public UnityAction<string> OnbuttonReleasedEvent;
    public UnityAction<DIRECTION> OnDirectionSetEvent;
    #endregion 

    #region main variables

    private Animator Anim
    {
        get
        {
            return characterStats.Anim;
        }
    }
    /// <summary>
    /// Reference to the character stats that are associated with this character
    /// </summary>
    public CharacterStats characterStats { get; private set; }

    private bool isFacingRight { get { return characterStats.MovementMechanics.isFacingRight; } }

    public int PlayerIndex;

    private bool previousDirectionFasingRight = false;

    public DIRECTION CurrentDirection { get { return currentDirectionalInputStruct.direction; } }

    private DirectionalinputStruct currentDirectionalInputStruct;
    private Dictionary<string, int> framesRemainingUntilRemoveFromBuffer = new Dictionary<string, int>();

    private Vector2Int lastJoystickInput { get { return currentDirectionalInputStruct.directionInput; } }

    #endregion

    #region monobehaviour method
    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();


        framesRemainingUntilRemoveFromBuffer.Add(BUTTON_ACTION_TRIGGER, 0);

        framesRemainingUntilRemoveFromBuffer.Add(LP_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(MP_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(HP_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(LK_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(MK_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(HK_ANIM_TRIGGER, 0);

        framesRemainingUntilRemoveFromBuffer.Add(QCB_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(QCF_ANIM_TRIGGER, 0);

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
        if (Input.GetButtonDown(LightPunchKey))
        {
            OnButtonEventTriggered(LP_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(LP_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(LightPunchKey))
        {
            OnbuttonReleasedEvent?.Invoke(LP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(MediumPunchKey))
        {
            OnButtonEventTriggered(MP_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(MP_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(MediumPunchKey))
        {
            OnbuttonReleasedEvent?.Invoke(MP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(HardPunchKey))
        {
            OnButtonEventTriggered(HP_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(HP_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(HardPunchKey))
        {
            OnbuttonReleasedEvent?.Invoke(HP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(LightKickKey))
        {
            OnButtonEventTriggered(LK_ANIM_TRIGGER);
            OnButtonPressedEvent?.Invoke(LK_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(LightKickKey))
        {
            OnbuttonReleasedEvent?.Invoke(LK_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(MediumKickKey))
        {
            OnButtonPressedEvent?.Invoke(MK_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(MediumKickKey))
        {
            OnbuttonReleasedEvent?.Invoke(MK_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(HardKickKey))
        {
            OnButtonPressedEvent?.Invoke(HK_ANIM_TRIGGER);
        }
        else if (Input.GetButtonUp(HardKickKey))
        {
            OnbuttonReleasedEvent?.Invoke(HK_ANIM_TRIGGER);
        }

        Vector2Int currentJoystickVec = GetJoystickInputAsVector2Int();
        if (lastJoystickInput != currentJoystickVec)
        {
            currentDirectionalInputStruct.direction = InterpretJoystickAsDirection(currentJoystickVec);
            OnDirectionSetEvent?.Invoke(CurrentDirection);
            DirectionalinputStruct dInput = new DirectionalinputStruct();
            dInput.direction = CurrentDirection;
            dInput.directionInput = currentJoystickVec;
            directionalInputRecordList.Add(dInput);
            StartCoroutine(RemoveDirectionalInputAfterTime());

            CheckForJumpInput(lastJoystickInput, currentJoystickVec);
            currentDirectionalInputStruct.directionInput = currentJoystickVec;

        }
    }

    #endregion

    private void CheckForJumpInput(Vector2 prevInput, Vector2 currentInput)
    {
        if (prevInput.y < 1 && currentInput.y >= 1)
        {
            characterStats.MovementMechanics.Jump();
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

    /// <summary>
    /// Returns the current axis as a direction
    /// </summary>
    /// <returns></returns>
    private DIRECTION InterpretJoystickAsDirection(Vector2Int joystickInput)
    {
        int x = joystickInput.x * (characterStats.MovementMechanics.isFacingRight ? 1 : -1 );
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
    /// 
    /// </summary>
    /// <param name="buttonEvent"></param>
   private void OnButtonEventTriggered(string buttonEventName)
    {
        Anim.SetTrigger(buttonEventName);
        Anim.SetTrigger(BUTTON_ACTION_TRIGGER);
        if (framesRemainingUntilRemoveFromBuffer[buttonEventName] <= 0)
        {
            StartCoroutine(DisableButtonTriggerAfterTime(buttonEventName));
        }
        if (framesRemainingUntilRemoveFromBuffer[BUTTON_ACTION_TRIGGER] <= 0)
        {
            StartCoroutine(DisableButtonTriggerAfterTime(BUTTON_ACTION_TRIGGER));
        }
        framesRemainingUntilRemoveFromBuffer[buttonEventName] = FRAMES_TO_BUFFER;
        framesRemainingUntilRemoveFromBuffer[BUTTON_ACTION_TRIGGER] = FRAMES_TO_BUFFER;
        CheckDirectionalInputCommands();
    }

    private IEnumerator DisableButtonTriggerAfterTime(string buttonEventName)
    {
        yield return null;
        
        while (framesRemainingUntilRemoveFromBuffer[buttonEventName] > 0)
        {
            yield return new WaitForFixedUpdate();
            //if (buttonEventName == BUTTON_ACTION_TRIGGER)
            //{
            //    print(framesRemainingUntilRemoveFromBuffer[buttonEventName]);
            //}
            --framesRemainingUntilRemoveFromBuffer[buttonEventName];
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


    private void CheckDirectionalInputCommands()
    {
        if (CheckIfDirectionalArrayMatches(QCB_INPUT) > 0)
        {
            Anim.SetTrigger(QCB_ANIM_TRIGGER);
            if (framesRemainingUntilRemoveFromBuffer[QCB_ANIM_TRIGGER] <= 0)
            {
                Debug.LogWarning("QCB Successful");
                StartCoroutine(DisableButtonTriggerAfterTime(QCB_ANIM_TRIGGER));
            }

        }
        if (CheckIfDirectionalArrayMatches(QCF_INPUT) > 0)
        {
            Anim.SetTrigger(QCF_ANIM_TRIGGER);
            if (framesRemainingUntilRemoveFromBuffer[QCF_ANIM_TRIGGER] <= 0)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    private void PrintDirectionalArray(DIRECTION direction)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    private struct DirectionalinputStruct
    {
        public Vector2Int directionInput;
        public DIRECTION direction;
    }
}
