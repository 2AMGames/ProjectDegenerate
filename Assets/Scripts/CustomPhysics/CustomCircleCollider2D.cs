using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCircleCollider2D : CustomCollider2D
{
    public float radius;
    public BoundsCircle bounds;
    
    #region monobehaviour methods
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

#if UNITY_EDITOR

        UnityEditor.Handles.DrawWireDisc()
#endif
    }
    #endregion monobehaviour methods

    public override bool LineIntersectWithCollider(Vector2 origin, Vector2 direction, float length)
    {
        throw new System.NotImplementedException();
    }

    public override void PushObjectOutsideOfCollider(CustomCollider2D collider)
    {
        throw new System.NotImplementedException();
    }

    protected override bool CheckCollisionDownFromVelocity()
    {
        throw new System.NotImplementedException();
    }

    protected override bool CheckCollisionLeftFromVelocity()
    {
        throw new System.NotImplementedException();
    }

    protected override bool CheckCollisionRightFromVelocity()
    {
        throw new System.NotImplementedException();
    }

    protected override bool CheckCollisionUpFromVelocity()
    {
        throw new System.NotImplementedException();
    }


    public override void UpdateBoundsOfCollider()
    {
        base.UpdateBoundsOfCollider();
        BoundsCircle cBounds = new BoundsCircle();
        cBounds.center = this.transform.position;
        cBounds.radius = radius;
    }
}
