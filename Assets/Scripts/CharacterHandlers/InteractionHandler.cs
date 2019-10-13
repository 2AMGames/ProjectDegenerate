using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class that handles any interaction with the enemy player or environment (hitboxes, projectiles, environmental traps, etc)
/// </summary>
public class InteractionHandler : MonoBehaviour
{

    #region const variables

    private const string HITSTUN_TRIGGER = "Hitstun";

    private const string GUARD_TRIGGER = "Guard";

    #endregion

    #region main variables

    public Animator Animator { get; private set; }

    public CharacterStats CharacterStats { get; private set; }

    public CommandInterpreter CommandInterpreter { get; private set; }

    public MovementMechanics MovementMechanics { get; private set; }

    public HashSet<InteractionHandler> CharactersHit { get; private set; }

    [SerializeField]
    private List<MoveData> CharacterMoves;

    private Dictionary<string, MoveData> CharacterMoveDict;

    private string AnimatorClipName
    {
        get
        {
            return Animator != null ? Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name : "idle";
        }
    }

    public MoveData CurrentMove
    {
        get
        {
            return Animator != null && CharacterMoveDict.ContainsKey(AnimatorClipName) ? CharacterMoveDict[AnimatorClipName] : default;
        }
    }

    public IEnumerator HitstunCoroutine;

    #region Interaction Data

    // Has the last move we activated already hit a player
    public bool MoveHitPlayer;

    public int Hitstun;

    #endregion

    #endregion

    #region monobehaviour methods

    public void OnMoveBegin()
    {
        CharactersHit.Clear();
    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        CommandInterpreter = GetComponent<CommandInterpreter>();
        MovementMechanics = GetComponent<MovementMechanics>();

        CharactersHit = new HashSet<InteractionHandler>();
        CharacterMoveDict = new Dictionary<string, MoveData>();
        foreach(MoveData move in CharacterMoves)
        {
            CharacterMoveDict.Add(move.MoveName, move);
        }
    }

    private void OnValidate()
    {
        CharacterMoveDict = new Dictionary<string, MoveData>();
        foreach (MoveData move in CharacterMoves)
        {
            CharacterMoveDict.Add(move.MoveName, move);
        }
    }

    #endregion

    #region public methods

    public void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, MoveData moveHitBy, bool didMoveLand)
    {

        int frames = didMoveLand ? moveHitBy.OnHitFrames : moveHitBy.OnGuardFrames;
        if (frames > 0)
        {
            if (Hitstun <= 0)
            {
                string triggerToSet = didMoveLand ? moveHitBy.Magnitude.ToString() : GUARD_TRIGGER;
                Animator.SetTrigger(triggerToSet);
                Animator.SetBool(HITSTUN_TRIGGER, true);
            }

            Hitstun = frames;

            CharacterStats.OnPlayerHitByEnemy(moveHitBy, didMoveLand);

            if (HitstunCoroutine != null)
            {
                StopCoroutine(HitstunCoroutine);
            }

            int direction = enemyHitbox.InteractionHandler.transform.position.x > transform.position.x ? -1 : 1;
            Vector2 destination = didMoveLand ? moveHitBy.OnHitKnockback : moveHitBy.OnGuardKnockback;
            destination.x *= direction;

            HitstunCoroutine = HandleHitstun(destination);
            StartCoroutine(HitstunCoroutine);
        }
    }

    public void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox, bool didMoveLand)
    {
        CharacterStats.OnPlayerHitEnemy(myHitbox, CurrentMove, didMoveLand);
        MoveHitPlayer = true;
        CharactersHit.Add(enemyHurtbox.InteractionHandler);

        // Handle case where player hit an enemy that is against a wall / static collider.
        // We should push this player away in this case.
        CustomPhysics2D enemyPhysics = enemyHurtbox.InteractionHandler.GetComponent<CustomPhysics2D>();

        if (Mathf.Abs(enemyPhysics.isTouchingSide.x) > 0)
        {
            // Distance should move = (Hit frames * DT * Knockback Vector)
            // Add two frames to the frames that the other player will move back to account for recovery frames.
            Vector2 destinationVector = new Vector2(didMoveLand ? CurrentMove.OnHitKnockback.x : CurrentMove.OnGuardKnockback.x, transform.position.y);
            int frames = (didMoveLand ? CurrentMove.OnHitFrames : CurrentMove.OnGuardFrames) + 2;
            float acceleration = MovementMechanics.IsInAir ? MovementMechanics.airAcceleration : MovementMechanics.groundAcceleration;

            float totalDistanceToMove = destinationVector.x * (didMoveLand ? CurrentMove.OnHitFrames : CurrentMove.OnGuardFrames) * Overseer.DELTA_TIME;
            float xVelocity = totalDistanceToMove - (acceleration * Overseer.DELTA_TIME * frames);

            destinationVector.x = xVelocity;
            MovementMechanics.TranslateForcedMovement(destinationVector, 1);
        }
        
    }

    public void OnClash(Hitbox enemyHitbox)
    {

    }

    #endregion

    #region private methods

    private IEnumerator HandleHitstun(Vector2 destination)
    {
        int totalHitStun = Hitstun;
        MovementMechanics.TranslateForcedMovement(destination, 1);
        float savedX = transform.position.x;
        while (Hitstun > 0)
        {
            MovementMechanics.ignoreJoystickInputs = true;
            MovementMechanics.ignoreJumpButton = true;
            yield return null;
            if (Overseer.Instance.IsGameReady)
            {
                --Hitstun;
            }
        }
        CharacterStats.OnHitstunFinished();

        Animator.SetBool(HITSTUN_TRIGGER, false);
        HitstunCoroutine = null;
    }
    
    #endregion

    #region structs

    [System.Serializable]
    public struct MoveData
    {
        public string MoveName;

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

        public HitHeight Height;

        public HitMagnitude Magnitude;

        public float SpecialMeterRequired;

        // Damage to apply if hit or blocked.
        public float HitDamage;
        public int OnHitFrames;
        public float HitMeterGain;
        public Vector2 OnHitKnockback;

        public float ChipDamage;
        public int OnGuardFrames;
        public float ChipMeterGain;
        public Vector2 OnGuardKnockback;

        // Allows the hitbox to register multiple hits on the opposing player in the same move.
        // Ex. A projectile may register as two hits when it connects with an opponent,
        // but a standing medium punch usually should not do the same.
        public bool AllowMultiHit;
        public bool GuardBreak;

    }
    
    #endregion
}
