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

    public CustomCollider2D.BoundsRect bounds;

    

    #region monobehaviour methods


    private void OnValidate()
    {
        if (boxColliderSize.x < 0)
        {
            boxColliderSize.x = 0;
        }
        if (boxColliderSize.y < 0)
        {
            boxColliderSize.y = 0;
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            
            UpdateColliderBoundsForGizmos(transform.root);
        }
        Color colorToDraw = GetColorToDrawGizmos();
        
        Color colorWithTransparency = colorToDraw;
        colorWithTransparency.a = .2f;
        #if UNITY_EDITOR
        UnityEditor.Handles.DrawSolidRectangleWithOutline(bounds.GetVertices(), colorWithTransparency, colorToDraw);
        #endif
    }
    #endregion monobehaviour methods

   


    /// <summary>
    /// This should be called by our HitboxManager
    /// </summary>
    public override void UpdateColliderBounds()
    {
        UpdateColliderBoundsForGizmos(this.transform.root);
    }

    /// <summary>
    /// This functions similarly to how our update bounds works but removes the need for an interaction handler but replacing it with their parent
    /// most object
    /// </summary>
    public void UpdateColliderBoundsForGizmos(Transform transformToUseAsScale)
    {
        bounds = new CustomCollider2D.BoundsRect();
        Vector2 origin = this.transform.position; //new Vector3(boxColliderOffset.x, boxColliderOffset.y);
        Vector2 charLocalScale;

        if (transformToUseAsScale && !ignoreParentScale)
        {
            charLocalScale = transformToUseAsScale.localScale; //We need this to properly scale the hitboxes to our parent object
            charLocalScale = new Vector2(Mathf.Abs(charLocalScale.x), Mathf.Abs(charLocalScale.y));
        }
        else
            charLocalScale = Vector2.one;
        
        bounds.topLeft = origin + (charLocalScale.y * Vector2.up * boxColliderSize.y / 2) - (charLocalScale.x * Vector2.right * boxColliderSize.x / 2);
        bounds.topRight = origin + (charLocalScale.y * Vector2.up * boxColliderSize.y / 2) + (charLocalScale.x * Vector2.right * boxColliderSize.x / 2);
        bounds.bottomLeft = origin - (charLocalScale.y * Vector2.up * boxColliderSize.y / 2) - (charLocalScale.x * Vector2.right * boxColliderSize.x / 2);
        bounds.bottomRight = origin - (charLocalScale.y * Vector2.up * boxColliderSize.y / 2) + (charLocalScale.x * Vector2.right * boxColliderSize.x / 2);
    }

    public override bool CheckHitboxIntersect(Hitbox hboxToCheck)
    {
        if (hboxToCheck is HitboxRect)
        {
            return CustomCollider2D.RectIntersectRect(this.bounds, ((HitboxRect)hboxToCheck).bounds);

            
        }
        if (hboxToCheck is HitboxCircle)
        {
            return CustomCollider2D.RectIntersectCircle(this.bounds, ((HitboxCircle)hboxToCheck).bounds);
        }
        return false;
    }

    
}
