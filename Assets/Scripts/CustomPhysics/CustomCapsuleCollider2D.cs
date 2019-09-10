﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCapsuleCollider2D : CustomCollider2D
{
    public float size = 1;
    public float radius = 1;

    private bool drawHorizontal;
    public Vector2 capsuleOffset;

    public BoundsCapsule bounds;
    public BoundsCapsule previousBounds;

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
        previousBounds = bounds;

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

    public override bool ColliderIntersect(CustomCollider2D colliderToCheck)
    {
        return false;

    }

    public override Vector2 GetCenter()
    {
        throw new System.NotImplementedException();
    }

    public override bool ColliderIntersectBasedOnVelocity(CustomCollider2D colliderToCheck)
    {
        throw new System.NotImplementedException();
    }
}
