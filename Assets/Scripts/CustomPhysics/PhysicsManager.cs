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
    private List<CustomCollider2D> nonStaticColliderList = new List<CustomCollider2D>();

    /// <summary>
    /// 
    /// </summary>
    private List<CustomCollider2D> staticColliderList = new List<CustomCollider2D>();
    /// <summary>
    /// A list of all the physics objects in the scene
    /// </summary>
    private List<CustomPhysics2D> allCustomPhysicsObjectsList = new List<CustomPhysics2D>();
    #region monobehaviour methods
    private void Awake()
    {
        instance = this;
    }
    private void LateUpdate()
    {
        if (!Overseer.Instance.IsGameReady)
        {
            return;
        }

        Overseer.Instance.HitboxManager.UpdateHitboxManager();

        foreach (CustomCollider2D collider in nonStaticColliderList)
        {
            if (collider.enabled)
            {
                collider.UpdateBoundsOfCollider();
            }
        }

        
        //Updates the velocity based on gravity
        foreach (CustomPhysics2D rigid in allCustomPhysicsObjectsList)
        {
            if (rigid.enabled)
            {
                if (rigid.UseAnimatorVelocity)
                {
                    // Multiply the x component of the velocity by the negative x scale of the transform (positive x scale value is left, negative x scale is facing right).
                    // Do this to keep the animator x component going in the right direction. The animator sets the absolute velocity, but we should be responsible for the direction.
                    rigid.Velocity.x = rigid.AnimatorVelocity.x * -Mathf.Sign(rigid.gameObject.transform.lossyScale.x);
                    rigid.Velocity.y = rigid.AnimatorVelocity.y;
                }
                else
                {
                    rigid.UpdateVelocityFromGravity();
                }
            }
        }

        foreach (CustomCollider2D collider in nonStaticColliderList)
        {
            collider.UpdateBoundsOfCollider();
            collider.originalVelocity = collider.rigid.Velocity;
        }

        bool collidedVerticallyWithAnyStatic = false;
        bool collidedHorizontallyWithAnyStatic = false;
        foreach (CustomCollider2D nonStaticCollider in nonStaticColliderList)
        {
            foreach (CustomCollider2D staticCollider in staticColliderList)
            {
                if (!staticCollider.isActiveAndEnabled || !nonStaticCollider.isActiveAndEnabled)
                {
                    continue;//Skip if either collider is inactive
                }
                bool collidedVertically = nonStaticCollider.ColliderIntersectVertically(staticCollider);
                bool collidedHorizontally = nonStaticCollider.ColliderIntersectHorizontally(staticCollider);

                if (collidedVertically)
                {
                    collidedVerticallyWithAnyStatic = true;
                    if (nonStaticCollider.rigid.isInAir && nonStaticCollider.rigid.Velocity.y <= 0)
                    {
                        nonStaticCollider.rigid.isInAir = false;
                        nonStaticCollider.rigid.OnPhysicsObjectGrounded();
                    }
                    if (nonStaticCollider.rigid.Velocity.y > 0)
                    {
                        nonStaticCollider.rigid.isTouchingSide.y = 1;
                    }
                    else if (nonStaticCollider.rigid.Velocity.y < 0)
                    {
                        nonStaticCollider.rigid.isTouchingSide.y = -1;
                    }
                    nonStaticCollider.rigid.Velocity.y = 0;
                    nonStaticCollider.originalVelocity = nonStaticCollider.rigid.Velocity;
                }
                if (collidedHorizontally)
                {
                    collidedHorizontallyWithAnyStatic = true;
                    if (nonStaticCollider.rigid.Velocity.x > 0)
                    {
                        nonStaticCollider.rigid.isTouchingSide.x = 1;
                    }
                    else if (nonStaticCollider.rigid.Velocity.x < 0)
                    {
                        nonStaticCollider.rigid.isTouchingSide.x = -1;
                    }
                    nonStaticCollider.rigid.Velocity.x = 0;
                    nonStaticCollider.originalVelocity = nonStaticCollider.rigid.Velocity;
                }



                if (collidedVertically || collidedHorizontally)
                {
                    nonStaticCollider.UpdateBoundsOfCollider();
                }


            }
            if (!collidedVerticallyWithAnyStatic && Mathf.Abs(nonStaticCollider.rigid.Velocity.y) > 0)
            {
                nonStaticCollider.rigid.isTouchingSide.y = 0;
            }
            if (!collidedHorizontallyWithAnyStatic && Mathf.Abs(nonStaticCollider.rigid.Velocity.x) > 0)
            {
                nonStaticCollider.rigid.isTouchingSide.x = 0;
            }
        }

        float xi;
        float xj;
        CustomCollider2D slowerCollider;
        CustomCollider2D fasterCollider ;
        float combinedVelocity;
        for (int i = 0; i < nonStaticColliderList.Count - 1; i++)
        {
            for (int j = i + 1; j < nonStaticColliderList.Count; j++)
            {
                if (!nonStaticColliderList[i].isActiveAndEnabled || !nonStaticColliderList[j].isActiveAndEnabled)
                {
                    continue;
                }

                xi = nonStaticColliderList[i].rigid.Velocity.x;
                xj = nonStaticColliderList[j].rigid.Velocity.x;
                if (!(nonStaticColliderList[i].rigid.isInAir ^ nonStaticColliderList[j].rigid.isInAir))
                {
                    if (Mathf.Abs(xj) > Mathf.Abs(xi))
                    {
                        fasterCollider = nonStaticColliderList[j];
                        slowerCollider = nonStaticColliderList[i];
                    }
                    else
                    {
                        fasterCollider = nonStaticColliderList[i];
                        slowerCollider = nonStaticColliderList[j];
                    }
                    if (!fasterCollider.ColliderIntersectHorizontally(slowerCollider))
                    {
                        continue;
                    }

                }
                else
                {
                    if (nonStaticColliderList[i].rigid.isInAir)
                    {
                        slowerCollider = nonStaticColliderList[j];
                        fasterCollider = nonStaticColliderList[i];
                    }
                    else
                    {
                        slowerCollider = nonStaticColliderList[i];
                        fasterCollider = nonStaticColliderList[j];
                    }
                    if (!fasterCollider.ColliderIntersectHorizontally(slowerCollider))
                    {
                        continue;
                    }
                    //CustomCollider2D temp = fasterCollider;
                    //fasterCollider = slowerCollider;
                    //slowerCollider = temp;
                }


                if ((xi > 0 && xj > 0) || (xi < 0 && xj < 0))
                {
                    combinedVelocity = Mathf.Sign(xi) * Mathf.Max(xi, xj);
                }
                else
                {
                    combinedVelocity = xi + xj;
                }
                fasterCollider.rigid.Velocity.x = combinedVelocity;
                slowerCollider.rigid.Velocity.x = combinedVelocity;
                slowerCollider.UpdateBoundsOfCollider();
                CustomCollider2D staticColliderThtWasHit;
                if (CheckForHorizontalCollisions(slowerCollider, out staticColliderThtWasHit))
                {
                    if (staticColliderThtWasHit != null)
                    {
                        fasterCollider.transform.position = new Vector3(slowerCollider.GetCenter().x - fasterCollider.colliderOffset.x, fasterCollider.transform.position.y, fasterCollider.transform.position.z);
                        Vector3 pos = fasterCollider.transform.position;
                        if (fasterCollider.GetCenter().x > staticColliderThtWasHit.GetCenter().x)
                        {

                            fasterCollider.transform.position = new Vector3(pos.x + .01f, pos.y, pos.z);
                        }
                        else
                        {
                            fasterCollider.transform.position = new Vector3(pos.x - .01f, pos.y, pos.z);
                        }
                        //print("Faster " + fasterCollider.name);
                        //print("Slower " + slowerCollider.name);
                    }
                    if (slowerCollider.rigid.Velocity.x > 0)
                    {
                        slowerCollider.rigid.isTouchingSide.x = 1;
                    }
                    else if (slowerCollider.rigid.Velocity.x < 0)
                    {
                        slowerCollider.rigid.isTouchingSide.x = -1;
                    }
                    slowerCollider.UpdateBoundsOfCollider();
                    fasterCollider.UpdateBoundsOfCollider();
                    fasterCollider.rigid.Velocity.x = 0;
                    slowerCollider.rigid.Velocity.x = 0;
                    if (fasterCollider.ColliderIntersectHorizontally(slowerCollider))
                    {
                        fasterCollider.UpdateBoundsOfCollider();
                        slowerCollider.UpdateBoundsOfCollider();
                    }

                }
            }
        }


        


        //Updates our physics object based on its physics state
        foreach (CustomPhysics2D rigid in allCustomPhysicsObjectsList)
        {
            if (rigid.enabled)
            {
                rigid.UpdatePhysics();
            }
        }


        foreach (CustomCollider2D collider in nonStaticColliderList)
        {
            collider.rigid.Velocity = collider.originalVelocity;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="colliderToCheck"></param>
    /// <param name=""></param>
    /// <returns></returns>
    public bool CheckForHorizontalCollisionWithNonstaticColliders(CustomCollider2D col1, CustomCollider2D col2)
    {

        CustomCollider2D fasterCollider;
        CustomCollider2D slowerCollider;
        if (col1.rigid.isInAir ^ col2.rigid.isInAir)
        {
            if (col1.rigid.isInAir)
            {
                fasterCollider = col1;
                slowerCollider = col2;
            }
            else
            {
                fasterCollider = col2;
                slowerCollider = col1;
            }
        }
        else
        {
            if (Mathf.Abs(col2.rigid.Velocity.x) > Mathf.Abs(col1.rigid.Velocity.x))
            {
                fasterCollider = col2;
                slowerCollider = col1;
            }
            else
            {
                fasterCollider = col1;
                slowerCollider = col2;
            }
        }

        if (fasterCollider.ColliderIntersectHorizontally(slowerCollider))
        {


            return true;
        }
        

        return false;
    }

    public bool CheckForHorizontalCollisions(CustomCollider2D colliderToCheck, out CustomCollider2D staticColliderThatWasHit)
    {
        staticColliderThatWasHit = null;
        foreach (CustomCollider2D staticCollider in staticColliderList)
        {
            if (colliderToCheck.ColliderIntersectHorizontally(staticCollider))
            {
                staticColliderThatWasHit = staticCollider;
                return true;
            }
        }
        return false;
    }
    #endregion monobehaviour methods

    #region collider interaction methods


    /// <summary>
    /// Add a physics object to the manager.
    /// </summary>
    /// <param name="rigid"></param>
    public void AddCustomPhysics(CustomPhysics2D rigid) 
    {
        if (allCustomPhysicsObjectsList.Contains(rigid))
        {
            return;
        }
        allCustomPhysicsObjectsList.Add(rigid);
    }

    /// <summary>
    /// Remove a physics object from the manager if is present in the manager
    /// </summary>
    /// <param name="rigid"></param>
    public void RemoveCustomPhysics(CustomPhysics2D rigid)
    {
        if (allCustomPhysicsObjectsList.Contains(rigid))
        {
            allCustomPhysicsObjectsList.Remove(rigid);
        }
    }

    /// <summary>
    /// This will add a collider to the appropriate list. The list that it is assigned to will be determined by whether or not
    /// it uses a rigid body to move. (i.e. whether or not it is static
    /// </summary>
    /// <param name="collider"></param>
    public void AddColliderToManager(CustomCollider2D collider)
    {
        if (collider.isStatic)
        {
            if (staticColliderList.Contains(collider))
            {
                Debug.LogWarning("We are trying to add a collider to our static collider list multiplie times");
            }
            else
            {
                staticColliderList.Add(collider);
            }
        }
        else
        {
            if (nonStaticColliderList.Contains(collider))
            {
                Debug.LogWarning("We are trying to add a collider to our non static collider list multiplie times");
            }
            else
            {
                nonStaticColliderList.Add(collider);
            }
        }
    }

    /// <summary>
    /// Removes a collider object from our physics manager. This should typically only be called upon the collider
    /// object being destroyed
    /// </summary>
    /// <param name="collider"></param>
    public void RemoveColliderFromManager(CustomCollider2D collider)
    {
        if (collider.isStatic)
        {
            if (staticColliderList.Contains(collider))
            {
                staticColliderList.Remove(collider);
            }
        }
        else
        {
            if (nonStaticColliderList.Contains(collider))
            {
                nonStaticColliderList.Remove(collider);
            }
        }
    }
    #endregion collider interaction methods

    /// <summary>
    /// This will compile a list of all the colliders that intersect with the line that is passed into our method
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
        foreach (CustomCollider2D coll in nonStaticColliderList)
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
