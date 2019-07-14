using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Base class of our custom collider. This will check to see if there are any points where our collider intersects
/// with other colliders.
/// </summary>
[RequireComponent(typeof(CustomPhysics2D))]
public class CustomCollider2D : MonoBehaviour {
	[Tooltip("Mark this value true if you would like to treat this value as a trigger")]
    public bool isTrigger;
    private const float HorizontalBuffer = .02f;
    private const float VerticalBuffer = .02f;


    public bool isStatic;
    /// <summary>
    /// 
    /// </summary>
    public ColliderBounds bounds { get; set; }
    /// <summary>
    /// 
    /// </summary>
    protected ColliderBounds previouBounds { get; set; }

    protected virtual void Awake()
    {
        UpdateBoundsOfCollider();
        Overseer.Instance.colliderManager.AddColliderToManager(this);
    }

    protected virtual void OnDestroy()
    {
        Overseer.Instance.colliderManager.RemoveColliderFromManager(this);
    }

    /// <summary>
    /// Be sure to call this methodd
    /// </summary>
    public virtual void UpdateBoundsOfCollider()
    {
        previouBounds = bounds;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public virtual bool LineIntersectWithCollider(Vector2 origin, Vector2 direction, float length)
    {
        return false;
    }


    //private bool UpdateCollisionUp()
    //{
    //    if (rigid.velocity.y <= 0)
    //    {
    //        return false;
    //    }
    //    Vector2 adjustedPoint1 = currentColliderBounds.topLeft + Vector2.right * HorizontalBuffer - VerticalBuffer * Vector2.up;
    //    Vector2 adjustedPoint2 = currentColliderBounds.topRight + Vector2.left * HorizontalBuffer - VerticalBuffer * Vector2.up;
    //    List<TileCollider> tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(adjustedPoint1, adjustedPoint2, Vector2.up, 
    //        Mathf.Abs(rigid.velocity.y * Time.deltaTime) + VerticalBuffer, verticalRayCount);
    //    if (tileCollidersThatWeHit.Count == 0)
    //    {
    //        return false;
    //    }
    //    float lowestYValue = tileCollidersThatWeHit[0].GetPointToBottom(transform.position).y;
    //    foreach (TileCollider tile in tileCollidersThatWeHit)
    //    {
    //        Vector2 pointThatWeCollidedWith = tile.GetPointToBottom(transform.position);
    //        if (pointThatWeCollidedWith.y < lowestYValue)
    //        {
    //            lowestYValue = pointThatWeCollidedWith.y;
    //        }
    //    }

    //    transform.position = new Vector3(transform.position.x, lowestYValue + (transform.position.y - currentColliderBounds.topLeft.y), transform.position.z);
    //    UpdateColliderBounds();
    //    return true;
    //}

    //private bool UpdateCollisionDown()
    //{
    //    if (rigid.velocity.y > 0)
    //    {
    //        return false;
    //    }

    //    Vector2 adjustedPoint1 = currentColliderBounds.bottomLeft + Vector2.right * HorizontalBuffer + VerticalBuffer * Vector2.up;
    //    Vector2 adjustedPoint2 = currentColliderBounds.bottomRight + Vector2.left * HorizontalBuffer + VerticalBuffer * Vector2.up;
    //    List<TileCollider> tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(adjustedPoint1, adjustedPoint2, Vector2.down, 
    //        Mathf.Abs(rigid.velocity.y * Time.deltaTime) + VerticalBuffer, verticalRayCount);
    //    if (tileCollidersThatWeHit.Count == 0)
    //    {
    //        return false;
    //    }
    //    float highestYValue = tileCollidersThatWeHit[0].GetPointToTop(transform.position).y;
    //    foreach (TileCollider tile in tileCollidersThatWeHit)
    //    {
    //        Vector2 pointThatWeCollidedWith = tile.GetPointToTop(transform.position);
    //        if (pointThatWeCollidedWith.y > highestYValue)
    //        {
    //            highestYValue = pointThatWeCollidedWith.y;
    //        }
    //    }

    //    transform.position = new Vector3(transform.position.x, highestYValue + (transform.position.y - currentColliderBounds.bottomLeft.y), transform.position.z);
    //    UpdateColliderBounds();//If we made it to the end, we wil need to update the collider bounds
    //    return true;
    //}

    //private bool UpdateCollisionRight()
    //{
    //    if (rigid.velocity.x <= 0)
    //    {
    //        return false;
    //    }

    //    List<TileCollider> tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(
    //        currentColliderBounds.topRight + Vector2.down * VerticalBuffer, currentColliderBounds.bottomRight + Vector2.up * VerticalBuffer, Vector2.right, 
    //        Mathf.Abs(rigid.velocity.x * Time.deltaTime) + HorizontalBuffer, horizontalRayCount);
    //    if (tileCollidersThatWeHit.Count == 0)
    //    {
    //        return false;
    //    }
    //    print(tileCollidersThatWeHit[0].name);
    //    float lowestXValue = tileCollidersThatWeHit[0].GetPointToLeft(transform.position).x;
    //    foreach (TileCollider tile in tileCollidersThatWeHit)
    //    {
    //        Vector2 pointThatWeCollidedWith = tile.GetPointToLeft(transform.position);
    //        if (pointThatWeCollidedWith.x < lowestXValue)
    //        {
    //            lowestXValue = pointThatWeCollidedWith.x;
    //        }
    //    }

    //    transform.position = new Vector3(lowestXValue + (transform.position.x - currentColliderBounds.topRight.x), transform.position.y, transform.position.z);
    //    print(currentColliderBounds);
    //    UpdateColliderBounds();
    //    print(currentColliderBounds);

    //    return true;
    //}

    //private bool UpdateCollisionLeft()
    //{
    //    if (rigid.velocity.x >= 0)
    //    {
    //        return false;
    //    }

    //    List<TileCollider> tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(
    //        currentColliderBounds.topLeft + Vector2.down * VerticalBuffer, currentColliderBounds.bottomLeft + Vector2.up * VerticalBuffer, 
    //        Vector2.left, Mathf.Abs(rigid.velocity.x * Time.deltaTime) + HorizontalBuffer, horizontalRayCount);
    //    if (tileCollidersThatWeHit.Count == 0)
    //    {
    //        return false;
    //    }
    //    print(tileCollidersThatWeHit[0].name);
    //    float highestXValue = tileCollidersThatWeHit[0].GetPointToRight(transform.position).x;
    //    foreach (TileCollider tile in tileCollidersThatWeHit)
    //    {
    //        Vector2 pointThatWeCollidedWith = tile.GetPointToRight(transform.position);
    //        if (pointThatWeCollidedWith.x > highestXValue)
    //        {
    //            highestXValue = pointThatWeCollidedWith.x;
    //        }
    //    }

    //    transform.position = new Vector3(highestXValue + (transform.position.x - currentColliderBounds.topLeft.x), transform.position.y, transform.position.z);
    //    print(currentColliderBounds);
    //    UpdateColliderBounds();
    //    print(currentColliderBounds);
    //    return true;

    //}

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
}
