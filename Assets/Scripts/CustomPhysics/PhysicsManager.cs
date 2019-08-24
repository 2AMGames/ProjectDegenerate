using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The manager that handles how we control all of our physics objects in the game. This updates objects that contain a custom physics object and any collider object
/// </summary>
public class PhysicsManager : MonoBehaviour
{
    private static PhysicsManager instance;

    public static PhysicsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<PhysicsManager>();
            }
            return instance;
        }
    }

    /// <summary>
    /// A list of all the custom colliders in the scene
    /// </summary>
    private List<CustomCollider2D> colliderList = new List<CustomCollider2D>();
    /// <summary>
    /// A list of all the physics objects in the scene
    /// </summary>
    private List<CustomPhysics2D> customPhysicsList = new List<CustomPhysics2D>();
    #region monobehaviour methods
    private void Awake()
    {
        instance = this;
    }

    private void LateUpdate()
    {
        Vector2 vec;
        foreach (CustomCollider2D collider in colliderList)
        {
            if (collider.enabled)
            {
                collider.UpdateBoundsOfCollider();
            }
        }


        
        
        //Updates the velocity based on gravity
        foreach (CustomPhysics2D rigid in customPhysicsList)
        {
            if (rigid.enabled)
            {
                rigid.UpdateVelocityFromGravity();
            }
        }
        
        //Updates our physics object based on its physics state
        foreach (CustomPhysics2D rigid in customPhysicsList)
        {
            if (rigid.enabled)
            {
                rigid.UpdatePhysics();
            }
        }

        for (int i = 0; i < colliderList.Count - 1; i++)
        {
            for (int j = i + 1; j < colliderList.Count; j++)
            {
                if (colliderList[i].ColliderIntersect(colliderList[j], out vec))
                {
                    //print("I made it here");
                    if (colliderList[i].isStatic)
                    {
                        colliderList[i].PushObjectOutsideOfCollider(colliderList[j]);
                    }
                    if (colliderList[j].isStatic)
                    {
                        colliderList[j].PushObjectOutsideOfCollider(colliderList[i]);
                    }
                }
            }
        }
    }
    #endregion monobehaviour methods

    #region collider interaction methods


    /// <summary>
    /// Add a physics object to the manager.
    /// </summary>
    /// <param name="rigid"></param>
    public void AddCustomPhysics(CustomPhysics2D rigid) 
    {
        if (customPhysicsList.Contains(rigid))
        {
            return;
        }
        customPhysicsList.Add(rigid);
    }

    /// <summary>
    /// Remove a physics object from the manager if is present in the manager
    /// </summary>
    /// <param name="rigid"></param>
    public void RemoveCustomPhysics(CustomPhysics2D rigid)
    {
        if (customPhysicsList.Contains(rigid))
        {
            customPhysicsList.Remove(rigid);
        }
    }

    /// <summary>
    /// Add a collider to the manager
    /// </summary>
    /// <param name="collider"></param>
    public void AddColliderToManager(CustomCollider2D collider)
    {
        if (colliderList.Contains(collider))
        {
            return;
        }
        colliderList.Add(collider);
    }

    /// <summary>
    /// Removes a collider object from our physics manager. This should typically only be called upon the collider
    /// object being destroyed
    /// </summary>
    /// <param name="collider"></param>
    public void RemoveColliderFromManager(CustomCollider2D collider)
    {
        if (!colliderList.Contains(collider))
        {
            return;
        }
        colliderList.Remove(collider);
    }
    #endregion collider interaction methods

    /// <summary>
    /// Override method
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public bool CheckLineIntersectWithCollider(Vector2 origin, Vector2 direction, float distance)
    {
        List<CustomCollider2D> list = new List<CustomCollider2D>();
        return CheckLineIntersectWithCollider(origin, direction, distance, out list);
    }

    /// <summary>
    /// Gets a list of all colliders that intersect the line that passes through
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <param name="collidersHit"></param>
    /// <returns></returns>
    public bool CheckLineIntersectWithCollider(Vector2 origin, Vector2 direction, float distance, out List<CustomCollider2D> collidersHit)
    {
        collidersHit = new List<CustomCollider2D>();
        foreach (CustomCollider2D coll in colliderList)
        {
            if (coll.enabled)
            {
                DebugSettings.DrawLineDirection(origin, direction, distance, Color.red);
                if (coll.LineIntersectWithCollider(origin, direction, distance))
                {
                    collidersHit.Add(coll);
                }
            }
        }
        return collidersHit.Count >= 1;
    }
}
