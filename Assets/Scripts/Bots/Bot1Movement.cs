using UnityEngine;
using UnityEngine.AI;
using FishNet.Managing;
using FishNet.Connection;
using FishNet.Object;
using UnityEditor.Rendering;
using Unity.VisualScripting;
public class NewMonoBehaviourScript : MonoBehaviour
{
    
    NavMeshAgent agent;
    private NetworkManager networkManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.destination = getClosestPlayer();    
    }

    Vector3 getClosestPlayer() {
        float minDistance = float.PositiveInfinity;
        Vector3 closestPlayerPosition = transform.position;
        foreach (var kvp in networkManager.ServerManager.Clients)
        {
            // kvp is a KeyValuePair, so we access the value, which is the NetworkConnection
            NetworkConnection conn = kvp.Value;

            // Get the NetworkObject associated with this connection (the player)
            NetworkObject playerNetworkObject = conn.FirstObject;

            if (playerNetworkObject != null)
            {
                // Access the player's Transform component
                Transform playerTransform = playerNetworkObject.transform;

                float distance = Vector3.Distance(playerTransform.position, transform.position);

                if (distance < minDistance) {
                    closestPlayerPosition = playerTransform.position;
                    minDistance = distance;
                }
                Debug.Log("Player ID: " + conn.ClientId + ", Position: " + playerTransform.position);
            }
        }
        // Return the closest player's position or the bot's position if no players are found
        return closestPlayerPosition;
    }
}
