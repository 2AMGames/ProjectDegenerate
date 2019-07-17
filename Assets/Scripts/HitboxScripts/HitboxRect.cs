using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Our custom collider hitbox class. Hitboxes do not interact with any other type of collider except for other hitboxes and hurtboxes
/// Do not use this to interact with the environment or activate the triggers
/// </summary>
public class HitboxRect : Hitbox
{
    public Vector2 boxColliderSize = Vector2.one;
    public Vector2 boxColliderPosition;

    public HitboxBounds hitboxColliderBounds;

   

    #region monobehaviour methods
    

    private void OnValidate()
    {
        UpdateBoxColliderPoints();
    }

    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoxColliderPoints();
        }
        Color colorToDraw = GetColorToDrawGizmos();
        
        Color colorWithTransparency = colorToDraw;
        colorWithTransparency.a = .2f;
        #if UNITY_EDITOR
        UnityEditor.Handles.DrawSolidRectangleWithOutline(hitboxColliderBounds.GetVertices(), colorWithTransparency, colorToDraw);
        #endif
    }
    #endregion monobehaviour methods

   


    /// <summary>
    /// This should be called by our HitboxManager
    /// </summary>
    public override void UpdateBoxColliderPoints()
    {
        hitboxColliderBounds = new HitboxBounds();
        Vector2 origin = this.transform.position + new Vector3(boxColliderPosition.x, boxColliderPosition.y);

        hitboxColliderBounds.topLeft = origin + Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        hitboxColliderBounds.topRight = origin + Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;
        hitboxColliderBounds.bottomLeft = origin - Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        hitboxColliderBounds.bottomRight = origin - Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;

    }

    public override bool CheckHitboxIntersect(Hitbox hboxToCheck)
    {
        if (hboxToCheck is HitboxRect)
        {
            HitboxRect hRect = (HitboxRect)(hboxToCheck);
            Vector2 tl1 = this.hitboxColliderBounds.topLeft;
            Vector2 br1 = this.hitboxColliderBounds.bottomRight;
            Vector2 tl2 = hRect.hitboxColliderBounds.topLeft;
            Vector2 br2 = hRect.hitboxColliderBounds.bottomRight;

            if (tl1.x > br2.x || tl2.x > br1.x)
            {
                return false;
            }
            if (tl1.y < br2.y || tl2.y < br1.y)
            {
                return false;
            }

            return true;
        }
        return false;
    }

    public struct HitboxBounds
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;

        public Vector3[] GetVertices()
        {
            return new Vector3[]
            {
                topLeft,
                topRight,
                bottomRight,
                bottomLeft,
            };
        }
    }
}
