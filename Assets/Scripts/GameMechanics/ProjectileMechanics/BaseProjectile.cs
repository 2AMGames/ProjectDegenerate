using System.Collections;
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
    public ProjectileInteractionHandler associatedInteractionHandler;
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
    }

    protected IEnumerator DespawnAfterTime(float despawnTime)
    {
        yield return new WaitForSeconds(despawnTime);
        DespawnProjectile();
    }

    public abstract void OnHit();

    /// <summary>
    /// Projectiles are typically shot. This method should be called upon launching the projectile
    /// </summary>
    public abstract void LaunchProjectile();

    #endregion
}
