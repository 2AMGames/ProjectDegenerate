using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : MonoBehaviour
{
    public List<Hitbox> allHitboxes = new List<Hitbox>();
    public List<Hitbox> allActiveHitboxes = new List<Hitbox>();
    #region monobehavoiur methods
    private void Awake()
    {
        Overseer.Instance.hitboxManager = this;
    }

    public void AddHitboxToList(Hitbox hitboxToAdd)
    {
        allActiveHitboxes.Add(hitboxToAdd);
        allHitboxes.Add(hitboxToAdd);
    }

    public void RemoveHitboxFromList(Hitbox hitboxToRemove)
    {
        allActiveHitboxes.Remove(hitboxToRemove);
        allHitboxes.Remove(hitboxToRemove);
    }

    /// <summary>
    /// This will be one of the last things that we check. We want to wait for everything to move before checking whether or not we have collided with anything
    /// </summary>
    private void LateUpdate()
    {
        Hitbox h1 = null;
        Hitbox h2 = null;
        for (int i = 0; i < allActiveHitboxes.Count - 1; i++)
        {
            for (int j = i + 1; j < allActiveHitboxes.Count; j++)
            {

                h1 = allActiveHitboxes[i];
                h2 = allActiveHitboxes[j];

                if (CheckHitboxIntersect(h1, h2))
                {
                    //print(h1.name + "  " + h2.name + " collided!");
                }
            }
        }
    }

    /// <summary>
    /// Returns whether or not the hitboxes that are passed through intersect
    /// </summary>
    /// <param name="h1"></param>
    /// <param name="h2"></param>
    /// <returns></returns>
    private bool CheckHitboxIntersect(Hitbox h1, Hitbox h2)
    {
        Vector2 tl1 = h1.hitboxColliderBounds.topLeft;
        Vector2 br1 = h1.hitboxColliderBounds.bottomRight;
        Vector2 tl2 = h2.hitboxColliderBounds.topLeft;
        Vector2 br2 = h2.hitboxColliderBounds.bottomRight;

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

    #endregion monobehaviour methods
}
