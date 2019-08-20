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

    /// <summary>
    /// 
    /// </summary>
    public BoundsRect bounds { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected BoundsRect previousBounds { get; set; }

    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

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
        
        Vector2 v0 = direction * length;
        Vector2 endpoint = origin + v0;

        Vector2 tr = bounds.topRight;
        Vector2 bl = bounds.bottomLeft;

        if (bl.x < origin.x && origin.x < tr.x && bl.y < origin.y && origin.y < tr.y)
        {
            return true;
        }


        if (LineCrossLine(origin, v0, bounds.bottomLeft, (bounds.bottomRight - bounds.bottomLeft)))
        {
            return true;
        }
        if (LineCrossLine(origin, v0, bounds.bottomRight, (bounds.topRight - bounds.bottomRight)))
        {
            return true;
        }
        if (LineCrossLine(origin, v0, bounds.topRight, (bounds.topLeft - bounds.topRight)))
        {
            return true;
        }
        if (LineCrossLine(origin, v0, bounds.topLeft, (bounds.bottomLeft - bounds.topLeft)))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    private bool LineCrossLine(Vector2 u0, Vector2 v0, Vector2 u1, Vector2 v1)
    {
        float d1 = GetDeterminant(v1, v0);
        if (d1 == 0)
        {
            return false;
        }
        

        float s = (1 / d1) * (((u0.x - u1.x) * v0.y) - ((u0.y - u1.y) * v0.x));
        float t = (1 / d1) * -((-(u0.x - u1.x) * v1.y) + ((u0.y - u1.y) * v1.x));
       
        return s > 0 && s < 1 && t > 0 && t < 1;
    }

    private float GetDeterminant(Vector2 v1, Vector2 v2)
    {
        return -v2.x * v1.y + v1.x * v2.y;
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

    public override Vector2 GetLowerBoundsAtXValue(float x)
    {
        return new Vector2(x, bounds.bottomLeft.y);
    }

    public override Vector2 GetUpperBoundsAtXValue(float x)
    {
        return new Vector2(x, bounds.topRight.y);
    }

    public override Vector2 GetRighBoundAtYValue(float y)
    {
        return new Vector2(bounds.topRight.x, y);
    }

    public override Vector2 GetLeftBoundAtYValue(float y)
    {
        return new Vector2(bounds.bottomLeft.x, y);
    }

    public override Vector2 GetCenter()
    {
        return bounds.center;
    }

    public override bool ColliderIntersect(CustomCollider2D colliderToCheck, out Vector2 intersectionPoint)
    {
        if (colliderToCheck is CustomBoxCollider2D)
        {
            return RectIntersectRect(this.bounds, ((CustomBoxCollider2D)colliderToCheck).bounds, out intersectionPoint);
        }
        else if (colliderToCheck is CustomCircleCollider2D)
        {
            return RectIntersectCircle(this.bounds, ((CustomCircleCollider2D)colliderToCheck).bounds, out intersectionPoint);
        }
        else if (colliderToCheck is CustomCapsuleCollider2D)
        {
            return CapsuleIntersectRect(((CustomCapsuleCollider2D)colliderToCheck).bounds, this.bounds, out intersectionPoint);
        }
        else
        {
            intersectionPoint = Vector2.zero;
            return false;
        }
    }
}
