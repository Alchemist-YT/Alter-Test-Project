using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectionScreenUI : BaseScreen
{
    [SerializeField] TextMeshProUGUI debugText;

    void Start()
    {
        Show();
    }

    public void StartHost()
    {
        Debug.Log("Starting Host...");
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
        }
    }

    public void StartClient()
    {
        Debug.Log("Starting Client...");
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
        }
    }
}