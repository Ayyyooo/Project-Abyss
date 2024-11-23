using System.Threading;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;

    public float maxWallRunTime;
    private float wallRunTimer;


    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;
    [Header("References")]
    public Transform orientation;
    private PlayerMovementTutorial pm;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementTutorial>();

    }

    private void Update()
    {
        CheckForWall();
        StateMachine();

    }

    private void FixedUpdate()
    {
        if (pm.wallRunning)
        {
            wallRunMovement();
        }
        stopWallRun();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);

    }

    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (wallLeft || wallRight && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!pm.wallRunning){
                startWallRun();
                if (verticalInput <= 0){
                gravityCounterForce = 10;
                stopWallRun();
                
                }
            }
            

            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 && pm.wallRunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }


            if (Input.GetKeyDown(jumpKey) &&(horizontalInput !=0 ||verticalInput !=0)) wallJump();
        }

        else if (exitingWall)
        {
            if (pm.wallRunning)
                stopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0){
                exitingWall = false;
            }
        }


        else
        {
            if (pm.wallRunning)
                stopWallRun();
        }

    }
    private void startWallRun()
    {
        pm.wallRunning = true;

        wallRunTimer = maxWallRunTime;

        Debug.Log("Wallrunning");

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

    }

    private void wallRunMovement()
    {
        rb.useGravity = useGravity;


        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal, ForceMode.Force);

        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void stopWallRun()
    {
        pm.wallRunning = false;
        Debug.Log("Wall running stopped");

    }

    private void wallJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;


        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

    }


}
