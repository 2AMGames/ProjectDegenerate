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
}
