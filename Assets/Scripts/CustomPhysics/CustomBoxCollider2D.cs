using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBoxCollider2D : CustomCollider2D
{
    public Vector2 boxColliderSize = Vector2.one;
    public Vector2 boxColliderPosition;

    

    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

        Color colorToDraw = Color.green;

        DebugSettings.DrawLine(bounds.bottomLeft, bounds.bottomRight, colorToDraw);
        DebugSettings.DrawLine(bounds.bottomRight, bounds.topRight, colorToDraw);
        DebugSettings.DrawLine(bounds.topRight, bounds.topLeft, colorToDraw);
        DebugSettings.DrawLine(bounds.topLeft, bounds.bottomLeft, colorToDraw);
    }



    /// <summary>
    /// This should be called by our HitboxManager
    /// </summary>
    public override void UpdateBoundsOfCollider()
    {
        base.UpdateBoundsOfCollider();
        
        BoundsRect b = new BoundsRect();
        Vector2 origin = this.transform.position + new Vector3(boxColliderPosition.x, boxColliderPosition.y);

        b.topLeft = origin + Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        b.topRight = origin + Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;
        b.bottomLeft = origin - Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        b.bottomRight = origin - Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;

        this.bounds = b;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public override bool LineIntersectWithCollider(Vector2 origin, Vector2 direction, float length)
    {
        
        Vector2 v0 = direction * length;
        Vector2 endpoint = origin + v0;

        Vector2 tr = bounds.topRight;
        Vector2 bl = bounds.bottomLeft;

        if (bl.x < origin.x && origin.x < tr.x && bl.y < origin.y && origin.y < tr.y)
        {
            return true;
        }


        if (LineCrossLine(origin, v0, bounds.bottomLeft, (bounds.bottomRight - bounds.bottomLeft)))
        {
            return true;
        }
        if (LineCrossLine(origin, v0, bounds.bottomRight, (bounds.topRight - bounds.bottomRight)))
        {
            return true;
        }
        if (LineCrossLine(origin, v0, bounds.topRight, (bounds.topLeft - bounds.topRight)))
        {
            return true;
        }
        if (LineCrossLine(origin, v0, bounds.topLeft, (bounds.bottomLeft - bounds.topLeft)))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    private bool LineCrossLine(Vector2 u0, Vector2 v0, Vector2 u1, Vector2 v1)
    {
        float d1 = GetDeterminant(v1, v0);
        if (d1 == 0)
        {
            return false;
        }
        

        float s = (1 / d1) * (((u0.x - u1.x) * v0.y) - ((u0.y - u1.y) * v0.x));
        float t = (1 / d1) * -((-(u0.x - u1.x) * v1.y) + ((u0.y - u1.y) * v1.x));
       
        return s > 0 && s < 1 && t > 0 && t < 1;
    }

    private float GetDeterminant(Vector2 v1, Vector2 v2)
    {
        return -v2.x * v1.y + v1.x * v2.y;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <param name="rayCount"></param>
    /// <returns></returns>
    public override CustomCollider2D[] GetAllTilesHitFromRayCasts(Vector2 v1, Vector2 v2, Vector2 direction, float distance, int rayCount)
    {
        Vector2 offset = (v2 - v1) / (rayCount - 1);
        List<CustomCollider2D> lineColliders;
        HashSet<CustomCollider2D> allLines = new HashSet<CustomCollider2D>();
        for (int i = 0; i < rayCount; i++)
        {
            Overseer.Instance.ColliderManager.CheckLineIntersectWithCollider(v1 + offset * i, direction, distance, out lineColliders);
            foreach (CustomCollider2D c in lineColliders)
            {
                if (c != this)
                {
                    allLines.Add(c);
                } 
            }
        }
        
        CustomCollider2D[] allValidColliderList = new CustomCollider2D[allLines.Count];
        allLines.CopyTo(allValidColliderList);
        return allValidColliderList;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override bool CheckCollisionDownFromVelocity()
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override bool CheckCollisionUpFromVelocity()
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override bool CheckCollisionRightFromVelocity()
    {
        if (rigid.velocity.x <= 0)
        {
            return false;
        }
        Vector2 adjustedPoint1 = bounds.topRight - Vector2.right * HorizontalBuffer - VerticalBuffer * Vector2.up;
        Vector2 adjustedPoint2 = bounds.bottomRight - Vector2.right * HorizontalBuffer + VerticalBuffer * Vector2.up;
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override bool CheckCollisionLeftFromVelocity()
    {
        if (rigid.velocity.x >= 0)
        {
            return false;
        }
        Vector2 adjustedPoint1 = bounds.topLeft + Vector2.right * HorizontalBuffer - VerticalBuffer * Vector2.up;
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

    /// <summary>
    /// Whenever we intersect with a collider this method should be called to move the collider outside
    /// </summary>
    public override void PushObjectOutsideOfCollider(CustomCollider2D collider)
    {
        if (collider.isStatic)
        {
            return;
        }
        CustomBoxCollider2D bCollider = (CustomBoxCollider2D)collider;
        Vector2 tr1 = previousBounds.topRight;
        Vector2 bl1 = previousBounds.bottomLeft;

        Vector2 tr2 = bCollider.previousBounds.topRight;
        Vector2 bl2 = bCollider.previousBounds.bottomLeft;

        Vector2 downLeftVec = tr1 - bl2;
        Vector2 upRightVec = tr2 - bl1;
        

        if (downLeftVec.x <= 0)
        {
            print("DLX");
            bCollider.transform.position = new Vector3(bounds.topRight.x - (-bCollider.transform.position.x + bl2.x) + .01f, bCollider.transform.position.y, bCollider.transform.position.z);
        }
        if (downLeftVec.y < 0)
        {
            print("DLY");
            bCollider.transform.position = new Vector3(bCollider.transform.position.x, bounds.bottomLeft.y - (bCollider.transform.position.y - tr2.y), bCollider.transform.position.z);
        }
        if (upRightVec.x <= 0)
        {
            print("URX");
            bCollider.transform.position = new Vector3(bounds.bottomLeft.x + (bCollider.transform.position.x - tr2.x) - .01f, bCollider.transform.position.y, bCollider.transform.position.z);
        }
        if (upRightVec.y < 0)
        {
            print("URY");
            bCollider.transform.position = new Vector3(bCollider.transform.position.x, bounds.topRight.y + (-bCollider.transform.position.y + bl2.y), bCollider.transform.position.z);
        }
        bCollider.UpdateBoundsOfCollider();
    }
}
