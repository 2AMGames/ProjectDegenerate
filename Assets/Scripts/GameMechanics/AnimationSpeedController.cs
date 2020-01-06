using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpeedController : MonoBehaviour
{
    #region main variables

    private Animator Anim;

    private CharacterStats CharacterStats;

    #endregion

    #region monobehaviour methods

    void Start()
    {
        Anim = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        Anim.enabled = false;

        StartCoroutine(UpdateAnimator());
    }

    private IEnumerator UpdateAnimator()
    {
        while (true)
        {
            SetAnimationSpeed();
            yield return null;

        }
    }
    #endregion

    #region private methods

    private void SetAnimationSpeed()
    {
        if (Overseer.Instance.IsGameReady && CharacterStats.ShouldCharacterMove)
        {
            Anim.Update(Overseer.DELTA_TIME);
        }
    }

    #endregion
}
