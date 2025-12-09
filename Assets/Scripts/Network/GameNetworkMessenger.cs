using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkMessenger : NetworkBehaviour
{
    public static GameNetworkMessenger Instance { get; private set; }
    public event Action<string> OnMessageReceived;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void SendAction(string jsonPayload)
    {
        if (IsServer)
        {
            OnMessageReceived?.Invoke(jsonPayload);

            BroadcastJsonClientRpc(jsonPayload);
        }
        else
        {
            SendJsonServerRpc(jsonPayload);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendJsonServerRpc(string json)
    {
        OnMessageReceived?.Invoke(json);
        BroadcastJsonClientRpc(json);
    }

    [ClientRpc]
    private void BroadcastJsonClientRpc(string json)
    {
        if (!IsServer)
        {
            OnMessageReceived?.Invoke(json);
        }
    }

   
}