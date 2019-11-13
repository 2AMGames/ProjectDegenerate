using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is used to carry out specific functions and mechanics that would only apply to the Character 'Yukari'
/// </summary>
public class YukariMechanics : MonoBehaviour
{
    [Tooltip("A prefab reference to Yukaris Arrow projectile")]
    public YukariArrow yukariArrowPrefab;

    #region monobehaivour methods
    private void Awake()
    {
        SpawnPool.Instance.InitializeSpawnPool(yukariArrowPrefab, 3);//Initialize spawn pool so that we can appropriately use Yukari's arrows
    }
    #endregion monobehaviour methods

    #region event methods


    #endregion event methods
}
