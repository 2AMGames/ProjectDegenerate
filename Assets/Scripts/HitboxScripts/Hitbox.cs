using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all types of hitboxes.
/// </summary>
public abstract class Hitbox : MonoBehaviour
{
    #region const variables
    protected static Color GIZMO_COLOR = new Color(204f / 255f, 0, 0);
    protected static Color GIZMO_HURTBOX_COLOR = new Color(51f / 255f, 1, 1);
    #endregion const variables
    public enum HitboxType
    {
        Hitbox,
        Hurtbox,
    }

    /// <summary>
    /// 
    /// </summary>
    public HitboxType hitboxType = HitboxType.Hurtbox;

    /// <summary>
    /// All hitboxes should have a reference to an interaction handler. Required for sending events properly to the associated character
    /// </summary>
    public InteractionHandler InteractionHandler;


    /// <summary>
    /// Hitboxes that we are currently intersecting
    /// </summary>
    public List<Hitbox> currentIntersectingHitboxes = new List<Hitbox>();

    #region monobehaivour methods
    private void Awake()
    {
        Overseer.Instance.HitboxManager.AddHitboxToList(this);
        InteractionHandler = GetComponentInParent<InteractionHandler>();

    }

    private void OnDestroy()
    {
        if (Overseer.Instance && Overseer.Instance.HitboxManager)
            Overseer.Instance.HitboxManager.RemoveHitboxFromList(this);
    }

    private void OnEnable()
    {
        Overseer.Instance.HitboxManager.SetHitboxEnabled(this, true);
    }

    private void OnDisable()
    {
        if (Overseer.Instance && Overseer.Instance.HitboxManager)
        {
            Overseer.Instance.HitboxManager.SetHitboxEnabled(this, false);
        }
        else
        {
            return;
        }
        foreach (Hitbox hbox in currentIntersectingHitboxes.ToArray())
        {
            if (hbox)
            {
                Overseer.Instance.HitboxManager.DetermineHitboxExitHitboxEvent(this, hbox);
            }
        }
        currentIntersectingHitboxes.Clear();
    }
    #endregion
    #region debug helper methods
    /// <summary>
    /// DEBGUG: Gets the color that is associated with the type of hitbox that we are using.
    /// </summary>
    /// <returns></returns>
    protected Color GetColorToDrawGizmos()
    {
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
        return colorToDraw;
    }
    #endregion debug helper methods

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
    /// Removes an intersecting hitbox from the list. Returns true if the object
    /// was found in the list and successfully removed
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
    /// Method to updated the collider bounds of our hitbox
    /// </summary>
    public abstract void UpdateColliderBounds();


    /// <summary>
    /// Checks whether or not we intersect with the hitbox that is passed in.
    /// </summary>
    /// <param name="hboxToCheck"></param>
    /// <returns></returns>
    public abstract bool CheckHitboxIntersect(Hitbox hboxToCheck);
}
