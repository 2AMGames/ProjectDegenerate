using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class CustomBoxCollider2D : CustomCollider2D
{
    public Vector2 boxColliderSize = Vector2.one;
    public Vector2 boxColliderPosition;
    [Tooltip("We will thin out the box collider horizontally when checking for collisions with our box collider")]
    public float HorizontalBuffer = .02f;
    [Tooltip("We will thin our box collider vertically to check our horizontal collisions")]
    public float VerticalBuffer = .02f;
    /// <summary>
    /// 
    /// </summary>
    public BoundsRect bounds { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected BoundsRect previousBounds { get; set; }

    protected BoundsRect horizontalCheckBounds;
    protected BoundsRect verticalCheckBounds;

    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }


        

#if UNITY_EDITOR
        if (!isStatic)
        {
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new Rect(this.verticalCheckBounds.topLeft.x, this.verticalCheckBounds.topLeft.y, verticalCheckBounds.bottomRight.x - verticalCheckBounds.topLeft.x, verticalCheckBounds.bottomRight.y - verticalCheckBounds.topLeft.y),
                Color.cyan, Color.blue);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new Rect(this.horizontalCheckBounds.topLeft.x, this.horizontalCheckBounds.topLeft.y, horizontalCheckBounds.bottomRight.x - horizontalCheckBounds.topLeft.x, horizontalCheckBounds.bottomRight.y - horizontalCheckBounds.topLeft.y),
                Color.red, Color.yellow);
        }

#endif
        Color colorToDraw = GIZMO_COLOR;

        DebugSettings.DrawLine(bounds.bottomLeft, bounds.bottomRight, colorToDraw);
        DebugSettings.DrawLine(bounds.bottomRight, bounds.topRight, colorToDraw);
        DebugSettings.DrawLine(bounds.topRight, bounds.topLeft, colorToDraw);
        DebugSettings.DrawLine(bounds.topLeft, bounds.bottomLeft, colorToDraw);
    }



    /// <summary>
    /// This should be called by our HitboxManager
    /// </summary>
    public override void UpdateBoundsOfCollider()
    {
        previousBounds = bounds;
        
        BoundsRect b = new BoundsRect();
        Vector2 origin = this.transform.position + new Vector3(boxColliderPosition.x, boxColliderPosition.y);

        b.center = origin;
        b.topLeft = origin + Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        b.topRight = origin + Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;
        b.bottomLeft = origin - Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        b.bottomRight = origin - Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;

        this.bounds = b;

        if (!isStatic)
        {
            verticalCheckBounds = this.bounds;
            Vector2 verticalOffset = Mathf.Sign(rigid.velocity.y) * Mathf.Max(Mathf.Abs(rigid.velocity.y * Overseer.DELTA_TIME), VerticalBuffer) * Vector2.up;
            verticalCheckBounds.SetOffset(verticalOffset);

            horizontalCheckBounds = this.bounds;
            Vector2 horizontalOffset = Mathf.Sign(rigid.velocity.x) * Mathf.Max(Mathf.Abs(rigid.velocity.x * Overseer.DELTA_TIME), HorizontalBuffer) * Vector2.right;
            horizontalCheckBounds.SetOffset(horizontalOffset);

            verticalCheckBounds.topLeft.x = this.bounds.bottomLeft.x + HorizontalBuffer / 2f;
            verticalCheckBounds.bottomLeft.x = verticalCheckBounds.topLeft.x;
            verticalCheckBounds.topRight.x = this.bounds.topRight.x - HorizontalBuffer / 2f;
            verticalCheckBounds.bottomRight.x = verticalCheckBounds.topRight.x;

            float verticalBufferOffset = Mathf.Max(VerticalBuffer / 2, Mathf.Abs(verticalOffset.y));
            float horizontalBufferOffset = Mathf.Max(HorizontalBuffer / 2, Mathf.Abs(horizontalOffset.x));

            if (rigid.velocity.y > 0)
            {
                verticalCheckBounds.bottomLeft.y = bounds.bottomLeft.y + verticalBufferOffset;
                verticalCheckBounds.bottomRight.y = bounds.bottomRight.y + verticalBufferOffset;
            }
            else if (rigid.velocity.y < 0)
            {
                verticalCheckBounds.topLeft.y = bounds.topLeft.y - verticalBufferOffset;
                verticalCheckBounds.topRight.y = bounds.topRight.y - verticalBufferOffset;
            }

            horizontalCheckBounds.bottomLeft.y = this.bounds.bottomLeft.y + VerticalBuffer / 2f;
            horizontalCheckBounds.bottomRight.y = horizontalCheckBounds.bottomLeft.y;
            horizontalCheckBounds.topLeft.y = this.bounds.topLeft.y - VerticalBuffer / 2f;
            horizontalCheckBounds.topRight.y = horizontalCheckBounds.topLeft.y;

            if (rigid.velocity.x > 0)
            {
                horizontalCheckBounds.topLeft.x = bounds.topLeft.x + horizontalBufferOffset;
                horizontalCheckBounds.bottomLeft.x = bounds.bottomLeft.x + horizontalBufferOffset;
            }
            else if (rigid.velocity.x < 0)
            {
                horizontalCheckBounds.topRight.x = bounds.topRight.x - horizontalBufferOffset;
                horizontalCheckBounds.bottomRight.x = bounds.bottomRight.x - horizontalBufferOffset;
            }
        }
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public override bool LineIntersectWithCollider(Vector2 origin, Vector2 direction, float length)
    {
        return LineIntersectRect(this.bounds, origin, direction, length);
    }

    

   
   
    /// <summary>
    /// Whenever we intersect with a collider this method should be called to move the collider outside
    /// </summary>
    public override void PushObjectOutsideOfCollider(CustomCollider2D collider)
    {
        if (collider.isStatic)
        {
            return;
        }
        if (!(collider is CustomBoxCollider2D))
        {
            return;
        }
        CustomBoxCollider2D bCollider = (CustomBoxCollider2D)collider;
        Vector2 tr1 = previousBounds.topRight;
        Vector2 bl1 = previousBounds.bottomLeft;

        Vector2 tr2 = bCollider.previousBounds.topRight;
        Vector2 bl2 = bCollider.previousBounds.bottomLeft;

        Vector2 upRightVec = tr1 - bl2;
        Vector2 downLeftVec = tr2 - bl1;
        

        if (downLeftVec.x <= 0)
        {
            bCollider.transform.position = new Vector3(bounds.bottomLeft.x + (bCollider.transform.position.x - tr2.x) - .01f, bCollider.transform.position.y, bCollider.transform.position.z);

        }
        if (downLeftVec.y <= 0)
        {
            bCollider.transform.position = new Vector3(bCollider.transform.position.x, bounds.bottomLeft.y - (bCollider.transform.position.y - tr2.y), bCollider.transform.position.z);
        }
        if (upRightVec.x <= 0)
        {
            bCollider.transform.position = new Vector3(bounds.topRight.x - (-bCollider.transform.position.x + bl2.x) + .01f, bCollider.transform.position.y, bCollider.transform.position.z);

        }
        if (upRightVec.y <= 0)
        {
            bCollider.transform.position = new Vector3(bCollider.transform.position.x, bounds.topRight.y + (-bCollider.transform.position.y + bl2.y), bCollider.transform.position.z);
        }

        bCollider.UpdateBoundsOfCollider();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public override Vector2 GetLowerBoundsAtXValue(float x)
    {
        return GetLowerBoundsAtXValueRect(this.bounds, x);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public override Vector2 GetUpperBoundsAtXValue(float x)
    {
        return GetUpperBoundsAtXValueRect(this.bounds, x);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public override Vector2 GetRighBoundAtYValue(float y)
    {
        return GetRighBoundAtYValueRect(this.bounds, y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public override Vector2 GetLeftBoundAtYValue(float y)
    {
        return GetLeftBoundAtYValueRect(this.bounds, y);
    }

    public override Vector2 GetCenter()
    {
        return bounds.center;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="colliderToCheck"></param>
    /// <returns></returns>
    public override bool ColliderIntersect(CustomCollider2D colliderToCheck)
    {
        return ColliderIntersectBounds(this.bounds, colliderToCheck);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="boundsToCheck"></param>
    /// <param name="colliderToCheck"></param>
    /// <returns></returns>
    private bool ColliderIntersectBounds(BoundsRect boundsToCheck, CustomCollider2D colliderToCheck)
    {
        if (colliderToCheck is CustomBoxCollider2D)
        {
            return RectIntersectRect(boundsToCheck, ((CustomBoxCollider2D)colliderToCheck).bounds);
        }
        else if (colliderToCheck is CustomCircleCollider2D)
        {
            return RectIntersectCircle(boundsToCheck, ((CustomCircleCollider2D)colliderToCheck).bounds);
        }
        else if (colliderToCheck is CustomCapsuleCollider2D)
        {
            return CapsuleIntersectRect(((CustomCapsuleCollider2D)colliderToCheck).bounds, boundsToCheck);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="colliderToCheck"></param>
    /// <param name="offsetDirection"></param>
    /// <returns></returns>
    public override bool ColliderIntersectVertically(CustomCollider2D colliderToCheck)
    {
        if (colliderToCheck == this) return false;


        if (rigid.velocity.y == 0)
        {
            return false;
        }

        if (ColliderIntersectBounds(verticalCheckBounds, colliderToCheck))
        {
            if (colliderToCheck is CustomBoxCollider2D)
            {
                if (rigid.velocity.y >= 0)
                {
                    float collisionPoint = bounds.topLeft.x;

                    float yPosition = colliderToCheck.GetLowerBoundsAtXValue(collisionPoint).y - (GetUpperBoundsAtXValue(collisionPoint).y - transform.position.y);
                    this.transform.position = new Vector3(this.transform.position.x, yPosition, this.transform.position.z);

                    rigid.velocity.y = 0;
                }
                else
                {
                    float collisionPoint = bounds.topRight.x;

                    float yPosition = colliderToCheck.GetUpperBoundsAtXValue(collisionPoint).y - (GetLowerBoundsAtXValue(collisionPoint).y - transform.position.y);
                    this.transform.position = new Vector3(this.transform.position.x, yPosition, this.transform.position.z);

                    rigid.velocity.y = 0;
                }
            }
            else if (colliderToCheck is CustomCircleCollider2D)
            {
                Vector2 collisionPoint = IntersectionPointRectOnCircle(this.bounds, ((CustomCircleCollider2D)colliderToCheck).bounds);
                if (rigid.velocity.y >= 0)
                {
                    float yPosition = colliderToCheck.GetLowerBoundsAtXValue(collisionPoint.x).y - (GetUpperBoundsAtXValue(collisionPoint.x).y - transform.position.y);
                    this.transform.position = new Vector3(this.transform.position.x, yPosition, this.transform.position.z);

                    rigid.velocity.y = 0;
                }
                else
                {
                    float yPosition = colliderToCheck.GetUpperBoundsAtXValue(collisionPoint.x).y - (GetLowerBoundsAtXValue(collisionPoint.x).y - transform.position.y);
                    this.transform.position = new Vector3(this.transform.position.x, yPosition, this.transform.position.z);

                    rigid.velocity.y = 0;
                }
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
        if (colliderToCheck == this) return false;

        if (rigid.velocity.x == 0) return false;

        if (ColliderIntersectBounds(horizontalCheckBounds, colliderToCheck))
        {
            if (colliderToCheck is CustomBoxCollider2D)
            {
                if (rigid.velocity.x >= 0)
                {
                    float collisionPoint = bounds.topRight.y;

                    float xPosition = colliderToCheck.GetLeftBoundAtYValue(collisionPoint).x - (GetRighBoundAtYValue(collisionPoint).x - transform.position.x);
                    this.transform.position = new Vector3(xPosition, this.transform.position.y, this.transform.position.z);
                }
                else
                {
                    float collisionPoint = bounds.topRight.y;

                    float xPosition = colliderToCheck.GetRighBoundAtYValue(collisionPoint).x - (GetLeftBoundAtYValue(collisionPoint).x - transform.position.x);
                    this.transform.position = new Vector3(xPosition, this.transform.position.y, this.transform.position.z);
                }
            }
            else if (colliderToCheck is CustomCircleCollider2D)
            {
                Vector2 collisionPoint = IntersectionPointRectOnCircle(this.bounds, ((CustomCircleCollider2D)colliderToCheck).bounds);
                if (rigid.velocity.x >= 0)
                {
                    float xPosition = colliderToCheck.GetLeftBoundAtYValue(collisionPoint.y).x - (GetRighBoundAtYValue(collisionPoint.y).x - transform.position.x);
                    this.transform.position = new Vector3(xPosition, this.transform.position.y, this.transform.position.z);
                }
                else
                {
                    float xPosition = colliderToCheck.GetRighBoundAtYValue(collisionPoint.y).x - (GetLeftBoundAtYValue(collisionPoint.y).x - transform.position.x);
                    this.transform.position = new Vector3(xPosition, this.transform.position.y, this.transform.position.z);
                }
            }
            return true;
        }
        return false;
    }
}
