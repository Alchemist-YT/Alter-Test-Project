using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class ConnectionScreenUI : BaseScreen
{
    [SerializeField] TextMeshProUGUI debugText;
    [SerializeField] TMP_InputField joinCodeIF;
    [SerializeField] TextMeshProUGUI roomCode;
    [SerializeField] Button hostButton, ClientButton;

    void Start()
    {
        Show();
    }

    public void StartHost()
    {
        /* Debug.Log("Starting Host...");
         bool success = NetworkManager.Singleton.StartHost();
         if (success)
         {
             Debug.Log("Host started successfully.");
             debugText.text = "Host started successfully.";
             screen.SetActive(false);
         }
         else
         {
             debugText.text = "Failed to start Host.";
             Debug.LogError("Failed to start Host.");
         }*/


        OnStartHost().Forget();
    }
    public async UniTask OnStartHost()
    {
        SetButtons();
        string joinCode =
            await RelayNetworkManager.Instance.StartHostWithRelay();

        roomCode.text = "Room Code: " + joinCode;
        screen.SetActive(false);

    }
    void SetButtons()
    {
        hostButton.interactable = false;
        ClientButton.interactable = false;
    }


    public void StartClient()
    {
        /*  Debug.Log("Starting Client...");
          bool success = NetworkManager.Singleton.StartClient();
          if (success)
          {
              Debug.Log("Client started successfully.");
              debugText.text = "Client started successfully.";
              screen.SetActive(false);
          }
          else
          {
              debugText.text = "Failed to start Client.";
              Debug.LogError("Failed to start Client.");
          }*/
        OnStartClient().Forget();
    }
    public async UniTask OnStartClient()
    {
        SetButtons();

        await RelayNetworkManager.Instance.StartClientWithRelay(joinCodeIF.text);
        roomCode.text = "Room Code: " + joinCodeIF.text;
        screen.SetActive(false);
    }

}