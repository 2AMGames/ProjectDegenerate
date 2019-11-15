using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// General spawn pool class for spawning and despawning monobehaviour objects in the game
/// </summary>
public class SpawnPool : MonoBehaviour
{
    #region static variables
    private static SpawnPool instance;
    public static SpawnPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SpawnPool>();
            }
            return instance;
        }
    }
    #endregion static variables
    /// <summary>
    /// This is strictly used to ensure that there is no repeat hashes for different prefabs
    /// </summary>
    private Dictionary<int, string> GeneratedSeeds = new Dictionary<int, string>();

    public Dictionary<int, Queue<SpawnableObject>> spawnPoolDictionary = new Dictionary<int, Queue<SpawnableObject>>();

    #region monobehaviour methods
    private void Awake()
    {
        instance = this;
    }
    #endregion monobehaviour methods


    /// <summary>
    /// Adds a new Instantiated object to the spawnpool list
    /// </summary>
    /// <param name="obj"></param>
    private void AddNewObjectToSpawnPool(SpawnableObject obj)
    {
        int hash = obj.name.GetHashCode();
        SpawnableObject newMono = Instantiate<SpawnableObject>(obj);

        newMono.name = newMono.name.Substring(0, obj.name.Length);//Removes the word clone from the instantiated object
        newMono.transform.SetParent(this.transform);
        newMono.gameObject.SetActive(false);
        newMono.isInSpawnPool = true;

        spawnPoolDictionary[hash].Enqueue(newMono);
    }

    /// <summary>
    /// This method will create a spawn pool for the the prefab that is passed in. And it will generate the number of items that are set in the count
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="count"></param>
    public void InitializeSpawnPool(SpawnableObject obj, int count = 1)
    {
        int hash = obj.name.GetHashCode();

        if (!spawnPoolDictionary.ContainsKey(hash))
        {
            GeneratedSeeds.Add(hash, obj.name);
            spawnPoolDictionary.Add(hash, new Queue<SpawnableObject>());
        }
        else
        {
            if (GeneratedSeeds[hash] != obj.name)
            {
                Debug.LogError("There was a repeat hash for " + obj.name + " and " + GeneratedSeeds[hash]);
                return;
            }
        }

        for (int i = 0; i < count; i++)
        {
            AddNewObjectToSpawnPool(obj);
        }
    }


    /// <summary>
    /// Spawns in an object from the spawn queue if there is one available. Creates a new one if there is no object in the queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefabObj"></param>
    /// <returns></returns>
    public T Spawn<T>(T prefabObj) where T : SpawnableObject
    {
        int hash = prefabObj.name.GetHashCode();
        if (!spawnPoolDictionary.ContainsKey(hash))
        {
            Debug.LogError(prefabObj.name + " Was Not Initialized! Please Be Sure To Initialize The Spawn Pool Before Attempting To Spawn A New Object");
            return null;
        }
        if (spawnPoolDictionary[hash].Count == 0)
        {
            AddNewObjectToSpawnPool(prefabObj);
        }
        T item = (T)spawnPoolDictionary[hash].Dequeue();
        item.gameObject.SetActive(true);
        item.isInSpawnPool = false;
        item.OnSpawnItem();

        return item;
    }

    /// <summary>
    /// Despawns an item in the game to be spawned in at another time in the future
    /// </summary>
    /// <param name="obj"></param>
    public void Despawn(SpawnableObject obj)
    {
        if (obj.isInSpawnPool)
        {
            Debug.LogError(obj.name + " was already despawned. Ignoring despawn call.");
            return;
        }
        int hash = obj.name.GetHashCode();
        if (!spawnPoolDictionary.ContainsKey(hash))
        {
            Debug.LogError("You are trying to despawn an object before you have initialized the spawn pool...Name: " + obj.name);
            return;
        }

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(this.transform);
        obj.isInSpawnPool = true;
        obj.OnDespawnItem();

        spawnPoolDictionary[hash].Enqueue(obj);
    }
}
