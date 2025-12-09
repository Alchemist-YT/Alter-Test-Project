using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Cysharp.Threading.Tasks;
using Unity.Networking.Transport.Relay;

public class RelayNetworkManager : MonoBehaviourSingleton<RelayNetworkManager>
{

    [Header("Players")]
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


    public async UniTask StartClientWithRelay(string joinCode)
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
    }
}
