using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed = 7f;
    public float sprintSpeed = 11f;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    public float dashSpeed = 20f;
    public float dashSpeedChangeFactor = 50f;

    public float groundDrag;

    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump;


    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Check")]
    public float maxSlopeAngle = 40f;
    private RaycastHit slopeHit;
    private bool exitSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        Walking,
        Sprinting,
        Dashing,
        Air
    }

    public bool isDashing;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (state == MovementState.Walking || state == MovementState.Sprinting)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        if(isDashing)
        {
            state = MovementState.Dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        else if(grounded && Input.GetKey(sprintKey))
        {
           state = MovementState.Sprinting; 
           desiredMoveSpeed = sprintSpeed;
        }
        
        else if(grounded)
        {
            state = MovementState.Walking;
            desiredMoveSpeed = walkSpeed;
        }

        else
        {
            state = MovementState.Air;
            if(desiredMoveSpeed < sprintSpeed)
            {
                desiredMoveSpeed = walkSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

        if(lastState == MovementState.Dashing) { keepMomentum = true; }

        if(desiredMoveSpeedHasChanged)
        {
            if(keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }

            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }
        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private float speedChangeFactor;

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
         float time = 0;
         float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
         float startVal = moveSpeed;

         float boostFactor = speedChangeFactor;

         while(time < difference)
         {
            moveSpeed = Mathf.Lerp(startVal, desiredMoveSpeed, time/difference);
            time += Time.deltaTime * boostFactor;

            yield return null;
         }

         moveSpeed = desiredMoveSpeed;
         speedChangeFactor = 1f;
         keepMomentum = false;
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if(OnSlope() && !exitSlope)
        {
          rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force); 

          if(rb.velocity.y > 0)
          {
            rb.AddForce(Vector3.down * 80f, ForceMode.Force);
          }
        }

        // on ground
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn off gravity
        rb.useGravity = !OnSlope();    
    }

    private void SpeedControl()
    {
        if(OnSlope() && !exitSlope)
        {
           if(rb.velocity.magnitude > moveSpeed)
           {
                rb.velocity = rb.velocity.normalized * moveSpeed;
           } 
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

    }

    private void Jump()
    {
        exitSlope = true;
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position,Vector3.down,out slopeHit, playerHeight * 0.5f +0.3f))
        {
          float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

          return angle < maxSlopeAngle && angle != 0;  
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

}