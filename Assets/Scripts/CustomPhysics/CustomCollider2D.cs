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
    protected ColliderBounds previouBounds { get; set; }

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
        previouBounds = bounds;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public abstract bool LineIntersectWithCollider(Vector2 origin, Vector2 direction, float length);


    private bool UpdateCollisionUp()
    {
        if (rigid.velocity.y <= 0)
        {
            return false;
        }
        Vector2 adjustedPoint1 = bounds.topLeft + Vector2.right * HorizontalBuffer - VerticalBuffer * Vector2.up;
        Vector2 adjustedPoint2 = bounds.topRight + Vector2.left * HorizontalBuffer - VerticalBuffer * Vector2.up;
        CustomCollider2D[] tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(adjustedPoint1, adjustedPoint2, Vector2.up,
            Mathf.Abs(rigid.velocity.y * Time.deltaTime) + VerticalBuffer, verticalRayCount);
        if (tileCollidersThatWeHit.Length == 0)
        {
            return false;
        }
        float lowestYValue = tileCollidersThatWeHit[0].bounds.bottomRight.y;
        foreach (CustomCollider2D tile in tileCollidersThatWeHit)
        {
            Vector2 pointThatWeCollidedWith = new Vector2(this.transform.position.x, tile.bounds.bottomRight.y);
            if (pointThatWeCollidedWith.y < lowestYValue)
            {
                lowestYValue = pointThatWeCollidedWith.y;
            }
        }
        rigid.velocity.y = 0;
        transform.position = new Vector3(transform.position.x, lowestYValue + (transform.position.y - bounds.topLeft.y), transform.position.z);
        UpdateBoundsOfCollider();
        return true;
    }

    private bool UpdateCollisionDown()
    {
        if (rigid.velocity.y > 0)
        {
            return false;
        }

        Vector2 adjustedPoint1 = bounds.bottomLeft + Vector2.right * HorizontalBuffer + VerticalBuffer * Vector2.up;
        Vector2 adjustedPoint2 = bounds.bottomRight + Vector2.left * HorizontalBuffer + VerticalBuffer * Vector2.up;
        CustomCollider2D[] tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(adjustedPoint1, adjustedPoint2, Vector2.down,
            Mathf.Abs(rigid.velocity.y * Time.deltaTime) + VerticalBuffer, verticalRayCount);
        if (tileCollidersThatWeHit.Length == 0)
        {
            return false;
        }
        float highestYValue = tileCollidersThatWeHit[0].bounds.topLeft.y;
        foreach (CustomCollider2D tile in tileCollidersThatWeHit)
        {
            Vector2 pointThatWeCollidedWith = new Vector2(this.transform.position.x, tile.bounds.topLeft.y);
            if (pointThatWeCollidedWith.y > highestYValue)
            {
                highestYValue = pointThatWeCollidedWith.y;
            }
        }
        rigid.velocity.y = 0;
        transform.position = new Vector3(transform.position.x, highestYValue + (transform.position.y - bounds.bottomLeft.y), transform.position.z);
        UpdateBoundsOfCollider();//If we made it to the end, we wil need to update the collider bounds
        return true;
    }

    private bool UpdateCollisionRight()
    {
        if (rigid.velocity.x <= 0)
        {
            return false;
        }
        Vector2 adjustedPoint1 = bounds.topRight + Vector2.right * HorizontalBuffer + VerticalBuffer * Vector2.up;
        Vector2 adjustedPoint2 = bounds.bottomRight + Vector2.right * HorizontalBuffer + VerticalBuffer * Vector2.up;
        CustomCollider2D[] tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(
            adjustedPoint1, adjustedPoint2, Vector2.right,
            Mathf.Abs(rigid.velocity.x * Time.deltaTime) + HorizontalBuffer, horizontalRayCount);
        if (tileCollidersThatWeHit.Length == 0)
        {
            return false;
        }
        //print(tileCollidersThatWeHit[0].name);
        float lowestXValue = tileCollidersThatWeHit[0].bounds.bottomLeft.x;
        foreach (CustomCollider2D tile in tileCollidersThatWeHit)
        {
            Vector2 pointThatWeCollidedWith = new Vector2(tile.bounds.bottomLeft.x, this.transform.position.y);
            if (pointThatWeCollidedWith.x < lowestXValue)
            {
                lowestXValue = pointThatWeCollidedWith.x;
            }
        }
        rigid.velocity.x = 0;
        transform.position = new Vector3(lowestXValue + (transform.position.x - bounds.topRight.x), transform.position.y, transform.position.z);
        UpdateBoundsOfCollider();

        return true;
    }

    private bool UpdateCollisionLeft()
    {
        if (rigid.velocity.x >= 0)
        {
            return false;
        }
        Vector2 adjustedPoint1 = bounds.topLeft + Vector2.right * HorizontalBuffer + VerticalBuffer * Vector2.up;
        Vector2 adjustedPoint2 = bounds.bottomLeft + Vector2.right * HorizontalBuffer + VerticalBuffer * Vector2.up;
        CustomCollider2D[] tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(
            adjustedPoint1, adjustedPoint2,
            Vector2.left, Mathf.Abs(rigid.velocity.x * Time.deltaTime) + HorizontalBuffer, horizontalRayCount);
        if (tileCollidersThatWeHit.Length == 0)
        {
            return false;
        }
        //print(tileCollidersThatWeHit[0].name);
        float highestXValue = tileCollidersThatWeHit[0].bounds.bottomRight.x;
        foreach (CustomCollider2D tile in tileCollidersThatWeHit)
        {
            Vector2 pointThatWeCollidedWith = new Vector2(tile.bounds.bottomRight.x, this.transform.position.y);
            if (pointThatWeCollidedWith.x > highestXValue)
            {
                highestXValue = pointThatWeCollidedWith.x;
            }
        }
        rigid.velocity.x = 0;
        transform.position = new Vector3(highestXValue + (transform.position.x - bounds.topLeft.x), transform.position.y, transform.position.z);
        UpdateBoundsOfCollider();
        return true;

    }

    public virtual void CheckForCollisions()
    {
        UpdateCollisionDown();
        UpdateCollisionUp();
        UpdateCollisionLeft();
        UpdateCollisionRight();
    }

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
}
