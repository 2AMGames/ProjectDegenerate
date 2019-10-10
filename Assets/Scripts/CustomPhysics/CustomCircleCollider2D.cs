using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCircleCollider2D : CustomCollider2D
{

    [Header("Kinematic Collision Buffers")]
    public float radiusBuffer = .01f;

    public float radius = 1;
    public Vector2 centerOffset;


    public BoundsCircle bounds;

    private BoundsCircle horizontalBoundsFromVelocity;
    private BoundsCircle verticalBoundsFromVelocity;

    #region monobehaviour methods

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

#if UNITY_EDITOR
        

        if (!isStatic)
        {
            //UnityEditor.Handles.color = Color.red;
            //UnityEditor.Handles.DrawSolidDisc(horizontalBoundsFromVelocity.center, Vector3.forward, horizontalBoundsFromVelocity.radius);
            //UnityEditor.Handles.color = Color.cyan;
            //UnityEditor.Handles.DrawSolidDisc(verticalBoundsFromVelocity.center, Vector3.forward, verticalBoundsFromVelocity.radius);
        }
        UnityEditor.Handles.color = GIZMO_COLOR;
        UnityEditor.Handles.DrawWireDisc(bounds.center, Vector3.forward, bounds.radius);

#endif
    }
    #endregion monobehaviour methods
    /// <summary>
    /// Override method. Returns whether or not a line passes through this circle collider
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public override bool LineIntersectWithCollider(Vector2 origin, Vector2 direction, float length)
    {
        return LineIntersectCircle(this.bounds, origin, direction, length);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="collider"></param>
    public override void PushObjectOutsideOfCollider(CustomCollider2D collider)
    {
        

    }


    /// <summary>
    /// 
    /// </summary>
    public override void UpdateBoundsOfCollider()
    {

        BoundsCircle cBounds = new BoundsCircle();
        cBounds.center = this.transform.position + new Vector3(colliderOffset.x, colliderOffset.y);
        cBounds.radius = radius;
        bounds = cBounds;

        if (!isStatic)
        {
            //Adjust the vertical and horizontal bounds of our circle collider if it is a static collider

            this.horizontalBoundsFromVelocity = bounds;
            this.horizontalBoundsFromVelocity.radius = bounds.radius - radiusBuffer;

            this.verticalBoundsFromVelocity = bounds;
            this.verticalBoundsFromVelocity.radius = bounds.radius - radiusBuffer;



            if (rigid.Velocity.y < 0)
            {
                verticalBoundsFromVelocity.center = bounds.center + Vector2.up * Mathf.Min(-radiusBuffer,  rigid.Velocity.y * Overseer.DELTA_TIME);
            }
            else if (rigid.Velocity.y > 0)
            {
                verticalBoundsFromVelocity.center = bounds.center + Vector2.up * Mathf.Max(radiusBuffer, rigid.Velocity.y * Overseer.DELTA_TIME);
            }
            if (rigid.Velocity.x < 0)
            {
                horizontalBoundsFromVelocity.center = bounds.center + Vector2.right * Mathf.Min(-radiusBuffer, rigid.Velocity.x * Overseer.DELTA_TIME);
            }
            else if (rigid.Velocity.x > 0)
            {
                horizontalBoundsFromVelocity.center = bounds.center + Vector2.right * Mathf.Max(radiusBuffer, rigid.Velocity.x * Overseer.DELTA_TIME);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public override Vector2 GetLowerBoundsAtXValue(float x)
    {
        return GetLowerBoundsAtXValueCircle(this.bounds, x);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public override Vector2 GetUpperBoundsAtXValue(float x)
    {
        return GetUpperBoundsAtXValueCircle(this.bounds, x);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public override Vector2 GetRightBoundAtYValue(float y)
    {
        return GetRighBoundAtYValueCircle(this.bounds, y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public override Vector2 GetLeftBoundAtYValue(float y)
    {
        return GetLeftBoundAtYValueCircle(this.bounds, y);
    }

    public override bool ColliderIntersect(CustomCollider2D colliderToCheck)
    {
        return CircleColliderCollisionsAtBounds(this.bounds, colliderToCheck);
    }

    private bool CircleColliderCollisionsAtBounds(CustomCollider2D.BoundsCircle cBounds, CustomCollider2D colliderToCheck)
    {
        if (colliderToCheck is CustomBoxCollider2D)
        {
            return RectIntersectCircle(((CustomBoxCollider2D)colliderToCheck).bounds, cBounds);
        }
        else if (colliderToCheck is CustomCircleCollider2D)
        {
            return CircleIntersectCircle(cBounds, ((CustomCircleCollider2D)colliderToCheck).bounds);
        }
        else if (colliderToCheck is CustomCapsuleCollider2D)
        {
            return CapsuleIntersectCircle(((CustomCapsuleCollider2D)colliderToCheck).bounds, cBounds);
        }
        else
        {
            Debug.LogError("Circle Collider does not support type: " + colliderToCheck.GetType());
            return false;
        }
    }

    public override Vector2 GetCenter()
    {
        return bounds.center;
    }

    /// <summary>
    /// Checks to see if our circle would collide with the collider object that is passed in. 
    /// </summary>
    /// <param name="colliderToCheck"></param>
    /// <param name="offsetDirection"></param>
    /// <returns></returns>
    public override bool ColliderIntersectVertically(CustomCollider2D colliderToCheck)
    {
        if (rigid.Velocity.y == 0)
        {
            return false;
        }

        if (CircleColliderCollisionsAtBounds(verticalBoundsFromVelocity, colliderToCheck))
        {
            if (colliderToCheck is CustomBoxCollider2D)
            {
                Vector2 pointOfCollision = IntersectionPointNonstaticRectOnStaticCircle(((CustomBoxCollider2D)colliderToCheck), this, true);
                
                
                this.transform.position = new Vector3(this.transform.position.x, pointOfCollision.y, this.transform.position.z);
                rigid.Velocity.y = 0;

                return true;
            }
            else if (colliderToCheck is CustomCircleCollider2D)
            {
                CustomCircleCollider2D customcircleToCheck = (CustomCircleCollider2D)colliderToCheck;
                Vector2 collisionPoint = IntersectionPointCircleOnCircle(this, customcircleToCheck, true);
               
                this.transform.position = new Vector3(this.transform.position.x, collisionPoint.y, this.transform.position.z);

            }
            else if (colliderToCheck is CustomCapsuleCollider2D)
            {
                CustomCapsuleCollider2D capsuleToCheck = (CustomCapsuleCollider2D)colliderToCheck;

                Vector2 collisionPoint = IntersectionPointStaticCapsuleNonstaticCircle(capsuleToCheck, this, true);
                this.transform.position = new Vector3(this.transform.position.x, collisionPoint.y, this.transform.position.z);
                
            }
            return true;
        }
        
        return false;
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="colliderToCheck"></param>
    /// <returns></returns>
    public override bool ColliderIntersectHorizontally(CustomCollider2D colliderToCheck)
    {

        if (rigid.Velocity.x == 0)
        {
            return false;
        }

        if (CircleColliderCollisionsAtBounds(horizontalBoundsFromVelocity, colliderToCheck))
        {
            if (colliderToCheck is CustomBoxCollider2D)
            {
                Vector2 pointOfCollision = IntersectionPointNonstaticRectOnStaticCircle(((CustomBoxCollider2D)colliderToCheck), this, false);
                
                this.transform.position = new Vector3(pointOfCollision.x, this.transform.position.y, this.transform.position.z);

                return true;
            }
            else if (colliderToCheck is CustomCircleCollider2D)
            {
                CustomCircleCollider2D customcircleToCheck = (CustomCircleCollider2D)colliderToCheck;
                
                Vector2 collisionPoint = IntersectionPointCircleOnCircle(this, customcircleToCheck, false);
                this.transform.position = new Vector3(collisionPoint.x, this.transform.position.y, this.transform.position.z);
            }
            else if (colliderToCheck is CustomCapsuleCollider2D)
            {
                CustomCapsuleCollider2D capCollider = (CustomCapsuleCollider2D)colliderToCheck;

                Vector2 collisionPoint = IntersectionPointStaticCapsuleNonstaticCircle(capCollider, this, false);
                this.transform.position = new Vector3(collisionPoint.x, this.transform.position.y, this.transform.position.z);
            }
            return true;
        }
        return false;
    }
}
