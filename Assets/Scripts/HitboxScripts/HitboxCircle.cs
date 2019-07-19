using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class HitboxCircle : Hitbox
{
    public float radius = 1;

    

    #region monobehaviour methods
    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoxColliderPoints();
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
            CircleIntersectRect((HitboxRect)hboxToCheck);
        }
        else if (hboxToCheck is HitboxCircle)
        {
            return this.radius + ((HitboxCircle)hboxToCheck).radius < Vector2.Distance(this.transform.position, hboxToCheck.transform.position);
        }
        return false;
    }

    public bool CircleIntersectRect(HitboxRect hboxRect)
    {
        Vector2 point = transform.position;

        Vector2 A = hboxRect.hitboxColliderBounds.topLeft;
        Vector2 B = hboxRect.hitboxColliderBounds.topRight;
        Vector2 D = hboxRect.hitboxColliderBounds.bottomLeft;
        float APdotAB = Vector2.Dot(point - A, B - A);
        float ABdotAB = Vector2.Dot(B - A, B - A);
        float APdotAD = Vector2.Dot(point - A, D - A);
        float ADdotAD = Vector2.Dot(D - A, D - A);
        return 0 <= APdotAB && APdotAB <= ABdotAB && 0 <= APdotAD && APdotAD < ADdotAD;
    }

    public override void UpdateBoxColliderPoints()
    {
        return;
    }
}
