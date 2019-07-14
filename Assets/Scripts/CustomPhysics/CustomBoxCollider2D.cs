using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBoxCollider2D : CustomCollider2D
{
    public Vector2 boxColliderSize = Vector2.one;
    public Vector2 boxColliderPosition;

    

    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

        Color colorToDraw = Color.green;

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
        base.UpdateBoundsOfCollider();
        
        ColliderBounds b = new ColliderBounds();
        Vector2 origin = this.transform.position + new Vector3(boxColliderPosition.x, boxColliderPosition.y);

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
}
