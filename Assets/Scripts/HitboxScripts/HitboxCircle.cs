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

        }
        else if (hboxToCheck is HitboxCircle)
        {
            return this.radius + ((HitboxCircle)hboxToCheck).radius < Vector2.Distance(this.transform.position, hboxToCheck.transform.position);
        }
        return false;
    }

    public override void UpdateBoxColliderPoints()
    {
        throw new System.NotImplementedException();
    }
}
