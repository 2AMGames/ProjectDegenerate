using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpeedController : MonoBehaviour
{
    #region main variables

    private Animator Anim;

    private IEnumerator UpdateAnimatorCoroutine;

    #endregion

    #region monobehaviour methods

    public void Awake()
    {
        RunAnimSpeedControllerSetup();
    }

    public void OnEnable()
    {
        UpdateAnimatorCoroutine = UpdateAnimator();
        StartCoroutine(UpdateAnimatorCoroutine);
    }

    public void OnDisable()
    {
        if (UpdateAnimatorCoroutine != null)
        {
            StopCoroutine(UpdateAnimatorCoroutine);
        }

    }

    #endregion

    public void RunAnimSpeedControllerSetup()
    {
        Anim = GetComponent<Animator>();
        Anim.enabled = false;
    }

    #region private methods

    private IEnumerator UpdateAnimator()
    {
        while (true)
        {
            if (Overseer.Instance.GameReady && this.enabled)
            {
                Anim.Update(Overseer.DELTA_TIME);
            }
            yield return null;
        }
    }

    #endregion
}
