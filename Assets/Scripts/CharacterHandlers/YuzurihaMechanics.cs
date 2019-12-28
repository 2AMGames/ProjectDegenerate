using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YuzurihaMechanics : MonoBehaviour
{
    #region fields

    private bool LightAttack;

    private bool MediumAttack;

    private bool HeavyAttack;

    private CommandInterpreter CommandInterpreter;

    private Animator Animator;

    #endregion

    #region monobehaviour

    private void Start()
    {
        CommandInterpreter = GetComponent<CommandInterpreter>();
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
    }

    public void HighAttackStart()
    {
        LightAttack = false;
        UpdateAnimator();
    }

    public void MidAttackStart()
    {
        MediumAttack = false;
        UpdateAnimator();
    }

    public void LowAttackStart()
    {
        HeavyAttack = false;
        UpdateAnimator();
    }

    #endregion

    #region private interface

    private void UpdateAnimator()
    {
        Animator.SetBool("LightReady", LightAttack);
        Animator.SetBool("MediumReady", MediumAttack);
        Animator.SetBool("HeavyReady", HeavyAttack);
    }

    #endregion
}
