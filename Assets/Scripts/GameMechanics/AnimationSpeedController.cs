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
        float speedModifier = Time.deltaTime > 0 ? Overseer.DELTA_TIME / Time.deltaTime: 0f;
        Animator.speed = speedModifier;
    }

    #endregion
}
