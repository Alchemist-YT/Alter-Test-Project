using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class NetworkHandler
{
    public static bool IsHost => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;

    public static void Initialize()
    {
        Cleanup();
        GameNetworkMessenger.Instance.OnMessageReceived += HandleNetworkMessage;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public static void Cleanup()
    {
        GameNetworkMessenger.Instance.OnMessageReceived -= HandleNetworkMessage;
        if (NetworkManager.Singleton)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private static void OnClientConnected(ulong clientId)
    {
        if (!IsHost) return;
        if (GameManager.Instance.CurrentState == GameState.WaitingForPlayers &&
            NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            GameManager.Instance.StartTurn().Forget();
            SendGameStartMessage();
        }
        else if (GameManager.Instance.CurrentState != GameState.WaitingForPlayers)
        {
            SendGameStateSync();
        }
    }

    private static void HandleNetworkMessage(string json)
    {
        string action = JsonHandler.GetActionType(json);

        switch (action)
        {
            case GameKeys.ActionKeys.GameStart:
                var startMsg = JsonHandler.Parse<GameStartMessage>(json);
                GameManager.Instance.OnStartTurn(startMsg.currentTurn);
                break;

            case GameKeys.ActionKeys.GameStateSync:
                var syncMsg = JsonHandler.Parse<GameStateSyncMessage>(json);
                GameManager.Instance.RestoreState(syncMsg);
                break;

            case GameKeys.ActionKeys.SyncBoard:
                if (!IsHost)
                {
                    var boardMsg = JsonHandler.Parse<SyncBoardMessage>(json);
                    GameManager.Instance.OnSyncBoard(boardMsg.opponentCardCount);
                }
                break;

            case GameKeys.ActionKeys.CommitTurn:
                if (IsHost)
                {
                    var commitMsg = JsonHandler.Parse<CommitTurnMessage>(json);
                    GameManager.Instance.OnCommitTurn(commitMsg.cardIds);
                }
                break;

            case GameKeys.ActionKeys.RevealSingleCard:
                var revealMsg = JsonHandler.Parse<RevealCardMessage>(json);
                GameManager.Instance.OnRevealSingleCard(revealMsg.playerId, revealMsg.orderIndex, revealMsg.cardId);
                break;

            case GameKeys.ActionKeys.UpdateScore:
                var scoreMsg = JsonHandler.Parse<ScoreUpdateMessage>(json);
                GameManager.Instance.OnUpdateScore(scoreMsg.hostScore, scoreMsg.clientScore);
                break;
            case GameKeys.ActionKeys.GameEnd:
                var gameOverMessage = JsonHandler.Parse<GameEndMessage>(json);

                GameManager.Instance.ProcessGameOver(gameOverMessage.finalHostScore, gameOverMessage.finalClientScore, gameOverMessage.winnerId);
                break;

            default:
                Debug.LogWarning($"Unknown action received: {action}");
                break;
        }
    }

    public static void SendCommitTurnMessage(List<int> myCards)
    {
        string json = JsonHandler.Serialize(new CommitTurnMessage
        {
            action = GameKeys.ActionKeys.CommitTurn,
            cardIds = myCards.ToArray()
        });
        GameNetworkMessenger.Instance.SendAction(json);
    }

    public static void SendSyncBoardMessage(List<int> myCards)
    {
        string json = JsonHandler.Serialize(new SyncBoardMessage
        {
            action = GameKeys.ActionKeys.SyncBoard,
            opponentCardCount = myCards.Count
        });
        GameNetworkMessenger.Instance.SendAction(json);
    }

    public static void SendCardRevealMessage(string playerId, int cardId, int index)
    {
        var revealMsg = new RevealCardMessage
        {
            action = GameKeys.ActionKeys.RevealSingleCard,
            playerId = playerId,
            cardId = cardId,
            orderIndex = index
        };
        GameNetworkMessenger.Instance.SendAction(JsonHandler.Serialize(revealMsg));
    }

    public static void SendUpdateScoreMessage(int hostScore, int clientScore)
    {
        var scoreMsg = new ScoreUpdateMessage
        {
            action = GameKeys.ActionKeys.UpdateScore,
            hostScore = hostScore,
            clientScore = clientScore
        };
        GameNetworkMessenger.Instance.SendAction(JsonHandler.Serialize(scoreMsg));
    }

    public static void SendGameStartMessage()
    {
        var msg = new GameStartMessage
        {
            action = GameKeys.ActionKeys.GameStart,
            totalTurns = GameManager.Instance.gameSettings.MaxTurns,
            currentTurn = GameManager.Instance.CurrentTurn
        };
        GameNetworkMessenger.Instance.SendAction(JsonHandler.Serialize(msg));
    }

    public static void SendGameEndMessage(string winner, int hostScore, int clientScore)
    {
        var endMsg = new GameEndMessage
        {
            action = GameKeys.ActionKeys.GameEnd,
            winnerId = winner,
            finalHostScore = hostScore,
            finalClientScore = clientScore
        };
        GameNetworkMessenger.Instance.SendAction(JsonHandler.Serialize(endMsg));
    }

    private static void SendGameStateSync()
    {
        var syncMsg = new GameStateSyncMessage
        {
            action = GameKeys.ActionKeys.GameStateSync,
            currentTurn = GameManager.Instance.CurrentTurn,
            hostScore = GameManager.Instance.HostScore,
            clientScore = GameManager.Instance.ClientScore,
            serverState = GameManager.Instance.CurrentState,
            opponentFoldedCount = PlayerBoardController.Instance.GetCommittedCards().Count
        };
        GameNetworkMessenger.Instance.SendAction(JsonHandler.Serialize(syncMsg));
    }

    public static void Disconnect()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}