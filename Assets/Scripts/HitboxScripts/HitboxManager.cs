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

    /// <summary>
    /// Adds a new hitbox to the list of all available hitboxes
    /// </summary>
    /// <param name="hitboxToAdd"></param>
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

    public void SetHitboxEnabled(Hitbox hitbox, bool hitboxEnabled)
    {
        if (hitboxEnabled)
        {
            
            if (!allActiveHitboxes.Contains(hitbox))
            {
                allActiveHitboxes.Add(hitbox);
            }
        }
        else
        {
            if (allActiveHitboxes.Contains(hitbox))
            {
                allActiveHitboxes.Remove(hitbox);
            }
        }
    }



    /// <summary>
    /// This will be one of the last things that we check. We want to wait for everything to move before checking whether or not we have collided with anything
    /// </summary>
    private void LateUpdate()
    {
        Hitbox h1 = null;
        Hitbox h2 = null;
        foreach (Hitbox hBox in allActiveHitboxes)
        {
            hBox.UpdateBoxColliderPoints();
        }
        for (int i = 0; i < allActiveHitboxes.Count - 1; i++)
        {
            h1 = allActiveHitboxes[i];
            for (int j = i + 1; j < allActiveHitboxes.Count; j++)
            {
                h2 = allActiveHitboxes[j];
                if (CheckHitboxIntersect(h1, h2))
                {
                    DetermineHitboxEnterHitboxEvent(h1, h2);
                }
                else
                {
                    DetermineHitboxExitHitboxEvent(h1, h2);
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
        if (h1.PlayerIndex == h2.PlayerIndex)//If they are both from the same character, we will ignore
        {
            return false;
        }
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

    #region hitbox events
    /// <summary>
    /// 
    /// </summary>
    /// <param name="h1"></param>
    /// <param name="h2"></param>
    /// <param name="isIntersecting"></param>
    public void DetermineHitboxEnterHitboxEvent(Hitbox h1, Hitbox h2)
    {
        bool firstTimeIntersecting = h1.AddIntersectingHitbox(h2);
        firstTimeIntersecting &= h2.AddIntersectingHitbox(h1);
        if (h1.hitboxType == Hitbox.HitboxType.Hitbox)
        {
            if (h2.hitboxType == Hitbox.HitboxType.Hurtbox)
            {
                OnHitboxStayHurtboxEvent(h1, h2);
                if (firstTimeIntersecting)
                {
                    OnHitboxEnteredHurtboxEvent(h1, h2);
                }
            }
            else if (h2.hitboxType == Hitbox.HitboxType.Hitbox)
            {
                OnHitboxStayHitboxEvent(h1, h2);
                if (firstTimeIntersecting)
                {
                    OnHitboxEnterHitboxEvent(h1, h2);
                }
            }
        }
        else if (h2.hitboxType == Hitbox.HitboxType.Hitbox)
        {
            if (h1.hitboxType == Hitbox.HitboxType.Hurtbox)
            {
                OnHitboxStayHurtboxEvent(h2, h1);
                if (firstTimeIntersecting)
                {
                    OnHitboxEnteredHurtboxEvent(h2, h1);
                }
            }
        }
    }

    public void DetermineHitboxExitHitboxEvent(Hitbox h1, Hitbox h2)
    {
        bool leaveHitbox = h1.RemoveIntersectingHitbox(h2);
        leaveHitbox = h2.RemoveIntersectingHitbox(h1);
        if (leaveHitbox)
        {
            if (h1.hitboxType == Hitbox.HitboxType.Hitbox)
            {
                if (h2.hitboxType == Hitbox.HitboxType.Hurtbox)
                {
                    OnHitboxExitHurtboxEvent(h1, h2);
                }
                else if (h2.hitboxType == Hitbox.HitboxType.Hitbox)
                {
                    OnHitboxExitHitboxEvent(h1, h2);
                }
            }
            else if (h2.hitboxType == Hitbox.HitboxType.Hitbox)
            {
                if (h1.hitboxType == Hitbox.HitboxType.Hurtbox)
                {
                    OnHitboxExitHurtboxEvent(h2, h1);
                }
            }
        }
    }

    private void OnHitboxEnteredHurtboxEvent(Hitbox hitbox, Hitbox hurtbox)
    {
        print(hitbox.name + " " + hurtbox.name + " entered!");
    }

    private void OnHitboxStayHurtboxEvent(Hitbox hitbox, Hitbox hurtbox)
    {
        print(hitbox.name + "  " + hurtbox.name + " stayed!");
        PlayerController hitController = Overseer.Instance.GetCharacterByIndex(hitbox.PlayerIndex);
        PlayerController hurtController = Overseer.Instance.GetCharacterByIndex(hurtbox.PlayerIndex);

        if (hitController && hitController.InteractionHandler)
        {
            hitController.InteractionHandler.OnHitEnemy(hitbox, hurtbox);
        }
        if (hurtController && hurtController.InteractionHandler)
        {
            hurtController.InteractionHandler.OnHitByEnemy(hurtbox, hitbox, hitController && hitController.InteractionHandler ? hitController.InteractionHandler.CurrentMove : (default));
        }
    }

    private void OnHitboxExitHurtboxEvent(Hitbox hitbox, Hitbox hurtbox)
    {
        print(hitbox.name + "  " + hurtbox.name + " exited!");
    }

    private void OnHitboxEnterHitboxEvent(Hitbox hitbox1, Hitbox hitbox2)
    {
        print(hitbox1.name + "  " + hitbox2.name + " entered!");
    }

    private void OnHitboxStayHitboxEvent(Hitbox hitbox1, Hitbox hitbox2)
    {
        print(hitbox1.name + "  " + hitbox2.name + " stayed!");
        PlayerController hitController1 = Overseer.Instance.GetCharacterByIndex(hitbox1.PlayerIndex);
        PlayerController hitController2 = Overseer.Instance.GetCharacterByIndex(hitbox2.PlayerIndex);
        if (hitController1 && hitController1.InteractionHandler)
        {
            hitController1.InteractionHandler.OnClash(hitbox2);
        }
        if (hitController2 && hitController2.InteractionHandler)
        {
            hitController2.InteractionHandler.OnClash(hitbox1);
        }
    }

    private void OnHitboxExitHitboxEvent(Hitbox hitbox1, Hitbox hitbox2)
    {
        print(hitbox1.name + "  " + hitbox2.name + " exited!");
    }
    #endregion hitbox events 
}
