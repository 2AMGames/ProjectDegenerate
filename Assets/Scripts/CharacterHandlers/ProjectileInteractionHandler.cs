
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

    public override void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, HitData hitData, HitType height, bool didMoveLand)
    {
        Debug.LogWarning("Projectile hit by enemy");

        Projectile.DespawnProjectile();
    }

    public override void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox, bool didMoveLand)
    {
        base.OnHitEnemy(myHitbox, enemyHurtbox, didMoveLand);

        Debug.LogWarning("Projectile hit enemy");

        Projectile.OnHit();

    }

    public override void OnClash(Hitbox enemyHitbox)
    {
        Debug.LogWarning("Projectile Clash");
        Projectile.DespawnProjectile();
    }

    #endregion

}
