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

    public InteractionHandler InteractionHandler;
    /// <summary>
    /// Hitboxes that we are currently intersecting
    /// </summary>
    public List<Hitbox> currentIntersectingHitboxes = new List<Hitbox>();

    #region monobehaviour methods
    private void Awake()
    {
        Overseer.Instance.hitboxManager.AddHitboxToList(this);
        InteractionHandler = GetComponentInParent<InteractionHandler>();

    }

    private void OnDestroy()
    {
        if (Overseer.Instance && Overseer.Instance.hitboxManager)
            Overseer.Instance.hitboxManager.RemoveHitboxFromList(this);
    }

    private void OnEnable()
    {
        Overseer.Instance.hitboxManager.SetHitboxEnabled(this, true);
    }

    private void OnDisable()
    {
        if (Overseer.Instance && Overseer.Instance.hitboxManager)
        {
            Overseer.Instance.hitboxManager.SetHitboxEnabled(this, false);
        }
        else
        {
            return;
        }
        foreach (Hitbox hbox in currentIntersectingHitboxes.ToArray())
        {
            if (hbox)
            {
                Overseer.Instance.hitboxManager.DetermineHitboxExitHitboxEvent(this, hbox);
            }
        }
        currentIntersectingHitboxes.Clear();
    }

    private void OnValidate()
    {
        UpdateBoxColliderPoints();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoxColliderPoints();
        }
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
        Color colorWithTransparency = colorToDraw;
        colorWithTransparency.a = .2f;
        #if UNITY_EDITOR
        UnityEditor.Handles.DrawSolidRectangleWithOutline(hitboxColliderBounds.GetVertices(), colorWithTransparency, colorToDraw);
        #endif
    }
    #endregion monobehaviour methods
    /// <summary>
    /// Returns true if we successfully added the hitbox to the list
    /// </summary>
    /// <param name="hitboxToAdd"></param>
    /// <returns></returns>
    public bool AddIntersectingHitbox(Hitbox hitboxToAdd)
    {
        if (currentIntersectingHitboxes.Contains(hitboxToAdd))
        {
            return false;
        }

        currentIntersectingHitboxes.Add(hitboxToAdd);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitboxToRemove"></param>
    /// <returns></returns>
    public bool RemoveIntersectingHitbox(Hitbox hitboxToRemove)
    {
        if (currentIntersectingHitboxes.Count == 0) return false;

        if (currentIntersectingHitboxes.Contains(hitboxToRemove))
        {
            currentIntersectingHitboxes.Remove(hitboxToRemove);
            return true;
        }
        return false;
    }

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
