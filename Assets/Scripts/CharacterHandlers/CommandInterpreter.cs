using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandInterpreter : MonoBehaviour
{
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


    #region main variables

    private Animator anim
    {
        get
        {
            return characterStats.anim;
        }
    }

    public CharacterStats characterStats { get; private set; }

    #endregion

    #region monobehaviour method
    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
    }

    private void Update()
    {
        if (Input.GetButtonDown(LP_ANIM_TRIGGER))
        {
            anim.SetTrigger(LP_ANIM_TRIGGER);
        }
        if (Input.GetButtonDown(MP_ANIM_TRIGGER))
        {
            anim.SetTrigger(MP_ANIM_TRIGGER);
        }

        if (Input.GetButtonDown(HP_ANIM_TRIGGER))
        {
            anim.SetTrigger(HP_ANIM_TRIGGER);
        }

    }

    #endregion

   
}
