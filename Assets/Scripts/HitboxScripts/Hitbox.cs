using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Our custom collider hitbox class. Hitboxes do not interact with any other type of collider except for other hitboxes and hurtboxes
/// Do not use this to interact with the environment or activate the triggers
/// </summary>
public class Hitbox : MonoBehaviour
{
    #region const variables
    private Color GIZMO_COLOR = Color.red;
    private Color GIZMO_HURTBOX_COLOR = Color.blue;
    #endregion const variables

    public enum HitboxType
    {
        Hitbox,
        Hurtbox,
    }

    public HitboxType hitboxType = HitboxType.Hurtbox;

    public Vector2 boxColliderSize;
    public Vector2 boxColliderPosition;

    private BoxColliderPoints currentColliderPoints;


    #region monobehaviour methods
    private void Awake()
    {
        
    }

    private void OnDestroy()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        UpdateBoxColliderPoints();

        Debug.DrawLine(currentColliderPoints.topRight, currentColliderPoints.topLeft, GIZMO_COLOR);
        Debug.DrawLine(currentColliderPoints.topRight, currentColliderPoints.bottomRight, GIZMO_COLOR);
        Debug.DrawLine(currentColliderPoints.topLeft, currentColliderPoints.bottomLeft, GIZMO_COLOR);
        Debug.DrawLine(currentColliderPoints.bottomLeft, currentColliderPoints.bottomRight, GIZMO_COLOR);
    }
    #endregion monobehaviour methods

    private void UpdateBoxColliderPoints()
    {
        currentColliderPoints = new BoxColliderPoints();
        Vector2 origin = this.transform.position + new Vector3(boxColliderPosition.x, boxColliderPosition.y);

        currentColliderPoints.topLeft = origin + Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        currentColliderPoints.topRight = origin + Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;
        currentColliderPoints.bottomLeft = origin - Vector2.up * boxColliderSize.y / 2 - Vector2.right * boxColliderSize.x / 2;
        currentColliderPoints.bottomRight = origin - Vector2.up * boxColliderSize.y / 2 + Vector2.right * boxColliderSize.x / 2;

    }

    private struct BoxColliderPoints
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }
}
