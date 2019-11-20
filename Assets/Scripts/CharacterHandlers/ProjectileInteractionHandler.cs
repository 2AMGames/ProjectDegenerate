
using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileInteractionHandler : InteractionHandler
{
    #region const variables

    #endregion

    #region main variables

    #endregion

    #region public methods

    public override void OnHitByEnemy(Hitbox myHurtbox, Hitbox enemyHitbox, HitData hitData, bool didMoveLand)
    {
        Debug.LogWarning("Projectile hit by enemy");
    }

    public override void OnHitEnemy(Hitbox myHitbox, Hitbox enemyHurtbox, bool didMoveLand)
    {
        base.OnHitEnemy(myHitbox, enemyHurtbox, didMoveLand);

        // TODO Do projectiles need to pause as well when they land?

        Debug.LogWarning("Projectile hit enemy");

    }

    public override void OnClash(Hitbox enemyHitbox)
    {
        Debug.LogWarning("Projectile Clash");
    }

    #endregion

}
