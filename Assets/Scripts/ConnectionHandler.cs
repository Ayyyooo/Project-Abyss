using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using UnityEditor.Rendering;
using UnityEditor.PackageManager;
using FishNet.Transporting;

public enum ConnectionType {
    Host, Client
}

public class ConnectionHandler : MonoBehaviour
{
    public ConnectionType ConnectionType;
#if UNITY_EDITOR
    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;

    }
    private void OnDisable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args) {
        if (args.ConnectionState == LocalConnectionState.Stopping) { 
            UnityEditor.EditorApplication.isPlaying = false;
        }
    
    }
#endif
    private void onStart() {
        #if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
        {
            InstanceFinder.ClientManager.StartConnection();
        }
        else {
            if (ConnectionType == ConnectionType.Host)
            {
                InstanceFinder.ServerManager.StartConnection();
                InstanceFinder.ClientManager.StartConnection();
            }
            else {
                InstanceFinder.ClientManager.StartConnection();
            }
          
        }
        #endif
        #if DEDICATED_SERVER
        InstanceFinder.ServerManager.StartConnection();
        #endif
    }



}
