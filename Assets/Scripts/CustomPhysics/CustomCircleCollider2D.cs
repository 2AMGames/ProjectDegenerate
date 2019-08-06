using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCircleCollider2D : CustomCollider2D
{
    public float radius;
    public BoundsCircle bounds;
    
    #region monobehaviour methods
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

#if UNITY_EDITOR

        UnityEditor.Handles.DrawWireDisc(bounds.center, Vector3.forward, bounds.radius);

#endif
    }
    #endregion monobehaviour methods

    public override bool LineIntersectWithCollider(Vector2 origin, Vector2 direction, float length)
    {
        throw new System.NotImplementedException();
    }

    public override void PushObjectOutsideOfCollider(CustomCollider2D collider)
    {
        throw new System.NotImplementedException();
    }

    protected override bool CheckCollisionDownFromVelocity()
    {
        throw new System.NotImplementedException();
    }

    protected override bool CheckCollisionLeftFromVelocity()
    {
        throw new System.NotImplementedException();
    }

    protected override bool CheckCollisionRightFromVelocity()
    {
        throw new System.NotImplementedException();
    }

    protected override bool CheckCollisionUpFromVelocity()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateBoundsOfCollider()
    {
        BoundsCircle cBounds = new BoundsCircle();
        cBounds.center = this.transform.position;
        cBounds.radius = radius;
    }

    public override Vector2 GetLowerBoundsAtXValue(float x)
    {
        throw new System.NotImplementedException();
    }

    public override Vector2 GetUpperBoundsAtXValue(float x)
    {
        throw new System.NotImplementedException();
    }

    public override Vector2 GetRighBoundAtYValue(float y)
    {
        throw new System.NotImplementedException();
    }

    public override Vector2 GetLeftBoundAtYValue(float y)
    {
        throw new System.NotImplementedException();
    }
}
