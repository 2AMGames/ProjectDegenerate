﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is used to carry out specific functions and mechanics that would only apply to the Character 'Yukari'
/// </summary>
public class YukariMechanics : MonoBehaviour
{
    #region main variables
    [Tooltip("A prefab reference to Yukaris Arrow projectile")]
    public YukariArrow yukariArrowPrefab;
    [Tooltip("This is a placeholder arrow that will be swapped out by Yukari upon launching an arrow. This will also be used to set the direction and position of the arrow upon launch")]
    public Transform yukariArrowTransformReference;

    /// <summary>
    /// A reference to Yukari's Character Stats
    /// </summary>
    private CharacterStats associatedCharacterStats;
    #endregion main variables
    #region monobehaivour methods
    private void Awake()
    {
        associatedCharacterStats = GetComponent<CharacterStats>();
        SpawnPool.Instance.InitializeSpawnPool(yukariArrowPrefab, 3);//Initialize spawn pool so that we can appropriately use Yukari's arrows
    }
    #endregion monobehaviour methods

    #region event methods
    /// <summary>
    /// Event that will launch an arrow. The animation will have to despawn the display arrow on its own. You can not toggle
    /// gameobject active state from code if it is used by the animator
    /// </summary>
    public void OnLaunchArrow()
    {
        YukariArrow newlySpawnedArrow = SpawnPool.Instance.Spawn(yukariArrowPrefab);

        newlySpawnedArrow.transform.SetParent(null);
        newlySpawnedArrow.SetupProjectile(associatedCharacterStats);
        newlySpawnedArrow.transform.position = yukariArrowTransformReference.position;
        newlySpawnedArrow.transform.right = Mathf.Sign(this.transform.localScale.x) * yukariArrowTransformReference.right;
        newlySpawnedArrow.LaunchProjectile();
    }

    #endregion event methods
}
