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

    private CommandInterpreter CommandInterpreter;

    private Animator Animator;

    private int ButtonPressed = 0;

    #endregion

    #region monobehaviour

    private void Start()
    {
        CommandInterpreter = GetComponent<CommandInterpreter>();
        CommandInterpreter.OnButtonPressedEvent += OnButtonPressed;
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

        UpdateAnimator();
        Animator.SetBool(ValidStanceKey, true);
    }

    public void HighAttackStart()
    {
        LightAttack = false;
        UpdateAnimator();
        UpdateButtonPressed();
    }

    public void MidAttackStart()
    {
        MediumAttack = false;
        UpdateAnimator();
        UpdateButtonPressed();
    }

    public void LowAttackStart()
    {
        HeavyAttack = false;
        UpdateAnimator();
        UpdateButtonPressed();
    }

    public void StanceIdleEnter()
    {
        var input = GetButtonPressed();
        Animator.SetBool(ValidStanceKey, ButtonPressed != 0 && ButtonPressed == input);
    }

    public void OnButtonPressed(int buttonKey)
    {
        if (buttonKey != ButtonPressed && CommandInterpreter.IsButtonPressed(ButtonPressed) == 0)
        {
            Animator.SetBool(ValidStanceKey, false);
        }
    }

    public void OnButtonReleased(int buttonKey)
    {
        if (buttonKey == ButtonPressed)
        {
            ButtonPressed = 0;
            Animator.SetBool(ValidStanceKey, false);
        }
    }

    #endregion

    #region private interface

    private void UpdateAnimator()
    {
        Animator.SetBool("LightReady", LightAttack);
        Animator.SetBool("MediumReady", MediumAttack);
        Animator.SetBool("HeavyReady", HeavyAttack);
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

        return 0;
    }

    private void UpdateButtonPressed()
    {
        var buttonPressed = GetButtonPressed();
        if (buttonPressed != ButtonPressed)
        {
            ButtonPressed = buttonPressed;
        }
    }

    #endregion
}
