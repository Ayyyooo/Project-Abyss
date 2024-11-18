using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class LegMovement : MonoBehaviour
{
    [SerializeField] Transform[] Targets = new Transform[6];
    [SerializeField] Transform[] NextTargets = new Transform[6];
    [SerializeField] Transform Head;
    [SerializeField] LayerMask GroundLayer;

    [SerializeField] float LegMaxLength = 2f;
    [SerializeField] float StepDistance = 0.6f;
    [SerializeField] int FramesForStep = 20;
    [SerializeField] float MaxStepHeight = 0.5f;
    [SerializeField] float targetOffset = 0.5f;
    [SerializeField] float WaitingTime = 0.01f;
    [SerializeField] float HeadOffset = 1.5f;

    Vector3[] TargetGroundPosition; // Keeps the ground position
    Transform[] NextTargetOrigins; // Keeps the raycast origins

    NavMeshAgent navMeshAgent;

    // Walking variables
    bool IsMoving = false; // True if legs are moving
    float distance = 0;

    // Define tripod gait groups
    readonly int[] GroupA = { 0, 2, 4 }; // Indices for Set A
    readonly int[] GroupB = { 1, 3, 5 }; // Indices for Set B
    bool MoveGroupA = true; // Indicates which group is currently moving

    // Gizmos 
    private Color gizmoColor = new Color(0f, 1f, 0f, 1f);

    private void Awake()
    {
        navMeshAgent = GetComponentInParent<NavMeshAgent>();
        NextTargetOrigins = new Transform[NextTargets.Length];
        TargetGroundPosition = new Vector3[Targets.Length];

        for (int i = 0; i < Targets.Length; i++)
        {
            TargetGroundPosition[i] = Targets[i].position;

            // Initializes the origin for the raycast
            NextTargetOrigins[i] = NextTargets[i].parent;
            NextTargetOrigins[i].position = Targets[i].position + Vector3.up + (Vector3.forward * targetOffset);
            
            
        }
    }

    private void FixedUpdate()
    {
        StickLegs();
        GroundNextTarget();
        setHead();

        // Calculate distance for the current moving group
        int[] currentGroup = MoveGroupA ? GroupA : GroupB;
        distance = Vector3.Distance(Targets[currentGroup[0]].position, NextTargets[currentGroup[0]].position);

        if (distance > StepDistance && !IsMoving)
        {
            StartCoroutine(MoveLegs(currentGroup));
        }
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

    void setHead() {
        float averagePosition = 0;
        foreach (Transform t in Targets) {
            averagePosition += t.position.y;    
        }
        averagePosition /= Targets.Length*2;
        Head.position = new Vector3(Head.position.x,averagePosition+HeadOffset,Head.position.z);
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
            Debug.DrawRay(NextTargetOrigins[i].position, Vector3.down * LegMaxLength, Color.green);

            if (Physics.Raycast(NextTargetOrigins[i].position, Vector3.down, out raycastHit, LegMaxLength, GroundLayer))
            {
                NextTargets[i].position = raycastHit.point;
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        foreach (Transform nextTarget in NextTargets)
        {
            Gizmos.DrawCube(nextTarget.position, Vector3.one * 0.2f);
        }
    }
}


