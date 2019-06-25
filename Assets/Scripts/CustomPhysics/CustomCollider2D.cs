using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is specifically used do detect collisions of kinematic objects in their environemnt. It will only 
/// ensure that an object does not pass through a InteractableTIle object
/// </summary>
[RequireComponent(typeof(CustomPhysics2D))]
public class CustomCollider2D : MonoBehaviour {
    public float horizontalBuffer = .02f;
    public float verticalBuffer = .02f;

    [Header("Ray Counts")]
    [Tooltip("The number of rays we will fire in the horizontal direction")]
    public int horizontalRayCount = 4;
    [Tooltip("The number of rays we will fire in the vertical direction")]
    public int verticalRayCount = 4;

    [Header("Collider Values")]
    public Vector2 centerPoint;
    public Vector2 colliderSize;



    private CustomPhysics2D rigid;
    private ColliderBounds currentColliderBounds;

    


    #region monobehaviour methods
    private void Awake()
    {
        rigid = GetComponent<CustomPhysics2D>();
    }

    private void Start()
    {
        if (rigid)
        {
            rigid.allCustomColliders.Add(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        UpdateColliderBounds();
        DebugSettings.DrawLine(currentColliderBounds.bottomLeft, currentColliderBounds.bottomRight, Color.green);
        DebugSettings.DrawLine(currentColliderBounds.bottomLeft, currentColliderBounds.topLeft, Color.green);
        DebugSettings.DrawLine(currentColliderBounds.topLeft, currentColliderBounds.topRight, Color.green);
        DebugSettings.DrawLine(currentColliderBounds.bottomRight, currentColliderBounds.topRight, Color.green);

    }

    private void OnValidate()
    {
        if (horizontalRayCount < 2)
        {
            horizontalRayCount = 2;
        }
        if (verticalRayCount < 2)
        {
            verticalRayCount = 2;
        }
    }

    private void OnDestroy()
    {
        if (rigid)
        {
            rigid.allCustomColliders.Remove(this);
        }
    }
    #endregion monobehaviour methods

    #region collision checks


    public void UpdateCollisionPhysics()
    {
        UpdateColliderBounds();

        if (UpdateCollisionDown())
        {
            rigid.velocity = new Vector2(rigid.velocity.x, 0);
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
        if (UpdateCollisionUp())
        {
            rigid.velocity = new Vector2(rigid.velocity.x, 0);

        }
        if (UpdateCollisionRight())
        {
            rigid.velocity = new Vector2(0, rigid.velocity.y);
        }
        if (UpdateCollisionLeft())
        {
            rigid.velocity = new Vector2(0, rigid.velocity.y);
        }
        //CheckCollisionUp();
    }

    /// <summary>
    /// Checks if an our collider interacts with an environmental collider while it is moving up. We will
    /// push the object to the lowest point that we have hit
    /// </summary>
    /// <returns></returns>
    private bool UpdateCollisionUp()
    {
        if (rigid.velocity.y <= 0)
        {
            return false;
        }
        Vector2 adjustedPoint1 = currentColliderBounds.topLeft + Vector2.right * horizontalBuffer - verticalBuffer * Vector2.up;
        Vector2 adjustedPoint2 = currentColliderBounds.topRight + Vector2.left * horizontalBuffer - verticalBuffer * Vector2.up;
        List<TileCollider> tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(adjustedPoint1, adjustedPoint2, Vector2.up, 
            Mathf.Abs(rigid.velocity.y * Time.deltaTime) + verticalBuffer, verticalRayCount);
        if (tileCollidersThatWeHit.Count == 0)
        {
            return false;
        }
        float lowestYValue = tileCollidersThatWeHit[0].GetPointToBottom(transform.position).y;
        foreach (TileCollider tile in tileCollidersThatWeHit)
        {
            Vector2 pointThatWeCollidedWith = tile.GetPointToBottom(transform.position);
            if (pointThatWeCollidedWith.y < lowestYValue)
            {
                lowestYValue = pointThatWeCollidedWith.y;
            }
        }

        transform.position = new Vector3(transform.position.x, lowestYValue + (transform.position.y - currentColliderBounds.topLeft.y), transform.position.z);
        UpdateColliderBounds();
        return true;
    }

    private bool UpdateCollisionDown()
    {
        if (rigid.velocity.y > 0)
        {
            return false;
        }

        Vector2 adjustedPoint1 = currentColliderBounds.bottomLeft + Vector2.right * horizontalBuffer + verticalBuffer * Vector2.up;
        Vector2 adjustedPoint2 = currentColliderBounds.bottomRight + Vector2.left * horizontalBuffer + verticalBuffer * Vector2.up;
        List<TileCollider> tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(adjustedPoint1, adjustedPoint2, Vector2.down, 
            Mathf.Abs(rigid.velocity.y * Time.deltaTime) + verticalBuffer, verticalRayCount);
        if (tileCollidersThatWeHit.Count == 0)
        {
            return false;
        }
        float highestYValue = tileCollidersThatWeHit[0].GetPointToTop(transform.position).y;
        foreach (TileCollider tile in tileCollidersThatWeHit)
        {
            Vector2 pointThatWeCollidedWith = tile.GetPointToTop(transform.position);
            if (pointThatWeCollidedWith.y > highestYValue)
            {
                highestYValue = pointThatWeCollidedWith.y;
            }
        }

        transform.position = new Vector3(transform.position.x, highestYValue + (transform.position.y - currentColliderBounds.bottomLeft.y), transform.position.z);
        UpdateColliderBounds();//If we made it to the end, we wil need to update the collider bounds
        return true;
    }

    private bool UpdateCollisionRight()
    {
        if (rigid.velocity.x <= 0)
        {
            return false;
        }

        List<TileCollider> tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(
            currentColliderBounds.topRight + Vector2.down * verticalBuffer, currentColliderBounds.bottomRight + Vector2.up * verticalBuffer, Vector2.right, 
            Mathf.Abs(rigid.velocity.x * Time.deltaTime) + horizontalBuffer, horizontalRayCount);
        if (tileCollidersThatWeHit.Count == 0)
        {
            return false;
        }
        print(tileCollidersThatWeHit[0].name);
        float lowestXValue = tileCollidersThatWeHit[0].GetPointToLeft(transform.position).x;
        foreach (TileCollider tile in tileCollidersThatWeHit)
        {
            Vector2 pointThatWeCollidedWith = tile.GetPointToLeft(transform.position);
            if (pointThatWeCollidedWith.x < lowestXValue)
            {
                lowestXValue = pointThatWeCollidedWith.x;
            }
        }

        transform.position = new Vector3(lowestXValue + (transform.position.x - currentColliderBounds.topRight.x), transform.position.y, transform.position.z);
        print(currentColliderBounds);
        UpdateColliderBounds();
        print(currentColliderBounds);

        return true;
    }

    private bool UpdateCollisionLeft()
    {
        if (rigid.velocity.x >= 0)
        {
            return false;
        }

        List<TileCollider> tileCollidersThatWeHit = GetAllTilesHitFromRayCasts(
            currentColliderBounds.topLeft + Vector2.down * verticalBuffer, currentColliderBounds.bottomLeft + Vector2.up * verticalBuffer, 
            Vector2.left, Mathf.Abs(rigid.velocity.x * Time.deltaTime) + horizontalBuffer, horizontalRayCount);
        if (tileCollidersThatWeHit.Count == 0)
        {
            return false;
        }
        print(tileCollidersThatWeHit[0].name);
        float highestXValue = tileCollidersThatWeHit[0].GetPointToRight(transform.position).x;
        foreach (TileCollider tile in tileCollidersThatWeHit)
        {
            Vector2 pointThatWeCollidedWith = tile.GetPointToRight(transform.position);
            if (pointThatWeCollidedWith.x > highestXValue)
            {
                highestXValue = pointThatWeCollidedWith.x;
            }
        }

        transform.position = new Vector3(highestXValue + (transform.position.x - currentColliderBounds.topLeft.x), transform.position.y, transform.position.z);
        print(currentColliderBounds);
        UpdateColliderBounds();
        print(currentColliderBounds);
        return true;

    }

    /// <summary>
    /// Gets a list of all environmental colliders hit given the following parameters.
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <param name="directionToCastRay"></param>
    /// <param name="distanceToCastRay"></param>
    /// <param name="totalPointsToCheck"></param>
    /// <returns></returns>
    private List<TileCollider> GetAllTilesHitFromRayCasts(Vector2 point1, Vector2 point2, Vector2 directionToCastRay, float distanceToCastRay, int totalPointsToCheck)
    {
        List<TileCollider> allCollidersThatWereHit = new List<TileCollider>();

        Vector2 segmentDistance = (point2 - point1) / (totalPointsToCheck - 1);

        Vector2 originPointForRaycast = point1;

        for (int i = 0; i < totalPointsToCheck; i++)
        {
            RaycastHit2D rayHit = Physics2D.Raycast(originPointForRaycast, directionToCastRay, distanceToCastRay, LayerMask.GetMask("Environment"));
            if (rayHit)
            {
                TileCollider tileColliderThatWasHit = rayHit.collider.GetComponent<TileCollider>();
                if (tileColliderThatWasHit)
                {

                    allCollidersThatWereHit.Add(tileColliderThatWasHit);
                }
            }

            DebugSettings.DrawLineDirection(originPointForRaycast, directionToCastRay, distanceToCastRay, Color.red);
            originPointForRaycast += segmentDistance;
        }

        return allCollidersThatWereHit;
    }
    #endregion collision checks

    private void UpdateColliderBounds()
    {
        Vector2 originPoint = transform.position + new Vector3(centerPoint.x, centerPoint.y, 0);
        currentColliderBounds = new ColliderBounds();
        currentColliderBounds.bottomLeft = originPoint - Vector2.right * colliderSize.x - Vector2.up * colliderSize.y;
        currentColliderBounds.topRight = originPoint + Vector2.right * colliderSize.x + Vector2.up * colliderSize.y;
        currentColliderBounds.bottomRight = originPoint + Vector2.right * colliderSize.x - Vector2.up * colliderSize.y;
        currentColliderBounds.topLeft = originPoint - Vector2.right * colliderSize.x + Vector2.up * colliderSize.y;
    }


    #region structs
    private struct ColliderBounds
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;

        public override string ToString()
        {

            return topLeft.ToString() + " | " + topRight + " | " + bottomLeft.ToString() +  " | "  + bottomRight.ToString();
        }
    }
    #endregion structs
}
