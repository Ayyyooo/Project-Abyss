using UnityEngine;
using UnityEngine.AI;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing;
using System.Collections;


public class Bot1Controller : NetworkBehaviour
{
    private NavMeshAgent Agent;
    private Transform ClosestPlayer;
    private NetworkManager NetworkManager;
    private Animator Animator;
    private NavMeshObstacle Obstacle;

    [SerializeField] private float StoppingDistance = 1.5f;
    [SerializeField] private float RotationSpeed = 2.0f;
    [SerializeField] private float AttackRange = 2.0f;
    [SerializeField] private float AttackCooldown = 2.0f;
    [SerializeField] private int MaxHealth = 100;

    private int CurrentHealth;
    private float LastAttackTime;
    private bool IsDead = false;
    private float DistanceToPlayer;

    private enum BotState { Idle, Chasing, Dead }
    private BotState CurrentState = BotState.Idle;

    private void Awake()
    {
        NetworkManager = FindAnyObjectByType<NetworkManager>();
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        Obstacle = GetComponent<NavMeshObstacle>();
        Animator.applyRootMotion = false;
        Obstacle.enabled = false;
        Agent.updatePosition = true;
        Agent.updateRotation = true;
        CurrentHealth = MaxHealth;
    }

    void Start()
    {
        InvokeRepeating("getClosestPlayer", 0f, 1.5f);
    }

    void FixedUpdate()
    {
        if (IsDead) return;
        if (ClosestPlayer != null)
        {
            DistanceToPlayer = Vector3.Distance(transform.position, ClosestPlayer.position);
        }
        else {
            DistanceToPlayer = 0;
        }

        IdleOrChasing();

        switch (CurrentState)
        {
            case BotState.Chasing:
                ChasePlayer();
                break;
            case BotState.Idle:
                Idle();
                break;
        }
        if (CanAttackPlayer())
        {
            AttackPlayer();
        }
    }

    private void getClosestPlayer()
    {
        if (IsDead) return;

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

        if (ClosestPlayer != null)
        {
            CurrentState = BotState.Chasing;
        }
        else
        {
            CurrentState = BotState.Idle;
        }
    }

    private void IdleOrChasing() {
        if (IsDead) return;
        if (DistanceToPlayer <= StoppingDistance)
        {
            StopAgent(true);
            CurrentState = BotState.Idle;
        }
        else
        {
            StopAgent(false);
            CurrentState = BotState.Chasing;
        }

    }

    private void Idle()
    {
        //Rotates to face the player
        Vector3 directionToPlayer = (ClosestPlayer.position - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToPlayer, transform.up), RotationSpeed * Time.deltaTime);
    }

    private void ChasePlayer()
    {
        if (ClosestPlayer == null) return;

        if (Agent.enabled)
        {
            Agent.SetDestination(ClosestPlayer.position);
        }
    }

    private bool CanAttackPlayer()
    {
        if (ClosestPlayer == null || IsDead) return false;

        return DistanceToPlayer <= AttackRange && Time.time - LastAttackTime > AttackCooldown;
    }

    private void AttackPlayer()
    {
        LastAttackTime = Time.time;
        Animator.SetTrigger("Attack");

        //damage to the player here
        Debug.Log("Attacked Player!");
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        CurrentState = BotState.Dead;
        StopAgent(true);
            
        Animator.SetTrigger("Die");
        

        // Add a delay before destroying the bot
        Destroy(gameObject, 5f);
    }

    private void StopAgent(bool stopAgent)
    {
        if (stopAgent) {
            if(Agent.enabled)
            {
                //sets agent velocity to zero, disables agent enables obstacle
                Agent.velocity = Vector3.zero;
                StartCoroutine(DisableAgent());
            }
        }
        else { 
            if(!Agent.enabled)
            {
                //disables obstacle enables agent
                StartCoroutine(EnableAgent());
            }
        }
    }

    private IEnumerator EnableAgent()
    {
        Obstacle.enabled = false;
        Agent.Warp(transform.position);
        yield return null;

        if (!Obstacle.enabled)
            Agent.enabled = true;
    }

    private IEnumerator DisableAgent()
    {
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
