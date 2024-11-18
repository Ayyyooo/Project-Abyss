using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using GameKit.Dependencies.Utilities.Types;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class NewMonoBehaviourScript : NetworkBehaviour
{
    private NavMeshAgent Agent;
    private Transform ClosestPlayer;
    private NetworkManager NetworkManager;
    private Animator Animator;
    private int SpeedHash;
    private bool BlockedAgent;
    private Vector3 currentWaypoint;
    private float StoppingToWaypoint;
    private float StoppingToPlayer;

    private void Awake()
    {
        NetworkManager = FindAnyObjectByType<NetworkManager>();
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        SpeedHash = Animator.StringToHash("Speed");
        BlockedAgent = false;
        StoppingToWaypoint = 0.1f;
        StoppingToPlayer = 1.7f;
    }

    void Start()
    {
        InvokeRepeating("getClosestPlayer", 0f, 1.5f);
    }

    void Update()
    {
        if (ClosestPlayer == null) return;

        // Checks if the player is in range 
        if (DistanceToPlayer() < 1.5f)
        {
            PlayerInRange();
        }
        // Resumes the movement
        else Agent.isStopped = false;

        // Check for potential blocks and handle waypoint adjustments
        blockedByAgent();

        // Check if a waypoint is set and if the agent has reached it
        if (currentWaypoint != Vector3.zero && Agent.remainingDistance < StoppingToWaypoint)
        {
            currentWaypoint = Vector3.zero;
            BlockedAgent = false;
            Agent.SetDestination(ClosestPlayer.position);
        }
        // Set destination to the closest player if no waypoint is active
        else if (currentWaypoint == Vector3.zero)
        {
            Agent.SetDestination(ClosestPlayer.position);
        }

        // Adjust animation speed based on movement velocity
        float velocity = Agent.velocity.magnitude;
        Animator.speed = Mathf.Max(velocity / 3, 0.1f);
        Animator.SetFloat(SpeedHash, velocity);
    }

    private void getClosestPlayer()
    {
        float minDistance = float.PositiveInfinity;
        foreach (var kvp in NetworkManager.ServerManager.Clients)
        {
            NetworkConnection conn = kvp.Value;
            NetworkObject playerNetworkObject = conn.FirstObject;

            if (playerNetworkObject != null)
            {
                Transform playerTransform = playerNetworkObject.transform;
                float distance = Vector3.Distance(playerTransform.position, transform.position);

                if (distance < minDistance)
                {
                    ClosestPlayer = playerTransform;
                    minDistance = distance;
                }
            }
        }
    }

    private void blockedByAgent()
    {
        if (Agent.path.corners.Length > 1)
        {
            float rayLength = 1.5f;
            float angleOffset = 20f;
            int numberOfRays = 4;
            float sampleRadius = 1.0f;

            Vector3 initialDirection = Agent.path.corners[1] - transform.position;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

            // Cast a ray in the initial direction
            float initialHitDistance = RayCollision(initialDirection.normalized, rayOrigin, rayLength);

            if (initialHitDistance == -1)
            {
                BlockedAgent = false;
                Agent.speed = 3.5f; // Reset to default max speed if unblocked
                return;
            }

            Agent.velocity = Vector3.zero;

            // Scale speed based on how close the obstacle is
            float speedFactor = Mathf.Clamp(initialHitDistance / rayLength, 0.1f, 1f);
            Agent.speed = 3.5f * speedFactor;

            if (!BlockedAgent)
            {
                BlockedAgent = true;

                // Check rays at various angles
                for (int i = 1; i <= numberOfRays; i++)
                {
                    float angle = angleOffset * i;
                    Vector3[] directions = {
                    Quaternion.Euler(0, angle, 0) * initialDirection.normalized,
                    Quaternion.Euler(0, -angle, 0) * initialDirection.normalized
                };

                    foreach (Vector3 direction in directions)
                    {
                        float hitDistance = RayCollision(direction, rayOrigin, rayLength);

                        if (hitDistance == -1)
                        {
                            Vector3 potentialWaypoint = rayOrigin + direction * rayLength;

                            if (NavMesh.SamplePosition(potentialWaypoint, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                            {
                                currentWaypoint = hit.position;
                                Agent.SetDestination(currentWaypoint);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    private float RayCollision(Vector3 direction, Vector3 rayOrigin, float rayLength)
    {
        Debug.DrawRay(rayOrigin, direction * rayLength, Color.red, 0.25f);
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, direction, out hit, rayLength))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NavmeshAgent"))
            {
                NavMeshAgent hitAgent = hit.collider.GetComponentInParent<NavMeshAgent>();
                if (hitAgent != null && hitAgent.velocity.magnitude < 0.1f)
                {
                    return hit.distance;
                }
            }
        }

        return -1; // No hit or irrelevant hit
    }


    private float DistanceToPlayer() {
        if (ClosestPlayer) {
            return Vector3.Distance(transform.position, ClosestPlayer.position);

        }
        return 0;
    }

    private void PlayerInRange() {
        // Stops the player
        Agent.velocity = Vector3.zero;
        Agent.isStopped = true;
        //Stop animation and hit handler

    }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Vector3 lastCorner = transform.position;
        if (Agent.enabled) { 
        foreach (var corner in Agent.path.corners)
        {
            Gizmos.DrawLine(lastCorner, corner);
            Gizmos.DrawSphere(corner, 0.2f);
            lastCorner = corner;

        }
        }
        //if (agent != null && agent.path.corners.Length > 1)
        //{

        //    Vector3 direction = agent.path.corners[1] - transform.position;

        //    // Set Gizmo color
        //    Gizmos.color = Color.red;

        //    // Draw the ray in the Scene view to visualize the raycast
        //    Gizmos.DrawRay(transform.position + Vector3.up, direction.normalized * 3f); // Length of the ray is 3 units
        //}
    }


}


