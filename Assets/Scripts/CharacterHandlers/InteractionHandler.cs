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
    }

    #endregion

    #region public methods

    public void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox)
    {
        //TODO Get current active move from animation and command interpreter. Pass null for now.
        CharacterStats.OnPlayerHitByEnemy(myHurtbox, enemyHitbox, null);
    }

    public void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox)
    {
        //TODO Get current active move from animation and command interpreter. Pass null for now.
        CharacterStats.OnPlayerHitEnemy(myHitbox, enemyHurtbox, null);
    }

    public void OnClash(Hitbox enemyHitbox)
    {

    }

    #endregion
}
