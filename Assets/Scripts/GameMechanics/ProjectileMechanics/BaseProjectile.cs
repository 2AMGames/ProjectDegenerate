﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(CustomPhysics2D))]
[RequireComponent(typeof(InteractionHandler))]
/// <summary>
/// This is the base projectile class. All projectiles in the game should extend this class
/// </summary>
public abstract class BaseProjectile : MonoBehaviour
{
    /// <summary>
    /// The associated character stats for our projectile. This should be the player that launched this projectile
    /// </summary>
    public CharacterStats associatedCharacterStats { get; set; }
    private InteractionHandler associatedInteractionHandler;
    public float launchSpeed;
    public Vector2 launchAngle;
    private CustomPhysics2D rigid;

    #region monobehaviour methods

    private void Awake()
    {
        rigid = GetComponent<CustomPhysics2D>();
        associatedInteractionHandler = GetComponent<InteractionHandler>();
    }
    #endregion monobheaviour methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="characterStats"></param>
    public virtual void SetupProjectile(CharacterStats characterStats)
    {
        this.associatedCharacterStats = characterStats;
        this.associatedInteractionHandler.CharacterStats = this.associatedCharacterStats;
    }

    /// <summary>
    /// Projectiles are typically shot. This method should be called upon launching the projectile
    /// </summary>
    public abstract void LaunchProjectile();
}
