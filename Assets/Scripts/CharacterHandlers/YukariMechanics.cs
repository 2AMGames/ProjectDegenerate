using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is used to carry out specific functions and mechanics that would only apply to the Character 'Yukari'
/// </summary>
public class YukariMechanics : MonoBehaviour
{
    #region const variables
    /// <summary>
    /// The number of arrow that we will spawn for our character at the start of the game
    /// </summary>
    private const int INITIAL_YUKARI_ARROW_COUNT = 3;
    #endregion const variables

    #region main variables
    [Tooltip("A prefab reference to Yukaris Arrow projectile")]
    public YukariArrow yukariArrowPrefab;
    [Tooltip("A list of placeholder arrows that wil be swapped out for YukariArrowPrefab when it is shot. This simply shows a sprite representation of where the prefab will appear")]
    public Transform[] yukariArrowTransformReferenceList = new Transform[3];

    /// <summary>
    /// A reference to Yukari's Character Stats
    /// </summary>
    private CharacterStats associatedCharacterStats;
    #endregion main variables

    #region monobehaivour methods
    private void Awake()
    {
        associatedCharacterStats = GetComponent<CharacterStats>();
        SpawnPool.Instance.InitializeSpawnPool(yukariArrowPrefab, INITIAL_YUKARI_ARROW_COUNT);//Initialize spawn pool so that we can appropriately use Yukari's arrows
    }
    #endregion monobehaviour methods

    #region event methods
    /// <summary>
    /// Event that will launch an arrow. The animation will have to despawn the display arrow on its own. You can not toggle
    /// gameobject active state from code if it is used by the animator
    /// </summary>
    public void OnLaunchArrow()
    {
        foreach (Transform yukariArrowTransformReference in yukariArrowTransformReferenceList)
        {
            if (yukariArrowTransformReference.gameObject.activeSelf)
            {
                YukariArrow newlySpawnedArrow = SpawnPool.Instance.Spawn(yukariArrowPrefab);

                newlySpawnedArrow.transform.SetParent(null);
                newlySpawnedArrow.SetupProjectile(associatedCharacterStats);

                newlySpawnedArrow.transform.position = yukariArrowTransformReference.position;
                newlySpawnedArrow.transform.localScale = yukariArrowTransformReference.localScale;
                newlySpawnedArrow.transform.rotation = yukariArrowTransformReference.rotation;

                newlySpawnedArrow.transform.right = Mathf.Sign(this.transform.localScale.x) * yukariArrowTransformReference.right;
                newlySpawnedArrow.LaunchProjectile();
            }
        }
        
    }


    #endregion event methods
}
