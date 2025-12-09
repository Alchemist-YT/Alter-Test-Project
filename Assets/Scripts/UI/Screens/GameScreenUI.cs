using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScreenUI : BaseScreen
{
    [Header("Header")]
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI turnText;

    [Header("Card Areas")]
    [SerializeField] CardAreaUI playerArea;
    [SerializeField] CardAreaUI opponentArea;


    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform handContainer;
    [SerializeField] Button playCardButton;

    Dictionary<CardData, CardUI> cardMap = new();


    void OnDisable()
    {
        GameManager.Instance.OnTimerTick -= UpdateTimer;
        GameManager.Instance.OnScoreUpdated -= UpdateScore;
        GameManager.Instance.OnStateChange -= UpdateStateInfo;
        GameManager.Instance.OnBothPlayerConnected -= OnConnected;
        GameManager.Instance.OnStateChange -= OnStateChange;


        var controller = PlayerBoardController.Instance;
        controller.OnCardDrawn -= HandleCardDrawn;
        controller.OnCardStaged -= HandleCardStaged;
        controller.OnCardUnstaged -= HandleCardUnstaged;
        controller.OnCardSelectionChanged -= HandleSelectionChanged;

        var opponenetController = OpponentBoardController.Instance;
        opponenetController.OnSpawnFaceDownRequest -= HandleSpawnRequest;
        opponenetController.OnRevealCardRequest -= HandleRevealRequest;
        opponenetController.OnBoardClearRequest -= HandleClearRequest;

        PlayerBoardController.Instance.OnChangeInMana -= OnChangeInMana;
        GameManager.Instance.OnTurnStart -= OnTurnStart;


    }

    void OnConnected()
    {
        Show();
    }

    private void OnEnable()
    {
        var playerController = PlayerBoardController.Instance;
        playerController.OnCardDrawn += HandleCardDrawn;
        playerController.OnCardStaged += HandleCardStaged;
        playerController.OnCardUnstaged += HandleCardUnstaged;
        playerController.OnCardSelectionChanged += HandleSelectionChanged;

        var opponenetController = OpponentBoardController.Instance;
        opponenetController.OnSpawnFaceDownRequest += HandleSpawnRequest;
        opponenetController.OnRevealCardRequest += HandleRevealRequest;
        opponenetController.OnBoardClearRequest += HandleClearRequest;


        playCardButton.onClick.AddListener(() => playerController.TryPlaySelectedCard());
        playCardButton.gameObject.SetActive(false);
    }

    void Start()
    {
        UpdateScore(GameManager.Instance.HostScore, GameManager.Instance.ClientScore);

        GameManager.Instance.OnTimerTick += UpdateTimer;
        GameManager.Instance.OnScoreUpdated += UpdateScore;
        GameManager.Instance.OnStateChange += UpdateStateInfo;
        GameManager.Instance.OnBothPlayerConnected += OnConnected;
        GameManager.Instance.OnStateChange += OnStateChange;
        GameManager.Instance.OnTurnStart += OnTurnStart;

        PlayerBoardController.Instance.OnChangeInMana += OnChangeInMana;

        OnChangeInMana(PlayerBoardController.Instance.GetCurrentBalance());

    }
    void OnTurnStart()
    {
        OnChangeInMana(PlayerBoardController.Instance.GetCurrentBalance());
    }

    private void OnStateChange(GameState state)
    {
        OnChangeInMana(PlayerBoardController.Instance.GetCurrentBalance());
    }

    void OnChangeInMana(int balanceMana)
    {
        playerArea.UpdateMana(balanceMana, PlayerBoardController.Instance.MaxMana);
    }

    void HandleClearRequest()
    {
        opponentArea.HandleClearRequest();
    }

    void HandleRevealRequest(int index, CardData data)
    {
        opponentArea.HandleRevealRequest(index, data);

    }

    void HandleSpawnRequest(int count)
    {
        opponentArea.HandleSpawnRequest(count);
    }

    private void HandleCardDrawn(CardData data)
    {
        GameObject newCardObj = Instantiate(cardPrefab, handContainer);
        CardUI cardUI = newCardObj.GetComponent<CardUI>();

        cardUI.Setup(data, PlayerBoardController.Instance);
        cardMap.Add(data, cardUI);
    }

    private void HandleCardStaged(CardData data)
    {
        if (cardMap.TryGetValue(data, out CardUI ui))
        {
            playerArea.SetCardAsChild(ui);
            UpdateButtonState();
            ui.SetStaged(true);
        }
    }

    void HandleCardUnstaged(CardData data)
    {
        if (cardMap.TryGetValue(data, out CardUI ui))
        {
            ui.transform.SetParent(handContainer);
            UpdateButtonState();
            ui.SetStaged(false);
        }
    }

    void HandleSelectionChanged(CardData data, bool isSelected)
    {
        if (cardMap.TryGetValue(data, out CardUI ui))
        {
            ui.SetSelected(isSelected);
        }
        UpdateButtonState();
    }

    void UpdateButtonState()
    {
        playCardButton.gameObject.SetActive(PlayerBoardController.Instance.HasSelection());
    }

    void UpdateTimer(int timeRemaining)
    {
        timerText.text = $"{timeRemaining}s";
        if (timeRemaining <= 5) timerText.color = Color.red;
        else timerText.color = Color.white;
    }

    void UpdateScore(int hostScore, int clientScore)
    {
        playerArea.UpdateScore(GameManager.Instance.IsHost ? hostScore : clientScore);
        opponentArea.UpdateScore(GameManager.Instance.IsHost ? clientScore : hostScore);
    }

    void UpdateStateInfo(GameState state)
    {
        if (state == GameState.TurnPlanning || state == GameState.RevealPhase)
        {
            turnText.text = $"TURN {GameManager.Instance.CurrentTurn} / {GameManager.Instance.gameSettings.MaxTurns}";
        }
    }
}
