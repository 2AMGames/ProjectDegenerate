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

                // Do not consider pairs of hit boxes that are both hurtboxes. These will never trigger an event
                // Do not consider hitboxes of the same player indices.
                if (!IsValidHitBoxPair(h1,h2)) 
                {
                    continue;
                }
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
        //print(hitbox.name + " " + hurtbox.name + " entered!");
        InteractionHandler hitHandler = hitbox.InteractionHandler;
        InteractionHandler hurtHandler = hurtbox.InteractionHandler;

        // If the hitbox for this move allows multi hit, then we can register another hit on this frame.
        // If not, only register a hit if the move has not already hit the player.
        // TODO Replace HitBox.AllowMultiHit with (CharacterMove).MultiHit
        // Assuming we want to keep move properties in a 
        if (hurtHandler && !hitHandler.CharactersHit.Contains(hurtHandler))
        {
            hurtHandler.OnHitByEnemy(hurtbox, hitbox, hitHandler.CurrentMove);
        }

        if (hitHandler)
        {
            hitHandler.OnHitEnemy(hitbox, hurtbox);
        }
    }

    private void OnHitboxStayHurtboxEvent(Hitbox hitbox, Hitbox hurtbox)
    {
        //print(hitbox.name + "  " + hurtbox.name + " stayed!");

    }

    private void OnHitboxExitHurtboxEvent(Hitbox hitbox, Hitbox hurtbox)
    {
        //print(hitbox.name + "  " + hurtbox.name + " exited!");
    }

    private void OnHitboxEnterHitboxEvent(Hitbox hitbox1, Hitbox hitbox2)
    {
        //print(hitbox1.name + "  " + hitbox2.name + " entered!");
    }

    private void OnHitboxStayHitboxEvent(Hitbox hitbox1, Hitbox hitbox2)
    {
        print(hitbox1.name + "  " + hitbox2.name + " stayed!");
        InteractionHandler hitHandler1 = hitbox1.InteractionHandler;
        InteractionHandler hitHandler2 = hitbox2.InteractionHandler;

        if (hitHandler1)
        {
            hitHandler1.OnClash(hitbox2);
        }
        if (hitHandler2)
        {
            hitHandler2.OnClash(hitbox1);
        }
    }

    private void OnHitboxExitHitboxEvent(Hitbox hitbox1, Hitbox hitbox2)
    {
        //print(hitbox1.name + "  " + hitbox2.name + " exited!");
    }

    private bool IsValidHitBoxPair(Hitbox h1, Hitbox h2)
    {
        return !(h1.InteractionHandler == h2.InteractionHandler || (h1.hitboxType == Hitbox.HitboxType.Hurtbox && h2.hitboxType == Hitbox.HitboxType.Hurtbox));
    }
    #endregion hitbox events 
}
