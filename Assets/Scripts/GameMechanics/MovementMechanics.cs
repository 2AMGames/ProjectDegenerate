using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is the base class that handles any functions that revolve around movement. This includes
/// running, jumping, crouching, etc. 
/// </summary>
[RequireComponent(typeof(CustomPhysics2D))]
public class MovementMechanics : MonoBehaviour {

    #region const variables

    private const string SPEED_ANIMATION_PARAMETER = "Speed";
    private const string IN_AIR_ANIMATION_PARAMETER = "InAir";
    private const string IS_CROUCHING_PARAMETER = "IsCrouching";
    private const string VERTICAL_SPEED_ANIMATION_PARAMETER = "VerticalSpeed";
    private const string HORIZONTAL_INPUT = "HorizontalInput";
    private const string VERTICAL_INPUT = "VerticalInput";
    private const string JUMP_TRIGGER = "Jump";

    private const float CROUCHING_THRESHOLD = .6F;

    #endregion

    #region action events
    public UnityEngine.Events.UnityAction<bool> OnDirectionChanged;
    #endregion action events

    #region main variables
    [Header("Mono References")]
    [Tooltip("A reference to the sprite renderer object")]
    public SpriteRenderer spriteRenderer;
    [Header("Ground Movement")]
    [Tooltip("The maximum walking speed")]
    public float walkingSpeed = 2f;
    [Tooltip("The maximum running speed")]
    public float runningSpeed = 5f;
    [Tooltip("The units per second that our speed will increase")]
    public float groundAcceleration = 25f;
    public float maximumAirSpeed = 8f;
    public float airAcceleration = 20f;

    [Header("Sprite Renderer Referneces")]
    [Tooltip("This indicates what direction our sprite will be facing. This will change based on input")]
    public bool isFacingRight;

    [Header("Jumping Variables")]
    [Tooltip("The time it will take for our character to reach the top of their jump")]
    public float timeToReachJumpApex = 1;
    [Tooltip("The height in units that our character will reach when they perform a jump")]
    public float heightOfJump = 1;
    [Tooltip("The scale we apply to our character when they are fast falling, typically meaning the player isn't holding down the jump button")]
    public float fastFallScale = 1.7f;
    [Tooltip("The number of jumps that our character is allowed to perform. If 0 is set than this character is not allowed to jump in any situation.")]
    public int maxAvailableJumps = 1;
    [Tooltip("Indicates whether our character is fast falling or not")]
    private bool isFastFalling = false;
    [Tooltip("The calculated acceleration that will be applied to the character when they are in the air")]
    private float jumpingAcceleration = 1f;

    [Header("Dashing Variables")]
    [Tooltip("The animation curve that will determine the velocity at which our character will move at points within our dash animation")]
    public AnimationCurve dashVelocityAnimationCurve;
    public float maxDashSpeed = 3f;
    [Tooltip("The time in seconds to complete a dashing animation")]
    public float timeToCompleteDash = .6f;
    public float delayBeforeDashing = .15f;
    [HideInInspector]
    /// <summary>
    /// If this value is set to true, we will act as if all inputs are set to 0. If there is an action that should occur where the character
    /// should not move, mark this value as true
    /// </summary>
    public bool ignoreJoystickInputs;
    /// <summary>
    /// Some characters may have the ability to move while they are crouching. Mark this value to true if they cay
    /// </summary>
    public bool canMoveWhileCrouching;
    [HideInInspector]
    /// <summary>
    /// Ignores inputs related to the jump function
    /// </summary>
    public bool ignoreJumpButton;

    /// <summary>
    /// Can this actor change direction. Usually set to true when animating an attack move.
    /// </summary>
    public bool CanChangeDirection = true;
    
    /// <summary>
    /// The number of jumps remaining that a character can pull off before landing. Once they land, their jumps will typically
    /// be reset back to the max number of jumps they have
    /// </summary>
    private int currentJumpsAvailable;

    private CharacterStats.CharacterState currentMovementState = CharacterStats.CharacterState.FreeMovement;
    /// <summary>
    /// Jump velocity is calculated based on the jump height and time to reach apex
    /// </summary>
    private float jumpVelocity;

    /// <summary>
    /// The custom physics component reference
    /// </summary>
    private CustomPhysics2D rigid;

    private InteractionHandler InteractionHandler;

    /// <summary>
    /// The last horizontal input that was passed in
    /// </summary>
    private int horizontalInput;
    private int verticalInput;
    private Animator anim;

    public bool IsCrouching { get; private set; }

    public bool IsInAir
    {
        get
        {
            return rigid.isInAir; 
        }
    }

    public Vector2 Velocity
    {
        get
        {
            return rigid.Velocity;
        }
    }

    private IEnumerator ForcedMovementCoroutine;

    #endregion main variables

    #region monobehaivour methods
    private void Awake()
    {
        rigid = GetComponent<CustomPhysics2D>();
        anim = GetComponent<Animator>();
        InteractionHandler = GetComponent<InteractionHandler>();

        currentJumpsAvailable = maxAvailableJumps;

        rigid.OnGroundedEvent += this.OnGroundedEvent;
        rigid.OnAirborneEvent += this.OnAirborneEvent;

        CommandInterpreter commandInterpreter = GetComponent<CommandInterpreter>();
        if (commandInterpreter != null)
        {
            commandInterpreter.OnDirectionSetEvent += JoystickDirectionSet;
        }
        InitializeMovementParameters();
    }

    private void Update()
    {
        // If the animator is overriding the velocity, we should not update the velocity due to input.
        if (!rigid.UseAnimatorVelocity)
        {
            if (rigid.isInAir)
            {
                UpdateCurrentSpeedInAir();
            }
            else
            {
                UpdateCurrentSpeedOnGround();
            }
        }

        if (anim && anim.runtimeAnimatorController)
        {
            anim.SetFloat(VERTICAL_SPEED_ANIMATION_PARAMETER, rigid.Velocity.y);
        }

        if (!IsCrouching && verticalInput <= -CROUCHING_THRESHOLD)
        {
            IsCrouching = true;
            if (anim && anim.runtimeAnimatorController)
            {
                anim.SetBool(IS_CROUCHING_PARAMETER, IsCrouching);
            }
        }
        else if(IsCrouching && verticalInput > -CROUCHING_THRESHOLD)
        {
            IsCrouching = false;
            anim.SetBool(IS_CROUCHING_PARAMETER, IsCrouching);
        }

        PlayerController opponent = Overseer.Instance.GetNextCharacterByIndex(GetComponent<CharacterStats>().PlayerIndex);
        if (CanChangeDirection && opponent != null)
        {
            FlipSpriteBasedOnOpponentDirection(opponent.CharacterStats.transform);
        }
    }

    private void OnValidate()
    {
        if (!rigid)
        {
            rigid = GetComponent<CustomPhysics2D>();
        }
        SetSpriteFlipped(isFacingRight);
        InitializeMovementParameters();
    }
   
    private void OnDestroy()
    {
        rigid.OnGroundedEvent -= this.OnGroundedEvent;//Make sure to unsubscribe from events to avoid errors and memory leaks
        rigid.OnAirborneEvent -= this.OnAirborneEvent;
    }
    #endregion monobehaviour methods

    #region public methods

    /// <summary>
    /// Sets the horizontal input that will determine the speed that our character will move
    /// </summary>
    /// <param name="horizontalInput"></param>
    public void SetHorizontalInput(float horizontalInput)
    {
        if (canMoveWhileCrouching && IsCrouching)
        {
            horizontalInput = 0;
        }

        this.horizontalInput = (int)Mathf.Sign(horizontalInput) * (Mathf.Abs(horizontalInput) > PlayerController.INPUT_THRESHOLD_RUNNING ? 1:0 );
       //FlipSpriteBasedOnInput(this.horizontalInput);
        if (anim && anim.runtimeAnimatorController)
        {
            //anim.SetFloat(SPEED_ANIMATION_PARAMETER, Mathf.Abs(horizontalInput));
            anim.SetInteger(HORIZONTAL_INPUT, (isFacingRight ? 1 : -1) * this.horizontalInput);
        }
    }

    /// <summary>
    /// Sets the vertical input that will effect states that involve vertical movement, such as crouching,
    /// climbing and fast=falling
    /// </summary>
    /// <param name="verticalInput"></param>
    public void SetVerticalInput(float verticalInput)
    {
        verticalInput = verticalInput;
        this.verticalInput = (int)Mathf.Sign(verticalInput) * (Mathf.Abs(verticalInput) > PlayerController.INPUT_THRESHOLD_RUNNING ? 1 : 0);
        anim.SetInteger(VERTICAL_INPUT, this.verticalInput);
    }

    /// <summary>
    /// Gets the currently set vertical input that will be used for movement
    /// </summary>
    /// <returns></returns>
    public float GetVerticalInput()
    {
        return this.verticalInput;
    }

    /// <summary>
    /// Gets the currently set horizontal input that will be used for movement
    /// </summary>
    /// <returns></returns>
    public float GetHorizontalInput()
    {
        return this.horizontalInput;
    }

    /// <summary>
    /// Flips sprite depending on direction of opponent.
    /// If opponent is in the air, should not rotate sprite until grounded
    /// </summary>
    ///   /// <param name="opponentPosition"></param>

    public void FlipSpriteBasedOnOpponentDirection(Transform opponentPosition)
    {
        if (rigid.isInAir)
        {
            return;
        }

        //Vector2 facingDirection = transform.right.normalized;
        Vector2 opponentDirection = (opponentPosition.position - transform.position).normalized;
        //float dotProduct = Vector2.Dot(facingDirection, opponentDirection);
        if (opponentDirection.x < 0 && isFacingRight)
        {
            SetSpriteFlipped(false);
        }
        if (opponentDirection.x > 0 && !isFacingRight)
        {
            SetSpriteFlipped(true);
        }
        //SetSpriteFlipped(opponentDirection.x >= 0.0f);

    }

    #endregion

    #region private methods

    /// <summary>
    /// Updates the speed of our character while they are grounded
    /// </summary>
    private void UpdateCurrentSpeedOnGround()
    {
        float goalSpeed = 0;
        if (Mathf.Abs(horizontalInput) > PlayerController.INPUT_THRESHOLD_RUNNING)
        {
            goalSpeed = runningSpeed * Mathf.Sign(horizontalInput);
        }
        
        Vector2 newVelocityVector = new Vector2(rigid.Velocity.x, rigid.Velocity.y);
        newVelocityVector.x = Mathf.MoveTowards(rigid.Velocity.x, goalSpeed, Overseer.DELTA_TIME * groundAcceleration);
        rigid.Velocity = newVelocityVector;
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateCurrentSpeedInAir()
    {
        if (Mathf.Abs(this.horizontalInput) < PlayerController.INPUT_THRESHOLD_RUNNING)
        {
            return;
        }
        float goalSpeed = Mathf.Sign(horizontalInput) * maximumAirSpeed;

        float updatedXVelocity = rigid.Velocity.x;
        updatedXVelocity = Mathf.MoveTowards(updatedXVelocity, goalSpeed, 
            Overseer.DELTA_TIME * airAcceleration);

        Vector2 updatedVectorVelocity = new Vector2(updatedXVelocity, rigid.Velocity.y);
        rigid.Velocity = updatedVectorVelocity;
    }


    /// <summary>
    /// Flips the character's sprite appropriately based on the input passed through
    /// </summary>
    /// <param name="horizontalInput"></param>
    private void FlipSpriteBasedOnInput(float horizontalInput, bool ignoreInAirCondition = false)
    {
       

        /*

        if (horizontalInput < 0 && isFacingRight)
        {
            SetSpriteFlipped(false);
        }
        else if (horizontalInput > 0 && !isFacingRight)
        {
            SetSpriteFlipped(true);
        }
        */
    }

    /// <summary>
    /// Sets whether our sprite is facing left or right
    /// </summary>
    private void SetSpriteFlipped(bool spriteFacingright)
    {

        if (!spriteRenderer)
        {
            return;
        }
        this.isFacingRight = spriteFacingright;
        if (spriteFacingright)
        {
            Vector3 currentScale = spriteRenderer.transform.parent.localScale;
            currentScale.x = -Mathf.Abs(currentScale.x);
            spriteRenderer.transform.parent.localScale = currentScale;
        }
        else
        {
            Vector3 currentScale = spriteRenderer.transform.parent.localScale;
            currentScale.x = Mathf.Abs(currentScale.x); ;
            spriteRenderer.transform.parent.localScale = currentScale;
        }
        OnDirectionChanged?.Invoke(spriteFacingright);
    }

    private void InitializeMovementParameters()
    {
        float gravity = (2 * heightOfJump) / Mathf.Pow(timeToReachJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToReachJumpApex;
        jumpingAcceleration = gravity / CustomPhysics2D.GRAVITY_CONSTANT;
        rigid.gravityScale = jumpingAcceleration;

        maxAvailableJumps = Mathf.Max(maxAvailableJumps, 0);
        maximumAirSpeed = Mathf.Max(maximumAirSpeed, 0);
    }

    #endregion

    #region jumping methods
    /// <summary>
    /// Method that performs a jump by applying a velocity in the y direction
    /// </summary>
    /// <returns></returns>
    public bool Jump()
    {
        if (ignoreJumpButton)
        {
            return false;
        }
        if (!rigid.isInAir)
        {
        }
        else if (currentJumpsAvailable > 0)
        {
            currentJumpsAvailable--;

        }
        else
        {
            return false;
        }

        //FlipSpriteBasedOnInput(this.horizontalInput, true);
        anim.SetTrigger(JUMP_TRIGGER);
        rigid.Velocity = new Vector2(rigid.Velocity.x, jumpVelocity);
        SetCharacterFastFalling(false);
        return true;
    }

    /// <summary>
    /// When our character is in the air, out player can control the rate at which they fall by holding down the jump
    /// button. If they release the jumpbutton, we should use the fastfall values
    /// </summary>
    public void SetCharacterFastFalling(bool isFastFalling)
    {
        this.isFastFalling = isFastFalling;
        if (!isFastFalling)
        {
            rigid.gravityScale = jumpingAcceleration;
        }
        else
        {
            rigid.gravityScale = jumpingAcceleration * fastFallScale;
        }
    }
    
    /// <summary>
    /// This method will be called any time our character touches the ground after being
    /// in an in-air state
    /// </summary>
    public void OnGroundedEvent()
    {
        if (anim && anim.runtimeAnimatorController)
        {
            anim.SetBool(IN_AIR_ANIMATION_PARAMETER, false);
        }
        this.currentJumpsAvailable = maxAvailableJumps;
    }

    /// <summary>
    /// This method will be called every time our character is put in the air after being in the grounded state
    /// </summary>
    public void OnAirborneEvent()
    {
        if (anim && anim.runtimeAnimatorController)
        {
            anim.SetBool(IN_AIR_ANIMATION_PARAMETER, true);
        }
        SetCharacterFastFalling(false);
        this.currentJumpsAvailable--;
    }


    private void JoystickDirectionSet(CommandInterpreter.DIRECTION direction, Vector2Int joystickDirectionVec)
    {
        SetHorizontalInput(joystickDirectionVec.x);
        SetVerticalInput(joystickDirectionVec.y);
    }
    #endregion jumping methods

    #region dashing methods
    /// <summary>
    /// Begins the coroutine to have our character dashing
    /// </summary>
    public void Dash()
    {
        if (currentMovementState != CharacterStats.CharacterState.FreeMovement)
        {
            return;
        }
        StartCoroutine(DashCoroutine());
    }

    /// <summary>
    /// When our character is performaing a dashing action, this coroutine will handle all the movement
    /// based on the animation curve that is set up
    /// </summary>
    /// <returns></returns>
    private IEnumerator DashCoroutine()
    {
        currentMovementState = CharacterStats.CharacterState.Dashing;
        rigid.useGravity = false;
        float timeThatHasPassed = 0;
        while (timeThatHasPassed < delayBeforeDashing)
        {
            rigid.Velocity = Vector2.zero;
            timeThatHasPassed += Overseer.DELTA_TIME;
            yield return null;
        }
        timeThatHasPassed = 0;
        Vector2 directionOfInput = new Vector2(horizontalInput, verticalInput).normalized;
        if (directionOfInput == Vector2.zero)
        {
            directionOfInput = new Vector2(this.spriteRenderer.transform.localScale.x, 0).normalized;
        }
        while (timeThatHasPassed < timeToCompleteDash)
        {
            rigid.Velocity = directionOfInput * dashVelocityAnimationCurve.Evaluate(timeThatHasPassed / timeToCompleteDash) * maxDashSpeed;
            timeThatHasPassed += Overseer.DELTA_TIME;
            yield return null;
        }

        currentMovementState = CharacterStats.CharacterState.FreeMovement;
        rigid.Velocity.x = Mathf.Sign(rigid.Velocity.x) * Mathf.Min(Mathf.Abs(rigid.Velocity.x), maximumAirSpeed);
        rigid.useGravity = true;
    }
    #endregion dashing methods

    #region player interaction methods

    /// <summary>
    /// Method that handles player state when their hurtbox is inflitrated by an active hitbox.
    /// </summary>
    public void HandlePlayerHit(Hitbox enemyHitbox, InteractionHandler.MoveData move)
    {
    }

    /// <summary>
    /// Method for moving characters due to forced movement (guard knockback, hit knockback, etc).
    /// </summary>
    /// <param name="destinationVector"></param>
    public void TranslateForcedMovement(Vector2 destinationVector)
    {
        Vector2 newVelocity = Vector2.Lerp(rigid.Velocity, destinationVector, .2f);
        rigid.Velocity = newVelocity;
    }

    #endregion

    #region coroutines

    private IEnumerator LoseUpwardMomentumFromJump()
    {
        if (rigid.Velocity.y < 0)
        {
            yield break;
        }
    }

    #endregion

}
