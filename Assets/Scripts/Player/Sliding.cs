using FishNet.Object;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovementTutorial pm;
    private CapsuleCollider playerCollider;
    private Animator animator;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    public float slideYScale; // Y scale for sliding
    private float slideTimer;
    private float startYScale;
    private float startColliderHeight;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;
    private bool sliding;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementTutorial>();
        playerCollider = GetComponent<CapsuleCollider>();

        startYScale = playerObj.localScale.y;
        startColliderHeight = playerCollider.height;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        {
            StartSlide();
        }
        if (Input.GetKeyUp(slideKey) && sliding)
        {
            StopSlide();
        }

        else if(Input.GetKey(slideKey)){
             animator.SetFloat("Speed", 0.7f);

        }
    }

    private void FixedUpdate()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, pm.playerHeight * 0.2f);
        
        if (sliding && grounded)
        {
            SlidingMovement();
        }
    }

    private void StartSlide()
    {
        sliding = true;

        // Reduce player scale and collider height
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); // Adjusted down force
        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        sliding = false;

        // Reset player scale and collider height
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
        playerCollider.height = startColliderHeight;
    }
}
