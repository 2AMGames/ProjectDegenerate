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

    public void Start()
    {
        Anim = GetComponent<Animator>();
        Anim.enabled = false;
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

    #region private methods

    private IEnumerator UpdateAnimator()
    {
        while (true)
        {
            yield return null;
            if (Overseer.Instance.GameReady)
            {
                Anim.Update(Overseer.DELTA_TIME);
            }
        }
    }

    #endregion
}
