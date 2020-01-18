using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension of base projectile. This class handles all the logic for our wind projectile that
/// Yukari shoots out, typically through summoning Isis
/// </summary>
public class YukariWindProjectile : BaseProjectile
{
    [Header("Visual Variables")]
    public Transform windProjectilParticlesContainer;
    public float rotationRate = 10;
    public float bobbingMagnitude = 10;
    public float bobbingFrequency = 100;


    #region monobehaviour methods
    private void Awake()
    {
        
    }

    private void Update()
    {
        RotateProjectileContainer();
        BobProjectileUpAndDown();
    }
    #endregion monobehaviour methods
    #region override methods

    public override void OnHitEnemy()
    {

    }

    public override void OnHitByEnemy()
    {

    }

    public override void OnClashWithEnemy()
    {

    }

    public override void LaunchProjectile()
    {

    }
    #endregion override methods

    #region visual helper methods
    /// <summary>
    /// 
    /// </summary>
    private void RotateProjectileContainer()
    {
        windProjectilParticlesContainer.Rotate(Vector3.up, Overseer.DELTA_TIME * rotationRate, Space.Self);
    }

    /// <summary>
    /// 
    /// </summary>
    private void BobProjectileUpAndDown()
    {
        float updatedYPosition = Mathf.Sin(Time.time * bobbingFrequency) * bobbingMagnitude;
        windProjectilParticlesContainer.localPosition = new Vector3(0, updatedYPosition, 0);
    }
    #endregion visual helper methods
}
