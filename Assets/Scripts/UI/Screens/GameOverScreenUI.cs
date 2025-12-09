using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreenUI : BaseScreen
{
    [SerializeField] TextMeshProUGUI winnerText;
    private void OnEnable()
    {
        GameManager.Instance.OnGameOver += OnGameOver;
    }

    private void OnGameOver(string winner, int hostScore, int clientScore)
    {
        winnerText.text = (winner == GameKeys.PlayerKeys.Host) && GameManager.Instance.IsHost ? "You Win!" : "You Lose!";
        Show();
    }
    private void OnDisable()
    {
        GameManager.Instance.OnGameOver -= OnGameOver;
    }

    public void PlayAgainButtonPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameManager.Instance.Disconnect();
    }
}
