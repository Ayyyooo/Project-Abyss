using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class LegMovement : MonoBehaviour
{
    [SerializeField] Transform[] Targets = new Transform[6];
    [SerializeField] Transform[] NextTargets = new Transform[6];
    
    [SerializeField] Transform Head;
    [SerializeField] Transform Body;
    [SerializeField] Transform Parent;
    [SerializeField] Transform ClosestPointOrigin; 
    [SerializeField] Transform WallDirection; 
    [SerializeField] LayerMask GroundLayer;
    [SerializeField] float LegMaxLength = 1.8f; //Maximum distance for raycast
    [SerializeField] float CheckWallDistance = 7f; //NOT IN USE
    [SerializeField] float WallAngle = -0.5f;
    [SerializeField] float RotationToWallFactor = 1f;
    [SerializeField] float MaxDistanceToWall = 2f;
    [SerializeField] float StepDistance = 0.75f; //Maximum distance between target and next target
    [SerializeField] int FramesForStep = 25; // Frames for each step
    [SerializeField] float MaxStepHeight = 0.5f; //Maximum height reached in a step
    [SerializeField] float targetOffset = 0.5f; //Difference from target to next step
    [SerializeField] float MaxTargetHeight = 1.5f; // Maximum height for thr raycast
    [SerializeField] float WaitingTime = 0.01f; //Waited time to move a different group of legs
    [SerializeField] float HeadMovementFactor = 0.02f; //
    [SerializeField] float MinHeadMovement = -0.0075f;
    [SerializeField] float MaxHeadMovement = 0.0065f;

    Vector3[] TargetGroundPosition; // Keeps the ground position
    Transform[] NextTargetOrigins; // Keeps the raycast origins

    // Walking variables
    bool IsMoving = false; // True if legs are moving
    float distance = 0;
    bool RotateToWall = false;
    float OriginalDistanceToWall = 0;
    Vector3 Normal;
    Vector3 WallNormal;

    // Define tripod gait groups
    readonly int[] GroupA = { 0, 2, 4 }; // Indices for Set A
    readonly int[] GroupB = { 1, 3, 5 }; // Indices for Set B
    bool MoveGroupA = true; // Indicates which group is currently moving

    NavMeshAgent Agent;

    // Gizmos 
    private Color gizmoColor = new Color(0f, 1f, 0f, 1f);

    private void Awake()
    {
        Agent = GetComponentInParent<NavMeshAgent>();
        NextTargetOrigins = new Transform[NextTargets.Length];
        TargetGroundPosition = new Vector3[Targets.Length];
        
        for (int i = 0; i < Targets.Length; i++)
        {
            TargetGroundPosition[i] = Targets[i].position;

            // Initializes the origin for the raycast
            NextTargetOrigins[i] = NextTargets[i].parent;
            NextTargetOrigins[i].position = Targets[i].position + (Body.up*MaxTargetHeight) + (Body.forward * targetOffset);
            
            
        }
    }

    private void FixedUpdate()
    {
        AdjustBodyAngle();   
        AdjustHeadHeight();
        StickLegs();
        GroundNextTarget();
        Debug.DrawRay(Body.position, Body.up*3, Color.yellow);
        //Debug.DrawRay(Body.position, Body.forward*3, Color.blue);
        //Debug.DrawRay(Body.position, Body.right*3, Color.red);

        // Calculate distance for the current moving group
        int[] currentGroup = MoveGroupA ? GroupA : GroupB;
        distance = Vector3.Distance(Targets[currentGroup[0]].position, NextTargets[currentGroup[0]].position);

        if (distance > StepDistance && !IsMoving)
        {
            StartCoroutine(MoveLegs(currentGroup));
        }
        

    }

    //public void OnTriggerEnter(Collider other)
    //{
    //    //  Get the true closest point on the collider's surface
    //    if (RotateToWall) return;
    //    Vector3 ClosestPoint = other.ClosestPoint(ClosestPointOrigin.position);
        
    //    setWallNormal(ClosestPoint);
    //}

    private void setWallNormal(Vector3 position) {
      
        Vector3 directionToClosestPoint = position - ClosestPointOrigin.position;
        
        //  Perform a raycast to get the normal and original distance
        RaycastHit hit;
        Debug.DrawRay(ClosestPointOrigin.position, directionToClosestPoint.normalized * MaxDistanceToWall, Color.magenta);
        if (Physics.Raycast(ClosestPointOrigin.position, directionToClosestPoint.normalized, out hit, MaxDistanceToWall, GroundLayer))
        {
            //Compares the angle between the normal and the direction of the ray, checks if the normal is not equal to the agent up vector
            if (Vector3.Dot(hit.normal, directionToClosestPoint.normalized) <= WallAngle && Parent.up != hit.normal)
            {
                // sets the original distance to the wall
                OriginalDistanceToWall = directionToClosestPoint.magnitude;
                // updates the wall direction
                WallDirection.position = position;
                RotateToWall = true;
                WallNormal = hit.normal;
            }

        }

    }

    private float DistanceToWall()
    {
        RaycastHit hit;
        
        Vector3 directionToWall = (WallDirection.position - ClosestPointOrigin.position).normalized;

        Debug.DrawRay(ClosestPointOrigin.position, directionToWall * OriginalDistanceToWall, Color.magenta);

        if (Physics.Raycast(ClosestPointOrigin.position, directionToWall, out hit, MaxDistanceToWall, GroundLayer))
        {
            WallNormal = hit.normal;
            return hit.distance;
        }

        RotateToWall = false;
        return OriginalDistanceToWall;
    }




    IEnumerator MoveLegs(int[] movingGroup)
    {
        IsMoving = true;
        
        // Smoothly move the legs in the current group
        for (int frame = 0; frame < FramesForStep; frame++)
        {
            foreach (int i in movingGroup)
            {
                float t = (float)frame / FramesForStep;
                Targets[i].position = Vector3.Lerp(TargetGroundPosition[i], NextTargets[i].position, t) + Vector3.up * (float)Math.Sin(Math.PI * t) * MaxStepHeight; //Horizontal + Vertical positions
            }
            //Here
            yield return null;
        }

        // Update the ground position for the moved group
        foreach (int i in movingGroup)
        {
            TargetGroundPosition[i] = NextTargets[i].position;
        }

        yield return new WaitForSeconds(WaitingTime);

        // Switch to the other group
        MoveGroupA = !MoveGroupA;
        IsMoving = false;
    }

    void AdjustBodyAngle()
    {
       
        //Calculate the legs vectors
        Vector3 v1 = Targets[GroupA[0]].position - Targets[GroupB[2]].position;
        Vector3 v2 = Targets[GroupA[1]].position - Targets[GroupB[1]].position;
        Vector3 v3 = Targets[GroupA[2]].position - Targets[GroupB[0]].position;

        Vector3 u = v2 - v1;
        Vector3 w = v3 - v1;

        Normal = Vector3.Cross(w, u).normalized;
        if (RotateToWall) {
           float distanceToWall = DistanceToWall();
           Normal = Vector3.Slerp(Normal, WallNormal, Mathf.Clamp01(1- (distanceToWall/OriginalDistanceToWall))).normalized;
           
            //print(Vector3.Dot(Normal, WallNormal));
            if (Vector3.Dot(Normal, WallNormal) >= RotationToWallFactor) RotateToWall = false;
           //Debug.Log($"Distance wall: {distanceToWall} , {OriginalDistanceToWall}");
        }

        Vector3 forwardDirection = Vector3.ProjectOnPlane(Parent.forward, Normal).normalized;


        Body.rotation = Quaternion.LookRotation(forwardDirection, Normal);  
       // Debug.DrawRay(Body.position, normal * 3, Color.magenta);
    }

    void AdjustHeadHeight() {
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        // Find the minimum and maximum Y positions of all leg targets
        foreach (Transform leg in Targets)
        {
            float legHeight = leg.localPosition.y;
            if (legHeight < minHeight) minHeight = legHeight;
            if (legHeight > maxHeight) maxHeight = legHeight;
        }


        // Calculate the average or center height based on leg positions
        float averageHeight = (minHeight + maxHeight) / 2;
        
        // negative value and z axis modified bc head is rotated 90 deg in x axis
        averageHeight = Math.Clamp(-(averageHeight * HeadMovementFactor), MinHeadMovement, MaxHeadMovement);
        Head.localPosition = new Vector3(Head.localPosition.x, Head.localPosition.y, averageHeight);
    }

    void StickLegs()
    {
        if (distance < StepDistance && !IsMoving)
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                Targets[i].position = TargetGroundPosition[i];
            }
        }
        else { 
            // Stick the stationary group to the ground
            int[] stationaryGroup = MoveGroupA ? GroupB : GroupA;
            foreach (int i in stationaryGroup)
            {
                Targets[i].position = TargetGroundPosition[i];
            }
        }
    }

    void GroundNextTarget()
    {
        for (int i = 0; i < NextTargets.Length; i++)
        {
            RaycastHit raycastHit;

            // Calculate the direction toward the body and normalize it
            Vector3 directionToBody = (Body.position - NextTargetOrigins[i].position).normalized;

            // Calculate the midpoint direction between -Body.up and directionToBody
            Vector3 midpointDirection = ((-Body.up + directionToBody) * 0.5f).normalized;

            // 1. Primary raycast: Downward
            Debug.DrawRay(NextTargetOrigins[i].position, -Body.up * LegMaxLength, Color.green);
            if (Physics.Raycast(NextTargetOrigins[i].position, -Body.up, out raycastHit, LegMaxLength, GroundLayer))
            {
                NextTargets[i].position = raycastHit.point;
            }
            // 2. Secondary raycast: Midpoint direction
            else if (Physics.Raycast(NextTargetOrigins[i].position, midpointDirection, out raycastHit, (LegMaxLength*1.5f), GroundLayer))
            {
               // Debug.DrawRay(NextTargetOrigins[i].position, midpointDirection * LegMaxLength, Color.yellow);
                NextTargets[i].position = raycastHit.point;
            }
            //// 3. Tertiary raycast: Toward the body
            //else if (Physics.Raycast(NextTargetOrigins[i].position, directionToBody, out raycastHit, (LegMaxLength*2), GroundLayer))
            //{
            //   // Debug.DrawRay(NextTargetOrigins[i].position, directionToBody * LegMaxLength, Color.red);
            //    NextTargets[i].position = raycastHit.point;
            //}
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        foreach (Transform nextTarget in NextTargets)
        {
            Gizmos.DrawCube(nextTarget.position, Vector3.one * 0.2f);
        }
        Gizmos.DrawCube(WallDirection.position, Vector3.one * 0.2f);
    }
}


