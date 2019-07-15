using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// </summary>
public class ColliderManager : MonoBehaviour
{
    private List<CustomCollider2D> colliderList = new List<CustomCollider2D>();

    #region monobehaviour methods
    private void Awake()
    {
        Overseer.Instance.colliderManager = this;
    }

    private void Update()
    {
        foreach (CustomCollider2D collider in colliderList)
        {
            if (collider.enabled && !collider.isStatic)
            {
                collider.UpdateBoundsOfCollider();
            }
        }

        for (int i = 0; i < colliderList.Count; i++)
        {
            if (colliderList[i].enabled)
            {
                for (int j = i + 1; j < colliderList.Count - 1; j++)
                {
                    colliderList[i].IntersectWithCollider(colliderList[j]);
                }
            }
        }
    }
    #endregion monobehaviour methods

    #region collider interaction methods


    public void AddColliderToManager(CustomCollider2D collider)
    {
        if (colliderList.Contains(collider))
        {
            return;
        }
        colliderList.Add(collider);
    }

    public void RemoveColliderFromManager(CustomCollider2D collider)
    {
        if (!colliderList.Contains(collider))
        {
            return;
        }
        colliderList.Remove(collider);
    }
    #endregion collider interaction methods

    public bool CheckLineIntersectWithCollider(Vector2 origin, Vector2 direction, float distance)
    {
        List<CustomCollider2D> list = new List<CustomCollider2D>();
        return CheckLineIntersectWithCollider(origin, direction, distance, out list);
    }

    public bool CheckLineIntersectWithCollider(Vector2 origin, Vector2 direction, float distance, out List<CustomCollider2D> collidersHit)
    {
        collidersHit = new List<CustomCollider2D>();
        foreach (CustomCollider2D coll in colliderList)
        {
            if (coll.enabled)
            {
                if (coll.LineIntersectWithCollider(origin, direction, distance))
                {
                    collidersHit.Add(coll);
                }
            }
        }
        return collidersHit.Count >= 1;
    }
}
