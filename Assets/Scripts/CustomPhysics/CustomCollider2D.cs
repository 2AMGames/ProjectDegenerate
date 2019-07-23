using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Base class of our custom collider. This will check to see if there are any points where our collider intersects
/// with other colliders.
/// </summary>
public abstract class CustomCollider2D : MonoBehaviour {
	[Tooltip("Mark this value true if you would like to treat this value as a trigger")]
    public bool isTrigger;
    public float HorizontalBuffer = .02f;
    public float VerticalBuffer = .02f;
    public int verticalRayCount;
    public int horizontalRayCount;

    public CustomPhysics2D rigid { get; set; }
    public bool isStatic;

    /// <summary>
    /// 
    /// </summary>
    public ColliderBounds bounds { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected ColliderBounds previousBounds { get; set; }

    protected virtual void Awake()
    {
        UpdateBoundsOfCollider();
        rigid = GetComponent<CustomPhysics2D>();
        if (rigid == null)
        {
            isStatic = true;
        }
        Overseer.Instance.ColliderManager.AddColliderToManager(this);
    }


    protected virtual void OnDestroy()
    {
        if (Overseer.Instance && Overseer.Instance.ColliderManager)
        {
            Overseer.Instance.ColliderManager.RemoveColliderFromManager(this);
        }
    }

    protected virtual void OnValidate()
    {
        if (verticalRayCount < 2)
        {
            verticalRayCount = 2;
        }
        if (horizontalRayCount < 2)
        {
            horizontalRayCount = 2;
        }

    }

    /// <summary>
    /// Be sure to call this methodd
    /// </summary>
    public virtual void UpdateBoundsOfCollider()
    {
        previousBounds = bounds;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public abstract bool LineIntersectWithCollider(Vector2 origin, Vector2 direction, float length);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected abstract bool CheckCollisionDownFromVelocity();


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected abstract bool CheckCollisionUpFromVelocity();


    /// <summary>
    /// Updates to see if we collided with any object to the Right
    /// 
    /// NOTE: Move this to CustomBoxCollider2D
    /// </summary>
    /// <returns></returns>
    protected abstract bool CheckCollisionRightFromVelocity();
    

    /// <summary>
    /// Updates to check if we have collided with an object to the left
    /// 
    /// NOTE: Move this over to CustomBoxCollider2D
    /// </summary>
    /// <returns></returns>
    protected abstract bool CheckCollisionLeftFromVelocity();
    

    /// <summary>
    /// 
    /// </summary>
    public virtual void CheckForCollisions()
    {
        if (CheckCollisionUpFromVelocity())
        {
            if (rigid.isInAir)
            {
                rigid.isInAir = false;
                rigid.OnPhysicsObjectGrounded();
            }
        }
        else
        {
            if (!rigid.isInAir)
            {
                rigid.isInAir = true;
                rigid.OnPhysicsObjectAirborne();
            }
        }
        CheckCollisionDownFromVelocity();
        CheckCollisionLeftFromVelocity();
        CheckCollisionRightFromVelocity();
    }

    public abstract void PushObjectOutsideOfCollider(CustomCollider2D collider);

    public abstract CustomCollider2D[] GetAllTilesHitFromRayCasts(Vector2 v1, Vector2 v2, Vector2 direction, float distance, int rayCount);

    /// <summary>
    /// Call this method to check if we intersect with a colider
    /// </summary>
    /// <param name="colliderToCheck"></param>
    /// <returns></returns>
    public virtual bool IntersectWithCollider(CustomCollider2D colliderToCheck)
    {
        return false;
    }

    public struct ColliderBounds
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

    public struct BoundsCircle
    {
        public Vector2 center;
        public float radius;
    }


    #region static methods
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool RectIntersectRect(ColliderBounds r1, ColliderBounds r2, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.zero;

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
    /// <param name="intersectionPoint"></param>
    /// <returns></returns>
    public static bool RectIntersectCircle(ColliderBounds r, BoundsCircle c, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.zero;

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <param name="intersectionPoint"></param>
    /// <returns></returns>
    public static bool CircleIntersectCircle(BoundsCircle c1, BoundsCircle c2, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.zero;

        return false;
    }
    #endregion static methods
}
