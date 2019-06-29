using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public enum CharacterTeam
    {
        PLAYER_1,
        PLAYER_2,
        NEUTRAL,
    }
    public MovementMechanics MovementMechanics { get; private set; }

    [HideInInspector]
    public int PlayerIndex;

    private void Awake()
    {
        MovementMechanics = GetComponent<MovementMechanics>();
    }

    public void OnPlayerHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox)
    {
        if (myHurtbox.associatedCharacterStats == this && enemyHitbox.associatedCharacterStats != this)
        {
            print("I got hit: " + PlayerIndex);
            MovementMechanics.HandlePlayerHit(myHurtbox, enemyHitbox);
        }
    }

    public void OnPlayerHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox)
    {
        if (myHitbox.associatedCharacterStats == this && enemyHurtbox.associatedCharacterStats != this)
        {
            print("I Hit Someone: " + PlayerIndex);
            MovementMechanics.HandlePlayerHitEnemy(myHitbox, enemyHurtbox);
        }
    }
    
    public void OnClash(Hitbox myHitbox, Hitbox enemyHitbox)
    {
        print("Clash");
    }

}
