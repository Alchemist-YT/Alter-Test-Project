using Cysharp.Threading.Tasks;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayNetworkManager : MonoBehaviourSingleton<RelayNetworkManager>
{
    public event Action<string> OnRelayError;
    [SerializeField] private int maxConnections = 1;

    private async UniTask InitializeServices()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
            return;

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async UniTask<string> StartHostWithRelay()
    {
        try
        {
            await InitializeServices();

            Allocation allocation =
                await RelayService.Instance.CreateAllocationAsync(maxConnections);

            string joinCode =
                await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(
                new RelayServerData(allocation, "dtls")
            );

            NetworkManager.Singleton.StartHost();

            Debug.Log("Host started | Join Code: " + joinCode);
            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError("Relay Host Error: " + e.Message);
            OnRelayError?.Invoke(e.Message);
            return null;
        }
    }

    public async UniTask<bool> StartClientWithRelay(string joinCode)
    {
        try
        {
            await InitializeServices();

            JoinAllocation joinAllocation =
                await RelayService.Instance.JoinAllocationAsync(joinCode);


            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(
                new RelayServerData(joinAllocation, "dtls")
            );

            NetworkManager.Singleton.StartClient();
            Debug.Log("Client connected");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Relay Client Error: " + e.Message);
            OnRelayError?.Invoke(e.Message);
            return false;
        }

    }
}
