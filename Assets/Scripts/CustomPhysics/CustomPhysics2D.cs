using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Typically will only interact with the environment. Interactions with enemies or projectiles or other hitboxes should take place with a
/// hitbox trigger collider
/// </summary>
public class CustomPhysics2D : MonoBehaviour {
    #region const variables
    public const float GRAVITY_CONSTANT = 9.8f;
    #endregion const variables
    #region main variables
    [System.NonSerialized]
    public Vector2 velocity = Vector2.zero;
    [Header("Gravity Values")]
    [Tooltip("When this is marked true, gravity will effect the object based on the gravity scale and gravity vector")]
    public bool useGravity = true;
    [Tooltip("If this is marked true, then the object will stop accelerating once it has reached the maximum velocity that it can travel")]
    public bool useTerminalVelocity = true;
    [SerializeField]
    [Tooltip("The direction that gravity will be acting on the object")]
    private Vector2 gravityVector = Vector2.down;

    public List<CustomCollider2D> allCustomColliders { get; private set; }
    public CharacterStats associatedCharacterStats { get; set; }
    

    /// <summary>
    /// A boolean value that indicates whether or not our character is currently in the 
    /// </summary>
    public bool isInAir { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Vector2 gravityRight { get { return new Vector2(-gravityVector.y, gravityVector.x); } }
    public Vector2 gravityLeft { get { return new Vector2(gravityVector.y, -gravityVector.x); } }
    public Vector2 gravityUp { get { return -gravityVector; } }
    public Vector2 gravityDown { get { return gravityVector; } }


    [Tooltip("The scale at which gravity can effect the object. Potentially can be used for varying the jump feel")]
    public float gravityScale = 1;
    [Tooltip("The maximum speed at which the ")]
    public float terminalVelocity = 10;
    #endregion main variables


    #region monobehavoiur methods
    private void Awake()
    {
        allCustomColliders = new List<CustomCollider2D>();
    }

    private void Update()
    {
        if (useGravity) UpdateVelocityFromGravity();


        foreach (CustomCollider2D customCollider in allCustomColliders)
        {
            customCollider.UpdateCollisionPhysics();
        }
        UpdatePositionFromVelocity();
    }

    private void OnValidate()
    {
        gravityVector = gravityVector.normalized;
    }
    #endregion monobehaviour methods


    #region event methods
    /// <summary>
    /// This event will be called anytime our object lands on a ground tile after being considered airborne
    /// </summary>
    public delegate void PhysicsObjectGroundedEvent();
    public event PhysicsObjectGroundedEvent OnGroundedEvent;

    /// <summary>
    /// This event will be called anytime our object becomes airborne after being considered grounded
    /// </summary>
    public delegate void PhysicsObjectAirborneEvent();
    public event PhysicsObjectAirborneEvent OnAirborneEvent;
    #endregion event methods

    /// <summary>
    /// Updates the current velocity that is caused by gravitational force
    /// </summary>
    private void UpdateVelocityFromGravity()
    {
        if (useTerminalVelocity)
        {
            float dotGravity = Vector2.Dot(gravityVector, velocity);
            Vector2 downComponent = dotGravity * gravityVector;
            Vector2 rightComponent = Vector2.Dot(gravityRight, velocity) * gravityRight;

            if (downComponent.magnitude > terminalVelocity && dotGravity > 0)
            {
                velocity = rightComponent + gravityDown * terminalVelocity;
            }
        }
        float gravityValueToApply = gravityScale * GRAVITY_CONSTANT * Time.deltaTime;
        velocity += gravityValueToApply * gravityVector;
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdatePositionFromVelocity()
    {
        Vector3 velocityVector3 = new Vector3(velocity.x, velocity.y, 0);
        
        this.transform.position += velocityVector3 * Time.deltaTime;
    }

    /// <summary>
    /// Sets the direction that gravity will be applied if applicable
    /// </summary>
    /// <param name="gravityVector"></param>
    public void SetGravityVector(Vector2 gravityVector)
    {
        this.gravityVector = gravityVector.normalized;
    }

    /// <summary>
    /// Used to activate our character grounded trigger
    /// </summary>
    public void OnPhysicsObjectGrounded()
    {
        if (OnGroundedEvent == null) return;
        OnGroundedEvent();
    }

    /// <summary>
    /// Used to activate our character airborne trigger
    /// </summary>
    public void OnPhysicsOjbectAirborne()
    {
        if (OnAirborneEvent == null) return;
        OnAirborneEvent();
    }
 
}
