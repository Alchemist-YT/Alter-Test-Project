using UnityEngine;
using TMPro;

public class WaitingScreenUI : BaseScreen
{
    [SerializeField] TextMeshProUGUI statusText;

    void Start()
    {
        screen.SetActive(false);
        GameManager.Instance.OnStateChange += HandleStateChange;
    }

    void OnDisable()
    {
        GameManager.Instance.OnStateChange -= HandleStateChange;
    }

    void HandleStateChange(GameState newState)
    {
        switch (newState)
        {
            case GameState.WaitingForPlayers:
                Show("Waiting for Opponent to Join...");
                break;

            case GameState.WaitingForSync:
            case GameState.TurnPlanning:
            case GameState.RevealPhase:
            case GameState.GameOver:
                Hide();
                break;
        }
    }

    public void Show(string message)
    {
        statusText.text = message;
        Show();
    }
}