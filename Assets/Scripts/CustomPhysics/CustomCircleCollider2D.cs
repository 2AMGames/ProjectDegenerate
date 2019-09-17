using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCircleCollider2D : CustomCollider2D
{

    [Header("Kinematic Collision Buffers")]
    public float radiusBuffer = .01f;

    public float radius = 1;
    public Vector2 centerOffset;


    public BoundsCircle previousBounds;
    public BoundsCircle bounds;

    private BoundsCircle horizontalBoundsFromVelocity;
    private BoundsCircle verticalBoundsFromVelocity;

    #region monobehaviour methods
    protected override void OnValidate()
    {
        base.OnValidate();
        
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

#if UNITY_EDITOR
        

        if (!isStatic)
        {
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawSolidDisc(horizontalBoundsFromVelocity.center, Vector3.forward, horizontalBoundsFromVelocity.radius);
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.DrawSolidDisc(verticalBoundsFromVelocity.center, Vector3.forward, verticalBoundsFromVelocity.radius);
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
        if (collider.isStatic)
        {
            return;
        }
        CustomPhysics2D rigidCol = collider.rigid;
        Vector2 offsetFromCollider = this.bounds.center - collider.GetCenter();

    }


    /// <summary>
    /// 
    /// </summary>
    public override void UpdateBoundsOfCollider()
    {
        previousBounds = bounds;

        BoundsCircle cBounds = new BoundsCircle();
        cBounds.center = this.transform.position;
        cBounds.radius = radius;
        bounds = cBounds;

        if (!isStatic)
        {
            //Adjust the vertical and horizontal bounds of our circle collider if it is a static collider

            this.horizontalBoundsFromVelocity = bounds;
            this.horizontalBoundsFromVelocity.radius = bounds.radius - radiusBuffer;

            this.verticalBoundsFromVelocity = bounds;
            this.verticalBoundsFromVelocity.radius = bounds.radius - radiusBuffer;



            if (rigid.velocity.y < 0)
            {
                verticalBoundsFromVelocity.center = bounds.center + Vector2.down * radiusBuffer + Vector2.up * rigid.velocity.y * Overseer.DELTA_TIME;
            }
            else if (rigid.velocity.y > 0)
            {
                verticalBoundsFromVelocity.center = bounds.center + Vector2.up * radiusBuffer + Vector2.up * rigid.velocity.y * Overseer.DELTA_TIME;
            }
            if (rigid.velocity.x < 0)
            {
                horizontalBoundsFromVelocity.center = bounds.center + Vector2.left * radiusBuffer + Vector2.right * rigid.velocity.x * Overseer.DELTA_TIME;
            }
            else if (rigid.velocity.x > 0)
            {
                horizontalBoundsFromVelocity.center = bounds.center + Vector2.right * radiusBuffer + Vector2.right * rigid.velocity.x * Overseer.DELTA_TIME;
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
    public override Vector2 GetRighBoundAtYValue(float y)
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
        if (rigid == null || colliderToCheck == this)
        {
            return false;
        }

        

        if (rigid.velocity.y == 0)
        {
            return false;
        }

        if (CircleColliderCollisionsAtBounds(verticalBoundsFromVelocity, colliderToCheck))
        {
            Vector2 closestCollisionPoint;
            if (colliderToCheck is CustomBoxCollider2D)
            {
                Vector2 pointOfCollision = IntersectionPointRectOnCircle(((CustomBoxCollider2D)colliderToCheck).bounds, this.bounds);
                if (rigid.velocity.y > 0)
                {
                    closestCollisionPoint = GetUpperBoundsAtXValue(pointOfCollision.x);
                }
                else 
                {
                    closestCollisionPoint = GetLowerBoundsAtXValue(pointOfCollision.x);
                }
                
                
                pointOfCollision.y = pointOfCollision.y - (closestCollisionPoint.y - bounds.center.y);
                this.transform.position = new Vector3(this.transform.position.x, pointOfCollision.y, this.transform.position.z);
                rigid.velocity.y = 0;

                return true;
            }
            if (colliderToCheck is CustomCircleCollider2D)
            {
                CustomCircleCollider2D customcircleToCheck = (CustomCircleCollider2D)colliderToCheck;
                float totalRadiusSize = bounds.radius + customcircleToCheck.bounds.radius;
                float xCollision = bounds.center.x + (colliderToCheck.GetCenter().x - bounds.center.x) * (bounds.radius) / totalRadiusSize;
                Vector2 collisionPoint;
                if (rigid.velocity.y > 0)
                {
                    collisionPoint = colliderToCheck.GetLowerBoundsAtXValue(xCollision);
                    collisionPoint.y = collisionPoint.y - (GetUpperBoundsAtXValue(xCollision).y - bounds.center.y);
                    //print(collisionPoint.y);
                }
                else
                {
                    collisionPoint = colliderToCheck.GetUpperBoundsAtXValue(xCollision);
                    collisionPoint.y = collisionPoint.y - (GetLowerBoundsAtXValue(xCollision).y - bounds.center.y);
                }
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

        if (rigid.velocity.x == 0)
        {
            return false;
        }

        if (CircleColliderCollisionsAtBounds(horizontalBoundsFromVelocity, colliderToCheck))
        {
            Vector2 closestCollisionPoint;
            if (colliderToCheck is CustomBoxCollider2D)
            {
                Vector2 pointOfCollision = IntersectionPointRectOnCircle(((CustomBoxCollider2D)colliderToCheck).bounds, this.bounds);
                if (rigid.velocity.x > 0)
                {
                    closestCollisionPoint = GetRighBoundAtYValue(pointOfCollision.y);
                }
                else
                {
                    closestCollisionPoint = GetLeftBoundAtYValue(pointOfCollision.y);
                }


                pointOfCollision.x = pointOfCollision.x - (closestCollisionPoint.x - bounds.center.x);
                this.transform.position = new Vector3(pointOfCollision.x, this.transform.position.y, this.transform.position.z);

                return true;
            }
            else if (colliderToCheck is CustomCircleCollider2D)
            {
                CustomCircleCollider2D customcircleToCheck = (CustomCircleCollider2D)colliderToCheck;
                float totalRadiusSize = bounds.radius + customcircleToCheck.bounds.radius;
                float yCollision = bounds.center.y + (colliderToCheck.GetCenter().y - bounds.center.y) * (bounds.radius) / totalRadiusSize;
                Vector2 collisionPoint;
                if (rigid.velocity.x > 0)
                {
                    collisionPoint = colliderToCheck.GetLeftBoundAtYValue(yCollision);
                    collisionPoint.x = collisionPoint.x - (GetRighBoundAtYValue(yCollision).x - bounds.center.x);
                }
                else
                {
                    collisionPoint = colliderToCheck.GetRighBoundAtYValue(yCollision);
                    collisionPoint.x = collisionPoint.x - (GetLeftBoundAtYValue(yCollision).x - bounds.center.x);
                }

                this.transform.position = new Vector3(collisionPoint.x, this.transform.position.y, this.transform.position.z);
            }
            return true;
        }
        return false;
    }
}
