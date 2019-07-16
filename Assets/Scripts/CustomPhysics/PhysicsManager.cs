using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// </summary>
public class PhysicsManager : MonoBehaviour
{
    /// <summary>
    /// A list of all the custom colliders in the scene
    /// </summary>
    private List<CustomCollider2D> colliderList = new List<CustomCollider2D>();
    /// <summary>
    /// A list of all the physics objects in the scene
    /// </summary>
    private List<CustomPhysics2D> customPhysicsList = new List<CustomPhysics2D>();
    #region monobehaviour methods
    private void LateUpdate()
    {
        foreach (CustomCollider2D collider in colliderList)
        {
            if (collider.enabled && !collider.isStatic)
            {
                collider.UpdateBoundsOfCollider();
            }
        }
        foreach (CustomPhysics2D rigid in customPhysicsList)
        {
            if (rigid.enabled)
            {
                rigid.UpdateVelocityFromGravity();
            }
        }
        for (int i = 0; i < colliderList.Count; i++)
        {
            if (!colliderList[i].isStatic)
                colliderList[i].CheckForCollisions();
            //if (colliderList[i].enabled)
            //{
            //    for (int j = i + 1; j < colliderList.Count - 1; j++)
            //    {
            //        colliderList[i].IntersectWithCollider(colliderList[j]);
            //    }
                
            //}
        }

        foreach (CustomPhysics2D rigid in customPhysicsList)
        {
            if (rigid.enabled)
            {
                rigid.UpdatePhysics();
            }
        }
    }
    #endregion monobehaviour methods

    #region collider interaction methods
    
    public void AddCustomPhysics(CustomPhysics2D rigid) 
    {
        if (customPhysicsList.Contains(rigid))
        {
            return;
        }
        customPhysicsList.Add(rigid);
    }

    public void RemoveCustomPhysics(CustomPhysics2D rigid)
    {
        if (customPhysicsList.Contains(rigid))
        {
            customPhysicsList.Remove(rigid);
        }
    }

    public void AddColliderToManager(CustomCollider2D collider)
    {
        if (colliderList.Contains(collider))
        {
            return;
        }
        colliderList.Add(collider);
    }

    public void RemoveColliderFromManager(CustomCollider2D collider)
    {
        if (!colliderList.Contains(collider))
        {
            return;
        }
        colliderList.Remove(collider);
    }
    #endregion collider interaction methods

    public bool CheckLineIntersectWithCollider(Vector2 origin, Vector2 direction, float distance)
    {
        List<CustomCollider2D> list = new List<CustomCollider2D>();
        return CheckLineIntersectWithCollider(origin, direction, distance, out list);
    }

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
