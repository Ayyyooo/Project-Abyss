using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet.Object;
using UnityEngine.InputSystem;

public class PlayerMovementTutorial : NetworkBehaviour
{
    [Header("Movement")]

    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float wallRunSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchkey = KeyCode.LeftControl;
    public KeyCode shoot = KeyCode.Mouse0;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask Default;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;



    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    private Animator animator;

    Rigidbody rb;


    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallRunning,
        crouching,
        air
    }

    public bool wallRunning;

    private void Start()
    {   
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.2f);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        if(moveDirection == Vector3.zero){
            animator.SetFloat("Speed",0);
        }

        else if(!Input.GetKey(sprintKey)){
            animator.SetFloat("Speed", 0.5f);
        }
        else if(Input.GetKey(sprintKey)){

            animator.SetFloat("Speed", 0.7f);
        }
        else if(Input.GetKeyDown(KeyCode.R)){
            animator.SetBool("Reloading", true);
        }
        else{
            animator.SetBool("Reloading", false);

        }

    }

    private void FixedUpdate()
    {
        if (!grounded)
        {
            // Apply custom gravity manually
            rb.AddForce(new Vector3(0, -20f, 0), ForceMode.Acceleration);
        }
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchkey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(crouchkey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        if(wallRunning){
            state = MovementState.wallRunning;
            moveSpeed = wallRunSpeed;
        }
        if (Input.GetKey(crouchkey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;

        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            //Debug.Log("Walking");
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        //if (!IsOwner) return;
        if(verticalInput > 0 || horizontalInput > 0){
            //Debug.Log("Vertical input:" + verticalInput + "Horizontal Input: " + horizontalInput);
        }

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 70f, ForceMode.Force);
            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 900f, ForceMode.Force);
            }
        }

        // on ground
        else if (grounded){
            //Debug.Log("Grounded");
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if(!wallRunning) rb.useGravity = !OnSlope();

        

    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

    }

    private void Jump()
    {
        exitingSlope = true;
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}