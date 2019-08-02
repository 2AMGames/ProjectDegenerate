using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCapsuleCollider : CustomCollider2D
{
    public float size = 1;
    public float radius = 1;

    private bool drawHorizontal;
    public Vector2 capsuleOffset;

    #region monobehaviour methods
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            UpdateBoundsOfCollider();
        }

        Color colorToDraw = Color.green;
#if UNITY_EDITOR
        if (!drawHorizontal)
        {
            DebugSettings.DrawLine(bounds.topLeft, bounds.bottomLeft, colorToDraw);
            DebugSettings.DrawLine(bounds.topRight, bounds.bottomRight, colorToDraw);

            Vector2 offsetToCenter = (bounds.topRight - bounds.topLeft) / 2f;
            UnityEditor.Handles.color = colorToDraw;
            UnityEditor.Handles.DrawWireDisc(bounds.topLeft + offsetToCenter, Vector3.forward, radius);
            UnityEditor.Handles.DrawWireDisc(bounds.bottomLeft + offsetToCenter, Vector3.forward, radius);

        }
#endif
    }
#endregion monobehaviour methods

    public override CustomCollider2D[] GetAllTilesHitFromRayCasts(Vector2 v1, Vector2 v2, Vector2 direction, float distance, int rayCount)
    {
        throw new System.NotImplementedException();
    }

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

        BoundsRect b = new BoundsRect();
        Vector2 origin = this.transform.position + new Vector3(capsuleOffset.x, capsuleOffset.y);
        float xSize = drawHorizontal ? size : radius * 2;
        float ySize = drawHorizontal ? radius * 2 : size;


        b.topLeft = origin + Vector2.up * ySize / 2 - Vector2.right * xSize / 2;
        b.topRight = origin + Vector2.up * ySize / 2 + Vector2.right * xSize / 2;
        b.bottomLeft = origin - Vector2.up * ySize / 2 - Vector2.right * xSize / 2;
        b.bottomRight = origin - Vector2.up * ySize / 2 + Vector2.right * xSize / 2;

        this.bounds = b;
    }
}
