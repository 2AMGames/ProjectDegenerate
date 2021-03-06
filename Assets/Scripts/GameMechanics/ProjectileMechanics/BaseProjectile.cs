﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(CustomPhysics2D))]
[RequireComponent(typeof(ProjectileInteractionHandler))]
/// <summary>
/// This is the base projectile class. All projectiles in the game should extend this class
/// </summary>
public abstract class BaseProjectile : SpawnableObject //In general projectiles will be spawnable objects, that should be reused if possible
{

    #region main variables

    /// <summary>
    /// The associated character stats for our projectile. This should be the player that launched this projectile
    /// </summary>
    public CharacterStats associatedCharacterStats { get; set; }
    public ProjectileInteractionHandler associatedInteractionHandler { get; set; }
    public float launchSpeed;
    public Vector2 launchAngle;
    protected CustomPhysics2D rigid;
    protected Coroutine DespawnCoroutine;

    #endregion

    #region monobehaviour methods

    private void Awake()
    {
        rigid = GetComponent<CustomPhysics2D>();
        associatedInteractionHandler = GetComponent<ProjectileInteractionHandler>();
        //associatedInteractionHandler.AssociatedCharacterStats = associatedCharacterStats;
    }

    #endregion monobheaviour methods

    public InteractionHandler.HitData[] projectileHitDataList;

    #region virtual methods

    /// <summary>
    /// This should be called at run time when setting up our missile upon spawning it
    /// </summary>
    /// <param name="characterStats"></param>
    public virtual void SetupProjectile(CharacterStats characterStats)
    {
        this.associatedCharacterStats = characterStats;
        this.associatedInteractionHandler.AssociatedCharacterStats = this.associatedCharacterStats;
        associatedInteractionHandler.OnMoveBegin();
    }

    public virtual void DespawnProjectile()
    {
        rigid.enabled = false;
        SpawnPool.Instance.Despawn(this);
    }

    /// <summary>
    /// Coroutine method that will despawn our projectile after some time
    /// </summary>
    /// <param name="despawnTime"></param>
    /// <returns></returns>
    protected IEnumerator DespawnAfterTime(float despawnTime)
    {
        yield return new WaitForSeconds(despawnTime);
        DespawnProjectile();
    }
    #endregion virtual methods

    #region abstract methods
    /// <summary>
    /// This method will be called upon our hitbox colliding with an enemy hurtbox
    /// </summary>
    public abstract void OnHitEnemy();

    /// <summary>
    /// This method will be called if our projectile has a hurtbox that was hit by an enemy hitbox
    /// </summary>
    public abstract void OnHitByEnemy();

    /// <summary>
    /// This method will be called whenever our hitbox collides with another enemy hit box
    /// </summary>
    public abstract void OnClashWithEnemy();


    /// <summary>
    /// Projectiles are typically shot. This method should be called upon launching the projectile
    /// </summary>
    public abstract void LaunchProjectile();

    #endregion abstract methods
}
