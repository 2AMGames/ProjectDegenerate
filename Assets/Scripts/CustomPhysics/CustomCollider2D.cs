using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Base class of our custom collider. This will check to see if there are any points where our collider intersects
/// with other colliders.
/// </summary>
public abstract class CustomCollider2D : MonoBehaviour {

    #region const variables
    protected readonly Color GIZMO_COLOR = Color.green;
    #endregion const variables
    [Tooltip("Mark this value true if you would like to treat this value as a trigger")]
    public bool isTrigger;
    
    /// <summary>
    /// The attached Custom physics component that is attached to our custom collider
    /// This is not required for components that are static.
    /// </summary>
    public CustomPhysics2D rigid { get; set; }

    /// <summary>
    /// Due to the fact that non static colliders will have the chance to have their velocity adjusted when colliding with other non static colliders
    /// it is good to keep a reference of the original velocity
    /// </summary>
    public Vector2 originalVelocity;

    /// <summary>
    /// IMPORTANT: If there is a Custom Physics object attached to the gameobject, this collider will be registered as a nonstatic collider
    /// </summary>
    public bool isStatic
    {
        get
        {
            return rigid == null;
        }
    }

    

    protected virtual void Awake()
    {
        UpdateBoundsOfCollider();
        rigid = GetComponent<CustomPhysics2D>();
        
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
        
    }

    /// <summary>
    /// Be sure to call this method
    /// </summary>
    public abstract void UpdateBoundsOfCollider();

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
    /// <param name="x"></param>
    /// <returns></returns>
    public abstract Vector2 GetLowerBoundsAtXValue(float x);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public abstract Vector2 GetUpperBoundsAtXValue(float x);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public abstract Vector2 GetRighBoundAtYValue(float y);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public abstract Vector2 GetLeftBoundAtYValue(float y);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract Vector2 GetCenter();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="colliderToCheck"></param>
    /// <returns></returns>
    public abstract bool ColliderIntersect(CustomCollider2D colliderToCheck);

    public abstract bool ColliderIntersectHorizontally(CustomCollider2D colliderToCheck);

    public abstract bool ColliderIntersectVertically(CustomCollider2D colliderToCheck);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collider"></param>
    public abstract void PushObjectOutsideOfCollider(CustomCollider2D collider);

    public virtual CustomCollider2D[] GetAllTilesHitFromRayCasts(Vector2 v1, Vector2 v2, Vector2 direction, float distance, int rayCount)
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
    public struct BoundsRect
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
        public Vector2 center;

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

        public void SetOffset(Vector2 offset)
        {
            topLeft += offset;
            topRight += offset;
            bottomLeft += offset;
            bottomRight += offset;
            center += offset;
        }

        public override string ToString()
        {
            return "TL: " + topLeft + "\nTR: " + topRight + "\nBL: " + bottomLeft + "\nBR: " + bottomRight;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct BoundsCircle
    {
        public Vector2 center;
        public float radius;
    }

    /// <summary>
    /// Bounds collider for our capsule collider simply contains two circles and a rect collider
    /// </summary>
    public struct BoundsCapsule
    {
        public BoundsRect rectBounds;
        public BoundsCircle topCircleBounds;
        public BoundsCircle bottomCircleBounds;
    }

    #region static methods
    /// <summary>
    /// Use this method to check if a rect bounds intersects another rect bound
    /// </summary>
    /// <returns></returns>
    public static bool RectIntersectRect(BoundsRect r1, BoundsRect r2)
    {
        Vector2 tl1 = r1.topLeft;
        Vector2 br1 = r1.bottomRight;
        Vector2 tl2 = r2.topLeft;
        Vector2 br2 = r2.bottomRight;

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

    /// <summary>
    /// Use this method to check if a rect bounds intersects a circle bounds
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
    /// <param name="intersectionPoint"></param>
    /// <returns></returns>
    public static bool RectIntersectCircle(BoundsRect r, BoundsCircle c)
    {

        Vector2 point = c.center;

        Vector2 A = r.topLeft;
        Vector2 B = r.topRight;
        Vector2 D = r.bottomLeft;
        float height = r.topLeft.y - r.bottomLeft.y;
        float width = r.topRight.x - r.topRight.x;
        float APdotAB = Vector2.Dot(point - A, B - A);
        float ABdotAB = Vector2.Dot(B - A, B - A);
        float APdotAD = Vector2.Dot(point - A, D - A);
        float ADdotAD = Vector2.Dot(D - A, D - A);
        if (0 <= APdotAB && APdotAB <= ABdotAB && 0 <= APdotAD && APdotAD < ADdotAD)
        {
            return true;

        }
        
        return LineIntersectCircle(c, r.bottomLeft, r.topRight);
        //float rectX = r.bottomLeft.x;
        //float recty = r.bottomLeft.y;

        //float nearestX = Mathf.Max(rectX, Mathf.Min(point.x, rectX + width));
        //float nearestY = Mathf.Max(recty, Mathf.Min(point.y, recty + height));

        //float dX = point.x - nearestX;
        //float dY = point.y - nearestY;

        //return (dX * dX + dY * dY) < c.radius * c.radius;
    }

    /// <summary>
    /// Use this method to check if two circle bounds are intersecting with each other
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <param name="intersectionPoint"></param>
    /// <returns></returns>
    public static bool CircleIntersectCircle(BoundsCircle c1, BoundsCircle c2)
    {
        float distanceMax = c1.radius + c2.radius;
        float distance = Vector2.Distance(c1.center, c2.center);

        return distance <= distanceMax;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static bool CapsuleIntersectCapsule(BoundsCapsule c, BoundsCapsule r)
    {
        if (CapsuleIntersectRect(c, r.rectBounds))
        {
            return true;
        }
        if (CapsuleIntersectCircle(c, r.topCircleBounds))
        {
            return true;
        }
        if (CapsuleIntersectCircle(c, r.bottomCircleBounds)) ;
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cap"></param>
    /// <param name="cir"></param>
    /// <returns></returns>
    public static bool CapsuleIntersectCircle(BoundsCapsule cap, BoundsCircle cir)
    {
        if (CircleIntersectCircle(cap.bottomCircleBounds, cir))
        {
            return true;
        }
        if (CircleIntersectCircle(cap.topCircleBounds, cir))
        {
            return true;
        }
        if (RectIntersectCircle(cap.rectBounds, cir))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static bool CapsuleIntersectRect(BoundsCapsule c, BoundsRect r)
    {
        if (RectIntersectCircle(r, c.bottomCircleBounds))
        {
            return true;
        }
        if (RectIntersectCircle(r, c.bottomCircleBounds))
        {
            return true;
        }
        if (RectIntersectRect(r, c.rectBounds))
        {
            return true;
        }

        return false;
    }

    #region Line Intersection methods

    /// <summary>
    /// Returns true if the line that was passed in intersect with the given circle
    /// </summary>
    /// <param name="c"></param>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool LineIntersectCircle(BoundsCircle c, Vector2 pointA, Vector2 pointB)
    {
        
        Vector2 point = c.center;

        float rectX = pointA.x;
        float recty = pointA.y;

        float nearestX = Mathf.Max(rectX, Mathf.Min(point.x, pointB.x));
        float nearestY = Mathf.Max(recty, Mathf.Min(point.y, pointB.y));

        float dX = point.x - nearestX;
        float dY = point.y - nearestY;

        return (dX * dX + dY * dY) < c.radius * c.radius;
    }

    /// <summary>
    /// Overload method of our line intersect method
    /// </summary>
    /// <param name="c"></param>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool LineIntersectCircle(BoundsCircle c, Vector2 origin, Vector2 direction, float length)
    {
        Vector2 pointA = origin;
        Vector2 pointB = origin + direction * length;
        return LineIntersectCircle(c, pointA, pointB);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bounds"></param>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool LineIntersectRect(BoundsRect bounds, Vector2 origin, Vector2 direction, float length)
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
    /// Returns true if the line passes through the given capsule
    /// </summary>
    /// <param name="bounds"></param>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static bool LineIntersectsCapsule(BoundsCapsule bounds, Vector2 origin, Vector2 direction, float length)
    {
        if (LineIntersectCircle(bounds.topCircleBounds, origin, direction, length))
        {
            return true;
        }
        if (LineIntersectCircle(bounds.bottomCircleBounds, origin, direction, length))
        {
            return true;
        }
        if (LineIntersectRect(bounds.rectBounds, origin, direction, length))
        {
            return true;
        }
        return false;
    }


    /// <summary>
    /// Return true if the two lines intersect. u0 and v0 are line 1 and u1 and v1 are line 2
    /// </summary>
    public static bool LineCrossLine(Vector2 u0, Vector2 v0, Vector2 u1, Vector2 v1)
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

    /// <summary>
    /// Returns the determinant of the two 2D vectors
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static float GetDeterminant(Vector2 v1, Vector2 v2)
    {
        return -v2.x * v1.y + v1.x * v2.y;
    }
    #endregion line intersection methods


    #region get outter bounds of collider

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Vector2 GetLowerBoundsAtXValueCircle(BoundsCircle cBounds, float x)
    {
        float adjustedX = x - cBounds.center.x;

        float angle = Mathf.Acos(adjustedX / cBounds.radius);
        return new Vector2(x, -Mathf.Sin(angle) * cBounds.radius + cBounds.center.y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Vector2 GetUpperBoundsAtXValueCircle(BoundsCircle cBounds, float x)
    {
        float adjustedX = x - cBounds.center.x;

        float angle = Mathf.Acos(adjustedX / cBounds.radius);
        return new Vector2(x, Mathf.Sin(angle) * cBounds.radius + cBounds.center.y);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector2 GetRighBoundAtYValueCircle(BoundsCircle cBounds, float y)
    {
        float adjustedY = y - cBounds.center.y;
        float angle = Mathf.Asin(adjustedY / cBounds.radius);
        return new Vector2(Mathf.Cos(angle) * cBounds.radius + cBounds.center.x, y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector2 GetLeftBoundAtYValueCircle(BoundsCircle cBounds, float y)
    {
        float adjustedY = y - cBounds.center.y;
        float angle = Mathf.Asin(adjustedY / cBounds.radius);
        return new Vector2(-Mathf.Cos(angle) * cBounds.radius + cBounds.center.x, y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Vector2 GetLowerBoundsAtXValueRect(BoundsRect rBounds, float x)
    {
        return new Vector2(x, rBounds.bottomLeft.y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Vector2 GetUpperBoundsAtXValueRect(BoundsRect rBounds, float x)
    {
        return new Vector2(x, rBounds.topRight.y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector2 GetRighBoundAtYValueRect(BoundsRect rBounds, float y)
    {
        return new Vector2(rBounds.topRight.x, y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector2 GetLeftBoundAtYValueRect(BoundsRect rBounds, float y)
    {
        return new Vector2(rBounds.bottomLeft.x, y);
    }
    #endregion get outter bounds of collider
    #endregion static methods
}
