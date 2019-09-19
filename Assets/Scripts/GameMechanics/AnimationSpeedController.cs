using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpeedController : MonoBehaviour
{
    #region main variables

    private Animator Anim;

    #endregion

    #region monobehaviour methods

    void Start()
    {
        Anim = GetComponent<Animator>();
        Anim.enabled = false;
    }

    void Update()
    {
        SetAnimationSpeed();
    }

    #endregion

    #region private methods

    private void SetAnimationSpeed()
    {
        Debug.LogWarning("Set speed to: " + Overseer.DELTA_TIME);
        Anim.Update(Overseer.DELTA_TIME);
    }

    #endregion
}
