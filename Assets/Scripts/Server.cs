using FishNet.Managing;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public NetworkManager networkManager;

    private void Start()
    {
        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<NetworkManager>();
        }
    }

    public void StartServer()
    {
        networkManager.ServerManager.StartConnection();
        Debug.Log("Server started");
    }

    public void StopServer()
    {
        networkManager.ServerManager.StopConnection(true);
        Debug.Log("Server stopped");
    }

    public void SearchForServers()
    {
        // Code to search for available servers on the network
        Debug.Log("Searching for servers...");
        // Implement specific discovery logic if using network discovery
    }
}


