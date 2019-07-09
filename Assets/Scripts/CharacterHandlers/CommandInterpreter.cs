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
    /// Light Punch trigger
    /// </summary>
    private const string LP_ANIM_TRIGGER = "LP";
    /// <summary>
    /// Medium Punch trigger
    /// </summary>
    private const string MP_ANIM_TRIGGER = "MP";

    /// <summary>
    /// Heavy punch trigger
    /// </summary>
    private const string HP_ANIM_TRIGGER = "HP";

    /// <summary>
    /// Light Kick Trigger
    /// </summary>
    private const string LK_ANIM_TRIGGER = "LK";

    /// <summary>
    /// Medium Kick Trigger
    /// </summary>
    private const string MK_ANIM_TRIGGER = "MK";

    /// <summary>
    /// Heavy Kick trigger
    /// </summary>
    private const string HK_ANIM_TRIGGER = "HK";

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

    private List<DIRECTION> directionalInputRecordList = new List<DIRECTION>();

    private const string BUTTON_ACTION_TRIGGER = "ButtonAction";
    #endregion const variables

    private DIRECTION currentDirection;

    private Dictionary<string, int> framesRemainingUntilRemoveFromBuffer = new Dictionary<string, int>();

    private Vector2Int lastJoystickInput = Vector2Int.zero;

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

        currentDirection = DIRECTION.NEUTRAL;
        lastJoystickInput = Vector2Int.zero;
    }

    private void Update()
    {
        if (Input.GetButtonDown(LP_ANIM_TRIGGER))
        {
            OnButtonEventTriggered(LP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(MP_ANIM_TRIGGER))
        {
            OnButtonEventTriggered(MP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(HP_ANIM_TRIGGER))
        {
            OnButtonEventTriggered(HP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(LK_ANIM_TRIGGER))
        {
            OnButtonEventTriggered(LK_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(MK_ANIM_TRIGGER))
        {

        }

        if (Input.GetButtonDown(HK_ANIM_TRIGGER))
        {
            
        }

        Vector2Int currentJoystickVec = GetJoystickInputAsVector2Int();
        if (lastJoystickInput != currentJoystickVec)
        {
            lastJoystickInput = currentJoystickVec;
            currentDirection = InterpretJoystickAsDirection(lastJoystickInput);
            OnDirectionSetEvent.Invoke(currentDirection);
            directionalInputRecordList.Add(currentDirection);
            StartCoroutine(RemoveDirectionalInputAfterTime());
        }
    }

    #endregion

    private Vector2Int GetJoystickInputAsVector2Int()
    {
        float horizontal = Input.GetAxisRaw(PlayerController.MOVEMENT_HORIZONTAL);
        float vertical = Input.GetAxisRaw(PlayerController.MOVEMENT_VERTICAL);

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
        int x = joystickInput.x;
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
                if (directionalInputRecordList[i + j] != inputArray[j])
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
    /// 
    /// </summary>
    /// <param name="direction"></param>
    private void PrintDirectionalArray(DIRECTION direction)
    {

    }
}
