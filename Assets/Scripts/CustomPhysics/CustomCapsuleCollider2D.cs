using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCapsuleCollider2D : CustomCollider2D
{
    public float size = 1;
    public float radius = 1;

    private bool drawHorizontal;
    public Vector2 capsuleOffset;

    public float colliderBuffer = .01f;
    public BoundsCapsule bounds;
    public BoundsCapsule verticalBounds;
    public BoundsCapsule horizontalBounds;

    #region monobehaviour methods
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

        Color colorToDraw = GIZMO_COLOR;
        
#if UNITY_EDITOR
        if (!drawHorizontal)
        {
            DebugSettings.DrawLine(bounds.rectBounds.topLeft, bounds.rectBounds.bottomLeft, colorToDraw);
            DebugSettings.DrawLine(bounds.rectBounds.topRight, bounds.rectBounds.bottomRight, colorToDraw);

            Vector2 offsetToCenter = (bounds.rectBounds.topRight - bounds.rectBounds.topLeft) / 2f;
            UnityEditor.Handles.color = colorToDraw;
            UnityEditor.Handles.DrawWireDisc(bounds.topCircleBounds.center, Vector3.forward, radius);
            UnityEditor.Handles.DrawWireDisc(bounds.bottomCircleBounds.center, Vector3.forward, radius);
        }
#endif
    }
#endregion monobehaviour methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public override bool LineIntersectWithCollider(Vector2 origin, Vector2 direction, float length)
    {
        return CustomCollider2D.LineIntersectsCapsule(bounds, origin, direction, length);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collider"></param>
    public override void PushObjectOutsideOfCollider(CustomCollider2D collider)
    {
        throw new System.NotImplementedException();
    }

    
    /// <summary>
    /// Updates the bounds of the capsule collider. The Capsule collider is made up of one rect bounds and two circle bounds
    /// </summary>
    public override void UpdateBoundsOfCollider()
    {
        BoundsRect b = new BoundsRect();
        Vector2 origin = this.transform.position + new Vector3(capsuleOffset.x, capsuleOffset.y);
        float xSize = drawHorizontal ? size : radius * 2;
        float ySize = drawHorizontal ? radius * 2 : size;


        b.topLeft = origin + Vector2.up * ySize / 2 - Vector2.right * xSize / 2;
        b.topRight = origin + Vector2.up * ySize / 2 + Vector2.right * xSize / 2;
        b.bottomLeft = origin - Vector2.up * ySize / 2 - Vector2.right * xSize / 2;
        b.bottomRight = origin - Vector2.up * ySize / 2 + Vector2.right * xSize / 2;
        bounds.rectBounds = b;

        BoundsCircle topCircle = new BoundsCircle();
        topCircle.center = b.topLeft + (b.topRight - b.topLeft) / 2f;
        topCircle.radius = radius;
        bounds.topCircleBounds = topCircle;

        BoundsCircle bottomCircle = new BoundsCircle();
        bottomCircle.center = b.bottomLeft + (b.bottomRight - b.bottomLeft) / 2f;
        bottomCircle.radius = radius;
        bounds.bottomCircleBounds = bottomCircle;

        if (!isStatic)
        {
            verticalBounds = bounds;

            verticalBounds.topCircleBounds.radius = bounds.topCircleBounds.radius - colliderBuffer;
            verticalBounds.bottomCircleBounds.radius = bounds.bottomCircleBounds.radius - colliderBuffer;
            verticalBounds.rectBounds.topLeft.x = bounds.rectBounds.topLeft.x + colliderBuffer / 2;
            verticalBounds.rectBounds.topRight.x = bounds.rectBounds.topRight.x - colliderBuffer / 2;
            verticalBounds.rectBounds.bottomLeft.x = verticalBounds.rectBounds.topLeft.x;
            verticalBounds.rectBounds.bottomRight.x = verticalBounds.rectBounds.topRight.x;


            horizontalBounds = bounds;
            horizontalBounds.topCircleBounds.center += Vector2.down * colliderBuffer;
            horizontalBounds.bottomCircleBounds.center += Vector2.up * colliderBuffer;

            Vector2 horizontalOffset = Vector2.zero;
            Vector2 verticalOffset = Vector2.zero;

            if (rigid.velocity.y > 0)
            {
                verticalOffset = Vector2.up * Mathf.Max(colliderBuffer, rigid.velocity.y * Overseer.DELTA_TIME);
            }
            else if(rigid.velocity.y < 0)
            {
                verticalOffset = Vector2.up * Mathf.Min(-colliderBuffer, rigid.velocity.y * Overseer.DELTA_TIME);
            }

            if (rigid.velocity.x > 0)
            {
                horizontalOffset = Vector2.right * Mathf.Max(colliderBuffer, rigid.velocity.x * Overseer.DELTA_TIME);
            }
            else if (rigid.velocity.x < 0)
            {
                horizontalOffset = Vector2.right * Mathf.Min(-colliderBuffer, rigid.velocity.x * Overseer.DELTA_TIME);
            }

            verticalBounds.SetOffset(verticalOffset);
            horizontalBounds.SetOffset(horizontalOffset);
        }
    }

    public override Vector2 GetLowerBoundsAtXValue(float x)
    {
        return CustomCollider2D.GetLowerBoundsAtXValueCircle(bounds.bottomCircleBounds, x);
    }

    public override Vector2 GetUpperBoundsAtXValue(float x)
    {
        return CustomCollider2D.GetUpperBoundsAtXValueCircle(bounds.topCircleBounds, x);
    }

    public override Vector2 GetRightBoundAtYValue(float y)
    {
        if (y > bounds.rectBounds.topLeft.y)
        {
            return CustomCollider2D.GetRighBoundAtYValueCircle(bounds.topCircleBounds, y);
        }
        else if (y < bounds.rectBounds.bottomLeft.y)
        {
            return CustomCollider2D.GetRighBoundAtYValueCircle(bounds.bottomCircleBounds, y);
        }
        else
        {
            return CustomCollider2D.GetRighBoundAtYValueRect(bounds.rectBounds, y);
        }
    }

    public override Vector2 GetLeftBoundAtYValue(float y)
    {
        if (y > bounds.rectBounds.topLeft.y)
        {
            return CustomCollider2D.GetLeftBoundAtYValueCircle(bounds.topCircleBounds, y);
        }
        else if (y < bounds.rectBounds.bottomLeft.y)
        {
            return CustomCollider2D.GetLeftBoundAtYValueCircle(bounds.bottomCircleBounds, y);
        }
        else
        {
            return CustomCollider2D.GetLeftBoundAtYValueRect(bounds.rectBounds, y);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="boundsToCheck"></param>
    /// <param name="colliderToCheck"></param>
    /// <returns></returns>
    private bool ColliderIntersectBounds(BoundsCapsule boundsToCheck, CustomCollider2D colliderToCheck)
    {
        if (colliderToCheck is CustomBoxCollider2D)
        {
            return CapsuleIntersectRect(boundsToCheck, ((CustomBoxCollider2D)colliderToCheck).bounds);
        }
        else if (colliderToCheck is CustomCircleCollider2D)
        {
            return CapsuleIntersectCircle(boundsToCheck, ((CustomCircleCollider2D)colliderToCheck).bounds);
        }
        else if (colliderToCheck is CustomCapsuleCollider2D)
        {
            return CapsuleIntersectCapsule(((CustomCapsuleCollider2D)colliderToCheck).bounds, boundsToCheck);
        }
        else
        {
            return false;
        }
    }

    public override bool ColliderIntersect(CustomCollider2D colliderToCheck)
    {
        return false;

    }

    public override Vector2 GetCenter()
    {
        return bounds.bottomCircleBounds.center + (bounds.topCircleBounds.center - bounds.bottomCircleBounds.center) / 2f;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="colliderToCheck"></param>
    /// <returns></returns>
    public override bool ColliderIntersectVertically(CustomCollider2D colliderToCheck)
    {


        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="colliderToCheck"></param>
    /// <returns></returns>
    public override bool ColliderIntersectHorizontally(CustomCollider2D colliderToCheck)
    {
        return false;
    }
}
