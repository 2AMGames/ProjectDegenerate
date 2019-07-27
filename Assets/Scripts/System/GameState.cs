using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{

    public int FrameCount;

    public ushort RoundTimeElapsed;

    public List<PlayerState> PlayerStates;

    public struct PlayerState
    {
        public PlayerInputData InputData;

        public Vector3 PlayerPosition;

        public string CurrentAnimationClip;

        public AnimatorStateInfo CurrentAnimatorState;

        public AnimatorTransitionInfo CurrentAnimatorTransition;

        public List<AnimatorControllerParameter> AnimationParameters;
    }
}
