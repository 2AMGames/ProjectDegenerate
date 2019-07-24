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

    public CustomCollider2D.ColliderBounds hitboxColliderBounds;

   

    #region monobehaviour methods
    

    private void OnValidate()
    {
        UpdateColliderBounds();
    }

    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateColliderBounds();
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
    public override void UpdateColliderBounds()
    {
        hitboxColliderBounds = new CustomCollider2D.ColliderBounds();
        Vector2 origin = this.transform.position + new Vector3(boxColliderPosition.x, boxColliderPosition.y);

        hitboxColliderBounds.topLeft = origin + Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        hitboxColliderBounds.topRight = origin + Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;
        hitboxColliderBounds.bottomLeft = origin - Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        hitboxColliderBounds.bottomRight = origin - Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;

    }

    public override bool CheckHitboxIntersect(Hitbox hboxToCheck)
    {
        Vector2 intersectionVec;
        if (hboxToCheck is HitboxRect)
        {
            return CustomCollider2D.RectIntersectRect(this.hitboxColliderBounds, ((HitboxRect)hboxToCheck).hitboxColliderBounds, out intersectionVec);

            
        }
        if (hboxToCheck is HitboxCircle)
        {
            return CustomCollider2D.RectIntersectCircle(this.hitboxColliderBounds, ((HitboxCircle)hboxToCheck).bounds, out intersectionVec);
        }
        return false;
    }

    
}
