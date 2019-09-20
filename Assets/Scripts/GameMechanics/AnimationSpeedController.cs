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

    private void LateUpdate()
    {
        if (Overseer.Instance.HasGameStarted)
        {
            /*
            AnimationClip[] clips = Anim.runtimeAnimatorController.animationClips;
            AnimatorStateInfo state = Anim.GetCurrentAnimatorStateInfo(0);
            string clipName = state.shortNameHash.ToString();
            for (int index = 0; index < clips.Length; ++index)
            {
                if (state.IsName(clips[index].name))
                {
                    clipName = clips[index].name;
                    break;
                }
            }

            //Debug.LogError("Late update frame: " + GameStateManager.Instance.FrameCount + ", Anim State: " + clipName + ", Time = " + state.normalizedTime);
            */
        }
    }

    #endregion

    #region private methods

    private void SetAnimationSpeed()
    {
        if (Overseer.Instance.IsGameReady)
        {
            Anim.Update(Overseer.DELTA_TIME);
        }
    }

    #endregion
}
