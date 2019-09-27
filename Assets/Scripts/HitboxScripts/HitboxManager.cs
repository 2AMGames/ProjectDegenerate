using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : MonoBehaviour
{
    [System.NonSerialized]
    /// <summary>
    /// 
    /// </summary>
    public List<Hitbox> allHitboxes = new List<Hitbox>();
    [System.NonSerialized]
    /// <summary>
    /// 
    /// </summary>
    public List<Hitbox> allActiveHitboxes = new List<Hitbox>();

    #region monobehavoiur methods
   
    /// <summary>
    /// Adds a new hitbox to the list of all available hitboxes
    /// </summary>
    /// <param name="hitboxToAdd"></param>
    public void AddHitboxToList(Hitbox hitboxToAdd)
    {
        allActiveHitboxes.Add(hitboxToAdd);
        allHitboxes.Add(hitboxToAdd);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitboxToRemove"></param>
    public void RemoveHitboxFromList(Hitbox hitboxToRemove)
    {
        allActiveHitboxes.Remove(hitboxToRemove);
        allHitboxes.Remove(hitboxToRemove);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitbox"></param>
    /// <param name="hitboxEnabled"></param>
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

    public void UpdateHitboxManager()
    {
        if (!Overseer.Instance.IsGameReady)
        {
            return;
        }

        Hitbox h1 = null;
        Hitbox h2 = null;
        foreach (Hitbox hBox in allActiveHitboxes)
        {
            hBox.UpdateColliderBounds();
        }
        for (int i = 0; i < allActiveHitboxes.Count - 1; i++)
        {
            h1 = allActiveHitboxes[i];
            for (int j = i + 1; j < allActiveHitboxes.Count; j++)
            {
                h2 = allActiveHitboxes[j];

                // Do not consider pairs of hit boxes that are both hurtboxes. These will never trigger an event
                // Do not consider hitboxes of the same player indices.
                if (!IsValidHitBoxPair(h1, h2))
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
        return h1.CheckHitboxIntersect(h2);
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
        if (h1.hitboxType == HitboxRect.HitboxType.Hitbox)
        {
            if (h2.hitboxType == HitboxRect.HitboxType.Hurtbox)
            { 
                OnHitboxStayHurtboxEvent(h1, h2);
                if (firstTimeIntersecting)
                {
                    OnHitboxEnteredHurtboxEvent(h1, h2);
                }
            }
            else if (h2.hitboxType == HitboxRect.HitboxType.Hitbox)
            {
                OnHitboxStayHitboxEvent(h1, h2);
                if (firstTimeIntersecting)
                {
                    OnHitboxEnterHitboxEvent(h1, h2);
                }
            }
        }
        else if (h2.hitboxType == HitboxRect.HitboxType.Hitbox)
        {
            if (h1.hitboxType == HitboxRect.HitboxType.Hurtbox)
            {
                OnHitboxStayHurtboxEvent(h2, h1);
                if (firstTimeIntersecting)
                {
                    OnHitboxEnteredHurtboxEvent(h2, h1);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="h1"></param>
    /// <param name="h2"></param>
    public void DetermineHitboxExitHitboxEvent(Hitbox h1, Hitbox h2)
    {
        bool leaveHitbox = h1.RemoveIntersectingHitbox(h2);
        leaveHitbox = h2.RemoveIntersectingHitbox(h1);
        if (leaveHitbox)
        {
            if (h1.hitboxType == HitboxRect.HitboxType.Hitbox)
            {
                if (h2.hitboxType == HitboxRect.HitboxType.Hurtbox)
                {
                    OnHitboxExitHurtboxEvent(h1, h2);
                }
                else if (h2.hitboxType == HitboxRect.HitboxType.Hitbox)
                {
                    OnHitboxExitHitboxEvent(h1, h2);
                }
            }
            else if (h2.hitboxType == HitboxRect.HitboxType.Hitbox)
            {
                if (h1.hitboxType == HitboxRect.HitboxType.Hurtbox)
                {
                    OnHitboxExitHurtboxEvent(h2, h1);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitbox"></param>
    /// <param name="hurtbox"></param>
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
            Debug.LogError("Player hit on frame: " + GameStateManager.Instance.FrameCount);
            bool didMoveHit = !WasMoveBlocked(hitHandler.CurrentMove, hitbox, hurtbox);
            hurtHandler.OnHitByEnemy(hurtbox, hitbox, hitHandler.CurrentMove, didMoveHit);
            hitHandler.OnHitEnemy(hitbox, hurtbox, didMoveHit);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitbox"></param>
    /// <param name="hurtbox"></param>
    private void OnHitboxStayHurtboxEvent(Hitbox hitbox, Hitbox hurtbox)
    {
        //print(hitbox.name + "  " + hurtbox.name + " stayed!");

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitbox"></param>
    /// <param name="hurtbox"></param>
    private void OnHitboxExitHurtboxEvent(Hitbox hitbox, Hitbox hurtbox)
    {
        //print(hitbox.name + "  " + hurtbox.name + " exited!");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitbox1"></param>
    /// <param name="hitbox2"></param>
    private void OnHitboxEnterHitboxEvent(Hitbox hitbox1, Hitbox hitbox2)
    {
        //print(hitbox1.name + "  " + hitbox2.name + " entered!");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitbox1"></param>
    /// <param name="hitbox2"></param>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitbox1"></param>
    /// <param name="hitbox2"></param>
    private void OnHitboxExitHitboxEvent(Hitbox hitbox1, Hitbox hitbox2)
    {
        //print(hitbox1.name + "  " + hitbox2.name + " exited!");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="h1"></param>
    /// <param name="h2"></param>
    /// <returns></returns>
    private bool IsValidHitBoxPair(Hitbox h1, Hitbox h2)
    {
        return !(h1.InteractionHandler == h2.InteractionHandler || (h1.hitboxType == HitboxRect.HitboxType.Hurtbox && h2.hitboxType == HitboxRect.HitboxType.Hurtbox));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="moveThatHit"></param>
    /// <param name="hitBox"></param>
    /// <param name="hurtBox"></param>
    /// <returns></returns>
    private bool WasMoveBlocked(InteractionHandler.MoveData moveThatHit, Hitbox hitBox, Hitbox hurtBox)
    {
        CommandInterpreter.DIRECTION hitPlayerInputDirection = hurtBox.InteractionHandler.CommandInterpreter.CurrentDirection;
        bool wasGuarded = true;
        wasGuarded &= hurtBox.InteractionHandler.Hitstun <= 0;
        if (!wasGuarded)
        {
            return wasGuarded;
        }

        bool isCharacterCrouching = hurtBox.InteractionHandler.MovementMechanics.IsCrouching;
        // Determine if blocked based on height
        switch (moveThatHit.Height)
        {
            case InteractionHandler.MoveData.HitHeight.Low:
                wasGuarded &= isCharacterCrouching;
                break;
            case InteractionHandler.MoveData.HitHeight.Mid:
                wasGuarded &= !isCharacterCrouching;
                break;
            case InteractionHandler.MoveData.HitHeight.High:
                wasGuarded &= !isCharacterCrouching;
                break;
            case InteractionHandler.MoveData.HitHeight.Air:
                wasGuarded &= !isCharacterCrouching;
                break;
        }

        if (!wasGuarded)
        {
            return wasGuarded;
        }

        return wasGuarded & (hitPlayerInputDirection == CommandInterpreter.DIRECTION.BACK || hitPlayerInputDirection == CommandInterpreter.DIRECTION.BACK_DOWN);

    }
    #endregion hitbox events 
}
