using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension of a monobehaivour object that is made to be added to a spawn pool.
/// Objects that would use this script involve projectiles recurring UI text objects, etc.
/// </summary>
public abstract class SpawnableObject : MonoBehaviour
{
    /// <summary>
    /// This method is primarily used to ensure that we do not spawn or despawn an object multiple times in a row. If an object
    /// is added to the spawn pool, this value will be marked true, otherwise it will be false.
    /// </summary>
    public bool isInSpawnPool { get; set; }

    /// <summary>
    /// Every time an item is spawned into the scene and removed from the spawn pool, this method will be called
    /// </summary>
    public virtual void OnSpawnItem()
    {

    }

    /// <summary>
    /// Every time an item is despawned from the scene and put back into this spawnpool, this method will be called
    /// </summary>
    public virtual void OnDespawnItem()
    {

    }
}
