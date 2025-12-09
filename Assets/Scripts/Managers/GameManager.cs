using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

public enum GameState
{
    WaitingForPlayers,
    TurnPlanning,
    WaitingForSync,
    RevealPhase,
    GameOver
}

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public event Action<int, int> OnScoreUpdated;
    public event Action<int> OnTimerTick;
    public event Action<GameState> OnStateChange;
    public event Action OnBothPlayerConnected;
    public event Action OnTurnStart;
    public event Action<string, int, int> OnGameOver;

    [field: SerializeField] public GameState CurrentState { get; private set; }
    [field: SerializeField] public int CurrentTurn { get; private set; } = 1;
    [field: SerializeField] public int HostScore { get; private set; }
    [field: SerializeField] public int ClientScore { get; private set; }
    public bool IsHost => NetworkHandler.IsHost;

    public GameSettingsSO gameSettings;

    bool localDone = false;
    bool remoteDone = false;

    List<int> hostCardsToReveal = new List<int>();
    List<int> clientCardsToReveal = new List<int>();

    void Start()
    {
        NetworkHandler.Initialize();
        CurrentState = GameState.WaitingForPlayers;
        OnStateChange?.Invoke(CurrentState);
    }
    void OnDestroy()
    {
        NetworkHandler.Cleanup();
    }

    public async UniTaskVoid StartTurn()
    {
        if (CurrentTurn == 1)
            OnBothPlayerConnected?.Invoke();

        CurrentState = GameState.TurnPlanning;
        OnStateChange?.Invoke(CurrentState);

        localDone = false;
        remoteDone = false;
        hostCardsToReveal.Clear();
        clientCardsToReveal.Clear();

        PlayerBoardController.Instance.CommitAndStartNewTurn();
        if (CurrentTurn != 1)
            PlayerBoardController.Instance.DrawCard();

        Debug.Log($"STARTING TURN {CurrentTurn}");

        var cts = new System.Threading.CancellationTokenSource();
        await UniTask.WhenAny(
            RunTurnTimer(gameSettings.TurnDuration, cts.Token),
            WaitForLocalPlayerEndTurn(cts.Token)
        );
        cts.Cancel();
        EnterSyncPhase();

        OnTurnStart?.Invoke();
    }

    async UniTask RunTurnTimer(int duration, System.Threading.CancellationToken token)
    {
        float timer = duration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            OnTimerTick?.Invoke(Mathf.CeilToInt(timer));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        OnLocalPlayerEndTurnBtn();
    }

    async UniTask WaitForLocalPlayerEndTurn(System.Threading.CancellationToken token)
    {
        await UniTask.WaitUntil(() => localDone, cancellationToken: token);
    }

    public void OnLocalPlayerEndTurnBtn()
    {
        if (localDone || CurrentState != GameState.TurnPlanning) return;

        localDone = true;
        var myCards = PlayerBoardController.Instance.GetCommittedCards();

        if (IsHost)
        {
            hostCardsToReveal = myCards;
            NetworkHandler.SendSyncBoardMessage(myCards);
        }
        else
        {
            NetworkHandler.SendCommitTurnMessage(myCards);
        }
    }

    void EnterSyncPhase()
    {
        CurrentState = GameState.WaitingForSync;
        OnStateChange?.Invoke(CurrentState);
        CheckIfBothReady();
    }

    void CheckIfBothReady()
    {
        if (localDone && remoteDone)
        {
            if (IsHost)
            {
                RunHostRevealSequence().Forget();
            }
        }
    }

    async UniTaskVoid RunHostRevealSequence()
    {
        CurrentState = GameState.RevealPhase;

        bool hostGoesFirst;
        if (HostScore > ClientScore) hostGoesFirst = true;
        else if (ClientScore > HostScore) hostGoesFirst = false;
        else hostGoesFirst = UnityEngine.Random.value > 0.5f;

        Queue<int> p1Queue = new Queue<int>(hostGoesFirst ? hostCardsToReveal : clientCardsToReveal);
        Queue<int> p2Queue = new Queue<int>(hostGoesFirst ? clientCardsToReveal : hostCardsToReveal);

        string p1Id = hostGoesFirst ? GameKeys.PlayerKeys.Host : GameKeys.PlayerKeys.Client;
        string p2Id = hostGoesFirst ? GameKeys.PlayerKeys.Client : GameKeys.PlayerKeys.Host;

        int maxCards = Mathf.Max(p1Queue.Count, p2Queue.Count);

        for (int i = 0; i < maxCards; i++)
        {
            if (p1Queue.Count > 0)
                await ProcessSingleReveal(p1Id, p1Queue.Dequeue(), i, hostGoesFirst);

            if (p2Queue.Count > 0)
                await ProcessSingleReveal(p2Id, p2Queue.Dequeue(), i, !hostGoesFirst);
        }

        await UniTask.Delay(250);

        if (CurrentTurn < gameSettings.MaxTurns)
        {
            CurrentTurn++;
            NetworkHandler.SendGameStartMessage();

            StartTurn().Forget();
        }
        else
        {
            CurrentState = GameState.GameOver;
            OnStateChange?.Invoke(CurrentState);

            string winner = "";
            if (HostScore > ClientScore) winner = GameKeys.PlayerKeys.Host;
            else if (ClientScore > HostScore) winner = GameKeys.PlayerKeys.Client;
            winner = UnityEngine.Random.value > 0.5f ? GameKeys.PlayerKeys.Host : GameKeys.PlayerKeys.Client;

            NetworkHandler.SendGameEndMessage(winner, HostScore, ClientScore);

            OnGameOver?.Invoke(winner, HostScore, ClientScore);
        }
    }
    public void ProcessGameOver(int hostScore, int clientScore, string winner)
    {
        HostScore = hostScore;
        ClientScore = clientScore;
        OnScoreUpdated?.Invoke(HostScore, ClientScore);

        CurrentState = GameState.GameOver;
        OnStateChange?.Invoke(CurrentState);
        OnGameOver?.Invoke(winner, HostScore, ClientScore);
    }



    async UniTask ProcessSingleReveal(string playerId, int cardId, int index, bool isHostPlayer)
    {
        CardData card = CardDatabase.Instance.GetCard(cardId);
        if (card != null)
        {
            ModifyScore(isHostPlayer, card.power);
            Ability ability = AbilityFactory.GetAbility(card.ability.type);
            ability.Execute(this, isHostPlayer, card.ability.value);
        }

        NetworkHandler.SendCardRevealMessage(playerId, cardId, index);

        NetworkHandler.SendUpdateScoreMessage(this.HostScore, this.ClientScore);

        await UniTask.Delay(200);
    }

    public void ModifyScore(bool isHost, int amount)
    {
        if (isHost) HostScore += amount;
        else ClientScore += amount;
    }


    public void OnUpdateScore(int hostScore, int clientScore)
    {
        HostScore = hostScore;
        ClientScore = clientScore;
        OnScoreUpdated?.Invoke(HostScore, ClientScore);
    }

    public void OnRevealSingleCard(string playerId, int orderIndex, int cardId)
    {
        bool isMsgHost = (playerId == GameKeys.PlayerKeys.Host);

        if (IsHost != isMsgHost)
        {
            OpponentBoardController.Instance.RevealCard(orderIndex, cardId);
        }
    }
    public void OnCommitTurn(int[] cardIds)
    {
        clientCardsToReveal = cardIds.ToList();
        OpponentBoardController.Instance.SpawnFaceDownCards(cardIds.Length);
        remoteDone = true;
        CheckIfBothReady();
    }
    public void OnSyncBoard(int opponentCardCount)
    {
        OpponentBoardController.Instance.SpawnFaceDownCards(opponentCardCount);
        remoteDone = true;
        CheckIfBothReady();
    }

    public void OnStartTurn(int currentTurn)
    {
        this.CurrentTurn = currentTurn;
        if (!IsHost) StartTurn().Forget();
    }

    public void RestoreState(GameStateSyncMessage msg)
    {
        CurrentTurn = msg.currentTurn;
        HostScore = msg.hostScore;
        ClientScore = msg.clientScore;
        CurrentState = msg.serverState;
        OnScoreUpdated?.Invoke(HostScore, ClientScore);
        OnStateChange?.Invoke(CurrentState);
        if (msg.opponentFoldedCount > 0)
        {
            OpponentBoardController.Instance.SpawnFaceDownCards(msg.opponentFoldedCount);
        }
    }

    public void Disconnect()
    {
        NetworkHandler.Disconnect();
    }
}