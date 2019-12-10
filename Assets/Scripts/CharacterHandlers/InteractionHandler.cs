using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This is the base class that handles any interaction with the enemy player or environment (hitboxes, projectiles, environmental traps, etc)
/// </summary>
public abstract class InteractionHandler : MonoBehaviour
{
    #region const variables

    private const string HITSTUN_TRIGGER = "Hitstun";

    private const string GUARD_TRIGGER = "Guard";

    protected const int PushbackFrames = 16;

    private const int WakeupDelayFrames = 30;

    #endregion

    #region main variables

    public Animator Animator { get; private set; }

    public CharacterStats AssociatedCharacterStats { get; set; }

    public HashSet<CharacterInteractionHandler> CharactersHit { get; private set; }

    protected string AnimatorClipName
    {
        get
        {
            return Animator != null ? Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name : "Idle";
        }
    }

    public virtual MoveData CurrentMove { get; set; }

    public HitData CurrentHitFromMove
    {
        get
        {
            return CurrentMove.Hits != null && CurrentMove.Hits.Length > HitIndex ? CurrentMove.Hits[HitIndex] : default;
        }
    }

    public int HitIndex;

    public bool MoveHitPlayer;

    #endregion

    #region monobehaviour methods

    public virtual void OnMoveBegin()
    {
        CharactersHit.Clear();
        MoveHitPlayer = false;
        HitIndex = 0;
    }

    /// <summary>
    /// Reset the move hit player bool, which should allow the currrent move to hit more than once.
    /// </summary>
    public void ResetMoveHit()
    {
        MoveHitPlayer = false;
    }

    /// <summary>
    /// If a move has more than one hit, increment the hit index. We should have code handle this variable, instead of having the animator set it.
    /// This is in case we want to do extra crazy stuff (ex. On hit, spend meter to reset the hit index).
    /// </summary>
    public void IncrementHitIndex()
    {
        ++HitIndex;
    }

    public virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        CustomPhysics2D rigid = GetComponent<CustomPhysics2D>();
        CharactersHit = new HashSet<CharacterInteractionHandler>();
    }

    #endregion

    #region public methods

    public virtual void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, HitData hitData, HitHeight height, bool didMoveLand)
    {
    }

    public virtual void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox, bool didMoveLand)
    {
        MoveHitPlayer = true;
        CharactersHit.Add((CharacterInteractionHandler)enemyHurtbox.InteractionHandler);
        AssociatedCharacterStats.OnPlayerHitEnemy(myHitbox, CurrentHitFromMove, didMoveLand);
    }

    public virtual void OnClash(Hitbox enemyHitbox)
    {

    }

    public virtual void OnComboEnded()
    {

    }

    #endregion

    #region private methods

    protected virtual IEnumerator PauseHandlerOnHit(UnityAction onPauseComplete)
    {
        yield break;
    }

    #endregion

    #region structs

    public enum HitHeight
    {
        Low,
        Mid,
        High,
        Air
    }
    public enum HitMagnitude
    {
        LightHit,
        MediumHit,
        HeavyHit,
    };

    [System.Serializable]
    /// <summary>
    /// Info about move. Contains array of hits to apply upon connecting with opposing player.
    /// </summary>
    public struct MoveData
    {
        [Header("Move Info")]
        public string MoveName;

        public float SpecialMeterRequired;

        public HitHeight Height;

        public HitData[] Hits;

    }

    [System.Serializable]
    /// <summary>
    ///  Data to read when a move makes contact.
    /// </summary>
    public struct HitData
    {

        public HitMagnitude Magnitude;
        public bool Knockdown;

        [Header("On Hit Parameters")]
        public float HitDamage;
        public int OnHitFrames;
        public float HitMeterGain;
        public Vector2 OnHitKnockback;

        [Header("On Guard Parameters")]
        public float ChipDamage;
        public int OnGuardFrames;
        public float ChipMeterGain;
        public Vector2 OnGuardKnockback;
        public bool GuardBreak;
    }
    #endregion
}
