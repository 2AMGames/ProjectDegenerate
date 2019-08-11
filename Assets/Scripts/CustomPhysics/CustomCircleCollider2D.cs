using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCircleCollider2D : CustomCollider2D
{
    public float radius = 1;
    public BoundsCircle previousBounds;
    public BoundsCircle bounds;

    #region monobehaviour methods
    protected override void OnValidate()
    {
        base.OnValidate();
        if (verticalRayCount % 2 == 0)
        {
            verticalRayCount += 1;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

#if UNITY_EDITOR
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
        return LineIntersectCircle(this.bounds, origin, origin + direction * length);
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
    /// <returns></returns>
    protected override bool CheckCollisionUpFromVelocity()
    {
        float degreeOffset = 180f / (verticalRayCount - 1);
        Vector2 direction = Vector2.zero;
        int centerIndex = horizontalRayCount / 2 + 1;
        for (int i = 0; i < horizontalRayCount; i++)
        {
            float rads = Mathf.Deg2Rad * (degreeOffset + 180);
            direction = new Vector2(Mathf.Cos(rads), Mathf.Sin(rads));
            Vector2 originPoint = bounds.center + bounds.radius * direction;
            List<CustomCollider2D> colliderList;
            Overseer.Instance.ColliderManager.CheckLineIntersectWithCollider(originPoint, Vector2.down, VerticalBuffer + rigid.velocity.y * Time.deltaTime, out colliderList);
            if (colliderList.Count > 0)
            {

            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override bool CheckCollisionLeftFromVelocity()
    {
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override bool CheckCollisionRightFromVelocity()
    {
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override bool CheckCollisionDownFromVelocity()
    {
        return false;
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
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public override Vector2 GetLowerBoundsAtXValue(float x)
    {
        float adjustedX = x - bounds.center.x;
        float angle = Mathf.Acos(adjustedX / bounds.radius);
        return new Vector2(x, Mathf.Sin(angle) * bounds.radius);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public override Vector2 GetUpperBoundsAtXValue(float x)
    {
        float adjustedX = x - bounds.center.x;
        float angle = Mathf.Acos(adjustedX / bounds.radius);
        return new Vector2(x, -Mathf.Sin(angle) * bounds.radius);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public override Vector2 GetRighBoundAtYValue(float y)
    {
        float adjustedY = y - bounds.center.y;
        float angle = Mathf.Asin(adjustedY / bounds.radius);
        return new Vector2(Mathf.Cos(angle) * bounds.radius, y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public override Vector2 GetLeftBoundAtYValue(float y)
    {
        float adjustedY = y - bounds.center.y;
        float angle = Mathf.Asin(adjustedY / bounds.radius);
        return new Vector2(-Mathf.Cos(angle) * bounds.radius, y);
    }

    public override bool ColliderIntersect(CustomCollider2D colliderToCheck, out Vector2 intersectionPoint)
    {
        if (colliderToCheck is CustomBoxCollider2D)
        {
            return RectIntersectCircle(((CustomBoxCollider2D)colliderToCheck).bounds, this.bounds, out intersectionPoint);
        }
        else if (colliderToCheck is CustomCircleCollider2D)
        {
            return CircleIntersectCircle(this.bounds, ((CustomCircleCollider2D)colliderToCheck).bounds, out intersectionPoint);
        }
        else if (colliderToCheck is CustomCapsuleCollider2D)
        {
            return CapsuleIntersectCircle(((CustomCapsuleCollider2D)colliderToCheck).bounds, this.bounds, out intersectionPoint);
        }
        else
        {
            Debug.LogError("Circle Collider does not support type: " + colliderToCheck.GetType());
            intersectionPoint = Vector2.zero;
            return false;
        }
    }
}
