using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpeedController : MonoBehaviour
{
    #region main variables

    private Animator Animator;

    #endregion

    #region monobehaviour methods

    void Start()
    {
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        SetAnimationSpeed();
    }

    #endregion

    #region private methods

    private void SetAnimationSpeed()
    {
        float speedModifier = Overseer.DELTA_TIME > 0 ? Time.deltaTime / Overseer.DELTA_TIME : 0f;
        Animator.speed = speedModifier;
    }

    #endregion
}
