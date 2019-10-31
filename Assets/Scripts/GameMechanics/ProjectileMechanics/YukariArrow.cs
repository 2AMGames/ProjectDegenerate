using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YukariArrow : BaseProjectile
{
    /// <summary>
    /// 
    /// </summary>
    public int bounceCount = 2;
    
    

    /// <summary>
    /// This method should be called upon spawning our projectile
    /// </summary>
    public override void SetupProjectile(CharacterStats characterStats)
    {
        base.SetupProjectile(characterStats);

    }

    public override void LaunchProjectile()
    {
        
    }
}
