using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{

    public short RoundCount;

    public ushort FrameCount;

    public ushort RoundTime;

    public List<PlayerState> PlayerStates;

    public struct PlayerState
    {

        // Player Info

        public int PlayerIndex;

        public Vector3 PlayerPosition;

        // Health

        public float Health;

        public float ChipDamage;

        public float ComboDamage;

        public float SpecialMeter;

        // Animation Info

        public string CurrentAnimationClip;

        public AnimatorStateInfo CurrentAnimatorState;

        public AnimatorTransitionInfo CurrentAnimatorTransition;

        public List<AnimatorControllerParameter> AnimationParameters;

        // Input Data

        public PlayerInputPacket.PlayerInputData InputData;
    }

    public struct PlayerMatchStatistics
    {

        public int PlayerIndex;

        public ushort RoundsWon;

        public ushort MaxCombo;

        public float DamageDone;

        public float DamageTaken;

        public float DamageBlocked;

    }
}
