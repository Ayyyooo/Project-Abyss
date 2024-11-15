using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovementTutorial pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    private bool sliding;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementTutorial>();
        startYScale = playerObj.localScale.y;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        {
            startSlide();
        }
        if (Input.GetKeyUp(slideKey) && sliding)
        {
            stopSlide();
        }

    }

    private void startSlide()
    {
        sliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 0.1f, ForceMode.Impulse);

        slideTimer = maxSlideTime;

    }
    private void FixedUpdate()
    {   
        bool grounded = Physics.Raycast(transform.forward, Vector3.down, pm.playerHeight * 0.2f);
        // Draw a ray representing the sliding force direction and magnitude

        if (sliding && grounded)

            slidingMovement();
    }

    private void slidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.forward * horizontalInput;
        

        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            // Normal sliding on flat ground
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
           
        }

        if (slideTimer <= 0)
        {
            stopSlide();
        }
    }


    private void stopSlide()
    {
        sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);

    }

}
