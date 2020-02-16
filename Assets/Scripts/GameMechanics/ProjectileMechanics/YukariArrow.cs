using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YukariArrow : BaseProjectile
{
    #region main variables
    /// <summary>
    /// 
    /// </summary>
    public int bounceCount = 2;

    public float DespawnTime = 3f;


    [Tooltip("This will set the maximum of Yukari's arrow")]
    public float maxStretchOfArrow = 1.2f;

    #endregion

    #region monobehaviour methods


    private void OnValidate()
    {
        if (rigid == null)
        {
            rigid = GetComponent<CustomPhysics2D>();
        }
        rigid.useGravity = false;
    }
    #endregion monobehaviour methods

    #region override methods

    /// <summary>
    /// This method should be called upon spawning our projectile
    /// </summary>
    public override void SetupProjectile(CharacterStats characterStats)
    {
        base.SetupProjectile(characterStats);

        associatedInteractionHandler.CurrentMove = new InteractionHandler.MoveData
        {
            MoveName = "BasicArrow",
            SpecialMeterRequired = 0,
            Hits = projectileHitDataList,
        };

        DespawnCoroutine = StartCoroutine(DespawnAfterTime(DespawnTime));

    }

    /// <summary>
    /// 
    /// </summary>
    public override void LaunchProjectile()
    {
        rigid.Velocity = -this.transform.right * launchSpeed;
        rigid.enabled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DespawnProjectile()
    {
        if (DespawnCoroutine != null)
        {
            StopCoroutine(DespawnCoroutine);
        }

        DespawnCoroutine = null;
        base.DespawnProjectile();
    }

    /// <summary>
    /// Upon landing a hit we despawn our projectile object from existence
    /// </summary>
    public override void OnHitEnemy()
    {
        DespawnProjectile();
    }

    public override void OnHitByEnemy()
    {
    }

    public override void OnClashWithEnemy()
    {
    }

    #endregion override methods

    #region private methods

    #endregion private methods
}
