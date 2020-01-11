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
            Height = InteractionHandler.HitType.Mid,
            SpecialMeterRequired = 0,

            Hits = new InteractionHandler.HitData[1]
            {
                new InteractionHandler.HitData
                {
                    HitDamage = 50,
                    OnHitFrames = 15,
                    HitMeterGain = 100,
                    OnHitKnockback = new Vector2(5f, 0f),
                    OnGuardFrames = 15,
                    OnGuardKnockback = new Vector2(10f, 0f),
                    ChipDamage = 1f,
                    ChipMeterGain = 1,
                }
            }
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

    public override void DespawnProjectile()
    {
        if (DespawnCoroutine != null)
        {
            StopCoroutine(DespawnCoroutine);
        }
        DespawnCoroutine = null;
        base.DespawnProjectile();
        OnDespawnArrow();
    }

    public override void OnHit()
    {
        DespawnProjectile();
    }

    #endregion

    #region private methods

    /// <summary>
    /// 
    /// </summary>
    private void OnDespawnArrow()
    {
        SpawnPool.Instance.Despawn(this);
    }

    #endregion
}
