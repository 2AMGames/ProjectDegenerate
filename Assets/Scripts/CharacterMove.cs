using UnityEngine;

/// <summary>
/// Character special move or action. Pretty self explanatory if you've played a video game.
/// </summary>
public class CharacterMove : MonoBehaviour
{
    /// <summary>
    /// Move Name
    /// </summary>
    public string MoveName;

    /// <summary>
    /// Index in character animation
    /// </summary>
    public int AnimationIndex;

    /// <summary>
    /// The pattern of player inputs needed to execute.
    /// </summary>
    public string InputPattern;

    /// <summary>
    /// Data that affects the receiving player when a hit is registered.
    /// </summary>
    public struct MoveData
    {
        public float OnGuardDamage;
        public float OnGuardFrames;
        public Vector2 OnGuardKnockback;

        public float OnHitDamage;
        public float OnHitFrames;
        public Vector2 OnHitKnockback;

        public bool GuardBreak;

    }
}
