using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CommandInterpreter : MonoBehaviour
{
    #region enum

    private enum DIRECTION
    {
        FORWARD_UP = 9,
        UP = 8,
        BACK_UP = 7,
        FORWARD = 6,
        NEUTRAL = 5, 
        BACK = 4,
        FORWARD_DOWN = 3,
        DOWN = 2,
        DOWN_BACK = 1,
    }


    #endregion enum
    #region const variabes
    private const int FRAMES_TO_BUFFER = 7;

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
    #endregion const variables

    private DIRECTION currentDirection;

    private Dictionary<string, int> framesRemainingUntilRemoveFromBuffer = new Dictionary<string, int>();

    #region main variables

    private Animator anim
    {
        get
        {
            return characterStats.anim;
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

        framesRemainingUntilRemoveFromBuffer.Add(LP_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(MP_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(HP_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(LK_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(MK_ANIM_TRIGGER, 0);
        framesRemainingUntilRemoveFromBuffer.Add(HK_ANIM_TRIGGER, 0);
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

        }

        if (Input.GetButtonDown(MK_ANIM_TRIGGER))
        {

        }

        if (Input.GetButtonDown(HK_ANIM_TRIGGER))
        {
            
        }
    }

    #endregion
    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonEvent"></param>
   private void OnButtonEventTriggered(string buttonEventName)
    {
        anim.SetTrigger(buttonEventName);
        if (framesRemainingUntilRemoveFromBuffer[buttonEventName] <= 0)
        {
            StartCoroutine(DisableButtonTriggerAfterTime(buttonEventName));
        }
        framesRemainingUntilRemoveFromBuffer[buttonEventName] = FRAMES_TO_BUFFER;
    }

    private IEnumerator DisableButtonTriggerAfterTime(string buttonEventName)
    {
        yield return null;
        
        while (framesRemainingUntilRemoveFromBuffer[buttonEventName] > 0)
        {
            yield return null;
            --framesRemainingUntilRemoveFromBuffer[buttonEventName];
        }
        if (anim.GetBool(buttonEventName))
        {
            anim.ResetTrigger(buttonEventName);

        }
    }
}
