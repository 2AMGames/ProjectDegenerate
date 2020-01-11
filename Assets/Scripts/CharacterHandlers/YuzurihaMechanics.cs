using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YuzurihaMechanics : MonoBehaviour
{
    #region fields

    public static readonly int ValidStanceKey = Animator.StringToHash("ValidStance");

    private bool LightAttack;

    private bool MediumAttack;

    private bool HeavyAttack;

    private bool SpecialAttack;

    private CommandInterpreter CommandInterpreter;

    private Animator Animator;

    private int ButtonPressed = 0;

    #endregion

    #region monobehaviour

    private void Start()
    {
        CommandInterpreter = GetComponent<CommandInterpreter>();
        CommandInterpreter.OnButtonReleasedEvent += OnButtonReleased;
        Animator = GetComponent<Animator>();
        ResetStanceAttacks();
    }

    #endregion

    #region public inteferface

    public void ResetStanceAttacks()
    {
        LightAttack = true;
        MediumAttack = true;
        HeavyAttack = true;
        SpecialAttack = true;

        UpdateAnimator();
        Animator.SetBool(ValidStanceKey, true);
    }

    public void StanceAttackStart()
    {
        UpdateButtonPressed();
        UpdateAnimator();
    }

    /// <summary>
    /// Called when entering stance idle by the animator.
    /// </summary>
    public void StanceIdleEnter()
    {
        var input = GetButtonPressed();
        bool valid = true;
        valid &= CommandInterpreter.IsButtonPressed(ButtonPressed) != 0;
        if (!valid)
        {
            OnButtonPressed(input);
        }
        else
        {
            Animator.SetBool(ValidStanceKey, true);
        }
    }

    public void OnButtonPressed(int buttonKey)
    {
        var button = buttonKey;
        bool validButton = false;
        validButton |= button == CommandInterpreter.LP_ANIM_TRIGGER && LightAttack;
        validButton |= button == CommandInterpreter.MP_ANIM_TRIGGER && MediumAttack;
        validButton |= button == CommandInterpreter.HP_ANIM_TRIGGER && HeavyAttack;
        validButton |= button == CommandInterpreter.SPECIAL_ANIM_TRIGGER && SpecialAttack;
        
        if (validButton)
        {
            ButtonPressed = buttonKey;                                              
        }
        else
        {
            Animator.SetBool(ValidStanceKey, false);
        }

    }

    public void OnButtonReleased(int buttonKey)
    {
        if (buttonKey == ButtonPressed)
        {
            Animator.SetBool(ValidStanceKey, false);
            ButtonPressed = 0;
        }
    }

    #endregion

    #region private interface

    private void UpdateAnimator()
    {
        Animator.SetBool("LightReady", LightAttack);
        Animator.SetBool("MediumReady", MediumAttack);
        Animator.SetBool("HeavyReady", HeavyAttack);
        Animator.SetBool("SpecialReady", SpecialAttack);
    }

    private int GetButtonPressed()
    {
        if (CommandInterpreter.IsButtonPressed(CommandInterpreter.LP_ANIM_TRIGGER) == 1)
        {
            return CommandInterpreter.LP_ANIM_TRIGGER;
        }
        else if (CommandInterpreter.IsButtonPressed(CommandInterpreter.MP_ANIM_TRIGGER) == 1)
        {
            return CommandInterpreter.MP_ANIM_TRIGGER;
        }
        else if (CommandInterpreter.IsButtonPressed(CommandInterpreter.HP_ANIM_TRIGGER) == 1)
        {
            return CommandInterpreter.HP_ANIM_TRIGGER;
        }
        else if (CommandInterpreter.IsButtonPressed(CommandInterpreter.SPECIAL_ANIM_TRIGGER) == 1)
        {
            return CommandInterpreter.SPECIAL_ANIM_TRIGGER;
        }

        return 0;
    }

    private void UpdateButtonPressed()
    {
        var buttonPressed = GetButtonPressed();
        if (buttonPressed != ButtonPressed && CommandInterpreter.IsButtonPressed(ButtonPressed) == 0)
        {
            Debug.LogWarning("Button pressed");
            ButtonPressed = buttonPressed;
        }

        if (buttonPressed == CommandInterpreter.LP_ANIM_TRIGGER)
        {
            LightAttack = false;
        }
        else if (buttonPressed == CommandInterpreter.MP_ANIM_TRIGGER)
        {
            MediumAttack = false;
        }
        else if (buttonPressed == CommandInterpreter.HP_ANIM_TRIGGER)
        {
            HeavyAttack = false;
        }
        else
        {
            SpecialAttack = false;
        }
    }

    #endregion
}
