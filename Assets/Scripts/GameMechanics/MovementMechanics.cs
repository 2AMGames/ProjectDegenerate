﻿using System.Collections;
using UnityEngine;


/// <summary>
/// This is the base class that handles any functions that revolve around movement. This includes
/// running, jumping, crouching, etc. 
/// </summary>
[RequireComponent(typeof(CustomPhysics2D))]
[RequireComponent(typeof(Animator))]
public class MovementMechanics : MonoBehaviour {

    #region const variables

    public const string VERTICAL_SPEED_ANIMATION_PARAMETER = "VerticalSpeed";

    private const string IN_AIR_ANIMATION_PARAMETER = "InAir";
    private const string IS_CROUCHING_PARAMETER = "IsCrouching";
    private const string HORIZONTAL_INPUT = "HorizontalInput";
    private const string VERTICAL_INPUT = "VerticalInput";
    private const string DashCount= "DashCount";

    public static readonly int ShouldChangeDirection = Animator.StringToHash("ShouldChangeDirection");
    public static readonly int JumpTrigger = Animator.StringToHash("Jump");

    private const float CROUCHING_THRESHOLD = .6f;

    #endregion

    #region action events
    public UnityEngine.Events.UnityAction<bool> OnDirectionChanged;
    #endregion action events

    #region main variables
    [Header("Ground Movement")]
    [Tooltip("The maximum walking speed")]
    public float WalkingSpeed = 2f;
    [Tooltip("The maximum running speed")]
    public float RunningSpeed = 5;
    [Tooltip("The units per second that our speed will increase")]
    public float GroundAcceleration = 25f;
    [Tooltip("Factor of acceleration. Set to 0 if velocity should not use acceleration.")]
    public float GroundAccelerationScale;
    public float MaximumAirSpeed = 8f;
    public float AirAcceleration = 20f;

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
    public float JumpingAcceleration = 1f;

    [Header("Dashing Variables")]
    public bool IsDashing;
    public float GroundDashSpeed = 6.5f;
    public float AirDashSpeed = 6.5f;
    
    /// <summary>
    /// The number of jumps remaining that a character can pull off before landing. Once they land, their jumps will typically
    /// be reset back to the max number of jumps they have
    /// </summary>
    private int currentJumpsAvailable;
    /// <summary>
    /// Jump velocity is calculated based on the jump height and time to reach apex
    /// </summary>
    private float jumpVelocity;

    /// <summary>
    /// The custom physics component reference
    /// </summary>
    private CustomPhysics2D rigid;

    /// <summary>
    /// The last horizontal input that was passed in
    /// </summary>
    private int horizontalInput;
    private int verticalInput;
    private Animator anim;

    public bool IsCrouching;
    public Vector2 GoalVelocity;

    public bool IsInAir
    {
        get
        {
            return rigid.isInAir; 
        }
    }

    #endregion main variables

    #region monobehaivour methods
    private void Awake()
    {
        rigid = GetComponent<CustomPhysics2D>();
        anim = GetComponent<Animator>();

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
        UpdateVelocity();

        if (Overseer.Instance.GameReady)
        {
            PlayerController opponent = Overseer.Instance.GetNextCharacterByIndex(GetComponent<CharacterStats>().PlayerIndex);
            if (opponent != null)
            {
                FlipSpriteBasedOnOpponentDirection(opponent.CharacterStats.transform);
            }
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
        timeToReachJumpApex = Mathf.Max(.05f, timeToReachJumpApex);
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
        this.verticalInput = (int)Mathf.Sign(verticalInput) * (Mathf.Abs(verticalInput) > PlayerController.INPUT_THRESHOLD_RUNNING ? 1 : 0);
        anim.SetInteger(VERTICAL_INPUT, this.verticalInput);
    }

    /// <summary>
    /// Flips sprite depending on direction of opponent.
    /// If opponent is in the air, should not rotate sprite until grounded
    /// </summary>
    ///   /// <param name="opponentPosition"></param>
    public void FlipSpriteBasedOnOpponentDirection(Transform opponentPosition, bool instant = false)
    {
        if (rigid.isInAir)
        {
            return;
        }
        Vector2 opponentDirection = (opponentPosition.position - transform.position).normalized;
        bool shouldChangeDirection = opponentDirection.x > 0 & !isFacingRight || opponentDirection.x < 0 && isFacingRight || instant;

        if (shouldChangeDirection)
        {
            if (instant)
            {
                if (opponentDirection.x < 0 && isFacingRight)
                {
                    SetSpriteFlipped(false);
                }
                else if (opponentDirection.x > 0 && !isFacingRight)
                {
                    SetSpriteFlipped(true);
                }
            }
            else
            {
                anim.SetBool(ShouldChangeDirection, true);
            }
        }
    }

    public void AnimatorChangeDirection()
    {
        PlayerController opponent = Overseer.Instance.GetNextCharacterByIndex(GetComponent<CharacterStats>().PlayerIndex);
        Vector2 opponentDirection = (opponent.CharacterStats.transform.position - transform.position).normalized;

        if (opponentDirection.x < 0)
        {
            SetSpriteFlipped(false);
        }
        else if (opponentDirection.x > 0)
        {
            SetSpriteFlipped(true);
        }
    }

    public void UpdateVerticalVelocity()
    {
        anim.SetFloat(VERTICAL_SPEED_ANIMATION_PARAMETER, rigid.Velocity.y);
    }

    #endregion

    #region private methods

    /// <summary>
    /// Updates the rigid body to the goal velocity.
    /// </summary>
    private void UpdateVelocity()
    {
        float goalVelocityX = rigid.Velocity.x;
        float goalVelocityY = rigid.Velocity.y;

        if (!rigid.UseAnimatorVelocity)
        {
            if (!rigid.isInAir)
            {
                float directionScale = isFacingRight ? 1 : -1;
                float accelerationFactor = GroundAccelerationScale * GroundAcceleration * directionScale;
                float targetXVelocity = GoalVelocity.x * RunningSpeed * directionScale;
                float newXVelocity = targetXVelocity;
                if (accelerationFactor > 0)
                {
                    newXVelocity = Mathf.MoveTowards(rigid.Velocity.x, targetXVelocity, accelerationFactor * Overseer.DELTA_TIME);
                }
                goalVelocityX = newXVelocity;
            }
        }
        else
        {
            goalVelocityX = GoalVelocity.x * (isFacingRight ? 1 : -1);
            goalVelocityY = GoalVelocity.y;
        }

        if (IsDashing)
        {
            float dashFactor = rigid.isInAir ? AirDashSpeed : GroundDashSpeed;
            goalVelocityX *= dashFactor;
            goalVelocityY *= dashFactor;
        }
      
        Vector2 updatedVelocity = new Vector2(goalVelocityX, goalVelocityY);
        rigid.Velocity = updatedVelocity;
    }

    /// <summary>
    /// Sets whether our sprite is facing left or right
    /// </summary>
    private void SetSpriteFlipped(bool spriteFacingright)
    {

        isFacingRight = spriteFacingright;
        if (spriteFacingright)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = -Mathf.Abs(currentScale.x);
            transform.localScale = currentScale;
        }
        else
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x); ;
            transform.localScale = currentScale;
        }

        if (anim)
        {
            anim.SetInteger(HORIZONTAL_INPUT, (isFacingRight ? 1 : -1) * this.horizontalInput);
            anim.SetBool(ShouldChangeDirection, false);
        }

        OnDirectionChanged?.Invoke(spriteFacingright);
    }

    private void InitializeMovementParameters()
    {
        float gravity = (2 * heightOfJump) / Mathf.Pow(timeToReachJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToReachJumpApex;
        JumpingAcceleration = gravity / CustomPhysics2D.GRAVITY_CONSTANT;
        rigid.gravityScale = JumpingAcceleration;

        maxAvailableJumps = Mathf.Max(maxAvailableJumps, 0);
        MaximumAirSpeed = Mathf.Max(MaximumAirSpeed, 0);
    }

    #endregion

    #region jumping methods
    /// <summary>
    /// Method that performs a jump by applying a velocity in the y direction
    /// 
    /// NOTE: Wondering if these conditions should actually be found in the animator itself
    /// </summary>
    /// <returns></returns>
    public bool Jump()
    {
        if (currentJumpsAvailable <= 0)
        {
            return false;
        }
        anim.SetTrigger(JumpTrigger);
        return true;
    }

    public void AnimatorJump()
    {
        float gravity = (2 * heightOfJump) / Mathf.Pow(timeToReachJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToReachJumpApex;
        JumpingAcceleration = gravity / CustomPhysics2D.GRAVITY_CONSTANT;
        rigid.gravityScale = JumpingAcceleration;

        rigid.Velocity = new Vector2(rigid.Velocity.x, jumpVelocity);
        --currentJumpsAvailable;
    }

    public void AnimatorAirDash()
    {
        anim.SetInteger(DashCount, anim.GetInteger(DashCount) - 1);
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
            rigid.gravityScale = JumpingAcceleration;
        }
        else
        {
            rigid.gravityScale = JumpingAcceleration * fastFallScale;
        }
    }

    /// <summary>
    /// This method will be called any time our character touches the ground after being
    /// in an in-air state
    /// </summary>
    public void OnGroundedEvent()
    {
        anim.SetBool(IN_AIR_ANIMATION_PARAMETER, false);
        anim.SetInteger(DashCount, 1);
        this.currentJumpsAvailable = maxAvailableJumps;

        PlayerController opponent = Overseer.Instance.GetNextCharacterByIndex(GetComponent<CharacterStats>().PlayerIndex);
        if (opponent != null)
        {
            FlipSpriteBasedOnOpponentDirection(opponent.CharacterStats.transform, true);
        }
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
        this.currentJumpsAvailable--;
    }


    private void JoystickDirectionSet(CommandInterpreter.DIRECTION direction, Vector2Int joystickDirectionVec)
    {
        SetHorizontalInput(joystickDirectionVec.x);
        SetVerticalInput(joystickDirectionVec.y);
    }
    #endregion jumping methods

    #region player interaction methods

    /// <summary>
    /// Method for moving characters due to forced movement (guard knockback, hit knockback, etc).
    /// </summary>
    /// <param name="destinationVector"></param>
    public void TranslateForcedMovement(Vector2 startingVector, Vector2 destinationVector, float lerpValue)
    {
        Vector2 newVelocity = Vector2.Lerp(startingVector, destinationVector, lerpValue);
        rigid.Velocity = newVelocity;
        anim.SetFloat(VERTICAL_SPEED_ANIMATION_PARAMETER, rigid.Velocity.y);
        if (rigid.Velocity.y > 0)
        {
            rigid.OnPhysicsObjectAirborne();
        }
    }

    public Vector2 GetVelocity()
    {
        return rigid.Velocity;
    }

    #endregion

}
