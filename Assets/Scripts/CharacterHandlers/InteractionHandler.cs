using System.Collections;
using UnityEngine;

/// <summary>
/// This is the base class that handles any interaction with the enemy player or environment (hitboxes, projectiles, environmental traps, etc)
/// </summary>
public class InteractionHandler : MonoBehaviour
{

    #region const variables

    private const string HITSTUN_TRIGGER = "Hitstun";

    #endregion

    #region main variables

    public Animator Animator { get; private set; }

    public CharacterStats CharacterStats { get; private set; }

    public CommandInterpreter CommandInterpreter { get; private set; }

    public MovementMechanics MovementMechanics { get; private set; }

    #region Current Move Data

    public float MoveGuardDamage;
    public int MoveGuardFrames;
    public Vector2 MoveGuardKnockback;

    public float MoveHitDamage;
    public int MoveHitFrames;
    public Vector2 MoveHitKnockback;

    public bool MoveGuardBreak;
    public bool MultiHit = false;

    // Has the last move we activated already hit a player
    public bool MoveHitPlayer;

    #endregion

    public int Hitstun;

    private Hitbox LastHitBoxCollidedWith;

    public MoveData CurrentMove
    {
        get
        {
            return new MoveData
            {
                OnGuardDamage = MoveGuardDamage,
                OnGuardFrames = MoveGuardFrames,
                OnGuardKnockback = MoveGuardKnockback,
                OnHitDamage = MoveHitDamage,
                OnHitFrames = MoveHitFrames,
                OnHitKnockback = MoveHitKnockback,
                GuardBreak = MoveGuardBreak
            };
        }
    }

    #endregion

    #region monobehaviour methods

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        CommandInterpreter = GetComponent<CommandInterpreter>();
        MovementMechanics = GetComponent<MovementMechanics>();
    }

    #endregion

    #region public methods

    public void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, MoveData currentMove)
    {
        Debug.LogWarning("Hit by enemy");

        //TODO Get current active move from animation and command interpreter.
        CharacterStats.OnPlayerHitByEnemy(myHurtbox, enemyHitbox, currentMove);
        MovementMechanics.HandlePlayerHit(currentMove);

        if (currentMove.OnHitFrames > 0)
        {
            Hitstun += currentMove.OnHitFrames;
            LastHitBoxCollidedWith = enemyHitbox;
            Animator.SetBool(HITSTUN_TRIGGER, true);
            Animator.SetTrigger(currentMove.Magnitude.ToString());
            StartCoroutine(HandleHitstun());
        }
    }

    public void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox)
    {
        //TODO Get current active move from animation and command interpreter. Pass null for now.
        CharacterStats.OnPlayerHitEnemy(myHitbox, enemyHurtbox, CurrentMove);
        MovementMechanics.HandlePlayerHitEnemy(CurrentMove);
        MoveHitPlayer = true;
    }

    public void OnClash(Hitbox enemyHitbox)
    {

    }

    #endregion

    #region private methods

    private IEnumerator HandleHitstun()
    {

        while (Hitstun > 0)
        {
            --Hitstun;
            yield return new WaitForEndOfFrame();
        }

        Animator.SetBool(HITSTUN_TRIGGER, false);
        LastHitBoxCollidedWith = null;
    }
    
    #endregion

    #region structs

    public struct MoveData
    {
        public enum HitMagnitude
        {
            LightHit,
            MediumHit,
            HeavyHit
        };

        public HitMagnitude Magnitude;

        public float OnGuardDamage;
        public int OnGuardFrames;
        public Vector2 OnGuardKnockback;

        public float OnHitDamage;
        public int OnHitFrames;
        public Vector2 OnHitKnockback;

        public bool GuardBreak;

        // Allows the hitbox to register multiple hits on the opposing player in the same move.
        // Ex. A projectile may register as two hits when it connects with an opponent,
        // but a standing medium punch usually should not do the same.
        public bool AllowMultiHit;

    }
    
    #endregion
}
