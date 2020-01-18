
using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileInteractionHandler : InteractionHandler
{
    #region const variables

    #endregion

    #region main variables

    private BaseProjectile Projectile;

    #endregion

    #region monobehaviour methods

    public override void Awake()
    {
        base.Awake();
        Projectile = GetComponent<BaseProjectile>();
    }

    #endregion

    #region public methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="myHurtbox"></param>
    /// <param name="enemyHitbox"></param>
    /// <param name="hitData"></param>
    /// <param name="height"></param>
    /// <param name="didMoveLand"></param>
    public override void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, HitData hitData, HitType height, bool didMoveLand)
    {
        //Debug.LogWarning("Projectile hit by enemy");
        Projectile.OnHitByEnemy();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="myHitbox"></param>
    /// <param name="enemyHurtbox"></param>
    /// <param name="didMoveLand"></param>
    public override void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox, bool didMoveLand)
    {
        base.OnHitEnemy(myHitbox, enemyHurtbox, didMoveLand);

        //Debug.LogWarning("Projectile hit enemy");

        Projectile.OnHitEnemy();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enemyHitbox"></param>
    public override void OnClash(Hitbox enemyHitbox)
    {
        //Debug.LogWarning("Projectile Clash");
        Projectile.OnClashWithEnemy();
    }

    #endregion

}
