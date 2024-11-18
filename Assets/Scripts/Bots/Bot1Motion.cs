using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Bot1Motion : MonoBehaviour
{

    private NavMeshAgent Agent;
    private Transform ClosestPlayer;
    private NetworkManager NetworkManager;
    private Animator Animator;
    private NavMeshObstacle Obstacle;

    private Vector2 Velocity;
    private Vector2 SmoothDeltaPosition;

    [SerializeField]
    private float StoppingDistance = 1.5f;

    private void Awake()
    {
        NetworkManager = FindAnyObjectByType<NetworkManager>();
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        Obstacle = GetComponent<NavMeshObstacle>();
        Animator.applyRootMotion = false;
        Agent.updatePosition = true;
        Obstacle.enabled = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("getClosestPlayer", 0f, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        //  Debug.Log($"Agent {Agent.enabled}, obstacle {Obstacle.enabled}");

        if (ClosestPlayer == null) return;
        StopAgent();
        if (Agent.enabled)
        {
            Agent.SetDestination(ClosestPlayer.position);
        } else{
            Animator.SetBool("move",  false);
        }
        
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

    private void StopAgent() {

        float distance = Vector3.Distance(transform.position,ClosestPlayer.position);
        
        if (distance > StoppingDistance) {
            
            if (!Agent.enabled) { 
                StartCoroutine(EnableAgent());
            }
            return;
        }
        if (Agent.enabled)
        {
            
            Agent.velocity = Vector3.zero;
            StartCoroutine(DisableAgent());
        }
    }

    private IEnumerator EnableAgent()
    {
        Obstacle.enabled = false;

        Agent.Warp(transform.position);

        yield return null;
        if(!Obstacle.enabled)
        Agent.enabled = true;
    }


    private IEnumerator DisableAgent() {
        Agent.enabled = false;
        yield return null;
        Obstacle.enabled = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Vector3 lastCorner = transform.position;
        if (Agent != null && Agent.enabled && ClosestPlayer)
        {
            foreach (var corner in Agent.path.corners)
            {
                Gizmos.DrawLine(lastCorner, corner);
                Gizmos.DrawSphere(corner, 0.2f);
                lastCorner = corner;

            }
        }
    }

}
