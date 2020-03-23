using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpeedController : MonoBehaviour
{
    #region main variables

    private Animator Anim;

    private CharacterStats CharacterStats;

    private IEnumerator UpdateAnimatorCoroutine;

    #endregion

    #region monobehaviour methods

    public void Start()
    {
        Anim = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        Anim.enabled = false;

        //StartCoroutine(UpdateAnimator());
    }

    private void Update()
    {
        SetAnimationSpeed();
    }
    #endregion

    #region private methods

    private void SetAnimationSpeed()
    {
        if (Overseer.Instance.GameReady && CharacterStats.ShouldCharacterMove)
        {
            Anim.Update(Overseer.DELTA_TIME);
        }
    }

    #endregion
}
