using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class HitboxCircle : Hitbox
{
    public float radius = 1;

    public CustomCollider2D.BoundsCircle bounds;

    #region monobehaviour methods
    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateColliderBounds();
        }
        Color colorToDraw = GetColorToDrawGizmos();
        Color colorToDrawTransparent = colorToDraw;
        colorToDrawTransparent.a = .2f;
#if UNITY_EDITOR
        
        UnityEditor.Handles.color = colorToDrawTransparent;
        UnityEditor.Handles.DrawSolidDisc(this.transform.position, Vector3.forward, radius);
        UnityEditor.Handles.color = colorToDraw;
        UnityEditor.Handles.DrawWireDisc(this.transform.position, Vector3.forward, radius);
#endif
    }
    #endregion monobehaviour methods

    public override bool CheckHitboxIntersect(Hitbox hboxToCheck)
    {
        if (hboxToCheck == null)
        {
            return false;
        }

        if (hboxToCheck is HitboxRect)
        {
            
            return CustomCollider2D.RectIntersectCircle(((HitboxRect)hboxToCheck).bounds, this.bounds);
        }
        else if (hboxToCheck is HitboxCircle)
        {
            return CustomCollider2D.CircleIntersectCircle(this.bounds, ((HitboxCircle)hboxToCheck).bounds);
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateColliderBounds()
    {
        bounds = new CustomCollider2D.BoundsCircle();
        bounds.center = this.transform.position;
        bounds.radius = this.radius;
    }
}
