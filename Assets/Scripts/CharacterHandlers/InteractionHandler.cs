using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class that handles any interaction with the enemy player or environment (hitboxes, projectiles, environmental traps, etc)
/// </summary>
public class InteractionHandler : MonoBehaviour
{

    #region main variables

    public Animator Animator { get; private set; }

    public CharacterStats CharacterStats { get; private set; }

    public CommandInterpreter CommandInterpreter { get; private set; }

    public MovementMechanics MovementMechanics { get; private set; }

    public float MoveGuardDamage;
    public float MoveGuardFrames;
    public Vector2 MoveGuardKnockback;

    public float MoveHitDamage;
    public float MoveHitFrames;
    public Vector2 MoveHitKnockback;

    public bool MoveGuardBreak;

    public int Hitstun = 0;

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
        //TODO Get current active move from animation and command interpreter.
        CharacterStats.OnPlayerHitByEnemy(myHurtbox, enemyHitbox, currentMove);
        MovementMechanics.HandlePlayerHit(currentMove);

        if (currentMove.OnHitFrames > 0)
        {
            Hitstun += (int)currentMove.OnHitFrames;
            Animator.SetBool("Hitstun", true);
            Animator.SetTrigger(currentMove.Magnitude.ToString());
            StartCoroutine(HandleHitstun());
        }
    }

    public void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox)
    {
        //TODO Get current active move from animation and command interpreter. Pass null for now.
        CharacterStats.OnPlayerHitEnemy(myHitbox, enemyHurtbox, CurrentMove);
        MovementMechanics.HandlePlayerHitEnemy(CurrentMove);
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
            Debug.LogWarning("Hitstun: " + Hitstun);
            yield return new WaitForEndOfFrame();
        }

        Animator.SetBool("Hitstun", false);
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
        public float OnGuardFrames;
        public Vector2 OnGuardKnockback;

        public float OnHitDamage;
        public float OnHitFrames;
        public Vector2 OnHitKnockback;

        public bool GuardBreak;

    }
    

    #endregion
}
