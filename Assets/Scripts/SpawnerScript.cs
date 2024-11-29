using FishNet.Object;  // Import FishNet for networking
using UnityEngine;

public class SpawnerScript : NetworkBehaviour
{
    public GameObject Enemy;        // The prefab to spawn (must be a networked prefab)
    public Rigidbody player;               // The player Rigidbody
    public float spawnRange = 10f;         // Range within which spawning starts
    public float spawnInterval = 2f;       // Time interval between spawns

    private float spawnTimer = 0f;         // Tracks time between spawns
    private bool isPlayerInRange = false;  // Tracks if the player is in range

    void Update()
    {
        // Only run this logic on the server
        if (!IsServerInitialized) return;

        // Check the distance between the player and the spawner
        float distance = Vector3.Distance(player.position, transform.position);

        // Determine if the player is within the specified range
        isPlayerInRange = distance <= spawnRange;

        if (isPlayerInRange)
        {
            // Update the spawn timer
            spawnTimer += Time.deltaTime;

            // Spawn the object if the timer exceeds the spawn interval
            if (spawnTimer >= spawnInterval)
            {
                // Call the spawning function on the server
                SpawnObjectServerRpc();
                spawnTimer = 0f;  // Reset the spawn timer
            }
        }
        else
        {
            spawnTimer = 0f;  // Reset timer if player is out of range
        }
    }

    [ServerRpc]
    private void SpawnObjectServerRpc()
    {
        // Instantiate the object and spawn it on the network
        GameObject spawnedSphere = Instantiate(Enemy, transform.position, Quaternion.identity);
        Spawn(spawnedSphere);  // FishNet's method to spawn objects over the network
    }
}
