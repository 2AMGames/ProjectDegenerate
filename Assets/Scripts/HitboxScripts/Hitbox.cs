using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Our custom collider hitbox class. Hitboxes do not interact with any other type of collider except for other hitboxes and hurtboxes
/// Do not use this to interact with the environment or activate the triggers
/// </summary>
public class Hitbox : MonoBehaviour
{
    #region const variables
    private static Color GIZMO_COLOR = new Color(204f / 255f, 0, 0);
    private static Color GIZMO_HURTBOX_COLOR = new Color(51f / 255f, 1, 1);
    #endregion const variables

    public enum HitboxType
    {
        Hitbox,
        Hurtbox,
    }

    public HitboxType hitboxType = HitboxType.Hurtbox;

    public Vector2 boxColliderSize = Vector2.one;
    public Vector2 boxColliderPosition;

    public HitboxBounds hitboxColliderBounds;


    #region monobehaviour methods
    private void Awake()
    {
        Overseer.Instance.hitboxManager.AddHitboxToList(this);
    }

    private void OnDestroy()
    {
        Overseer.Instance.hitboxManager.RemoveHitboxFromList(this);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void OnDrawGizmos()
    {
        UpdateBoxColliderPoints();
        Color colorToDraw = Color.white;
        switch (hitboxType)
        {
            case HitboxType.Hitbox:
                colorToDraw = GIZMO_COLOR;
                break;
            case HitboxType.Hurtbox:
                colorToDraw = GIZMO_HURTBOX_COLOR;
                break;
        }
        DebugSettings.DrawLine(hitboxColliderBounds.topRight, hitboxColliderBounds.topLeft, colorToDraw);
        DebugSettings.DrawLine(hitboxColliderBounds.topRight, hitboxColliderBounds.bottomRight, colorToDraw);
        DebugSettings.DrawLine(hitboxColliderBounds.topLeft, hitboxColliderBounds.bottomLeft, colorToDraw);
        DebugSettings.DrawLine(hitboxColliderBounds.bottomLeft, hitboxColliderBounds.bottomRight, colorToDraw);
    }
    #endregion monobehaviour methods

    /// <summary>
    /// This should be called by our HitboxManager
    /// </summary>
    public void UpdateBoxColliderPoints()
    {
        hitboxColliderBounds = new HitboxBounds();
        Vector2 origin = this.transform.position + new Vector3(boxColliderPosition.x, boxColliderPosition.y);

        hitboxColliderBounds.topLeft = origin + Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        hitboxColliderBounds.topRight = origin + Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;
        hitboxColliderBounds.bottomLeft = origin - Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        hitboxColliderBounds.bottomRight = origin - Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;

    }

    public struct HitboxBounds
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }
}
