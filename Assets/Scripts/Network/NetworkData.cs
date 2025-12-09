using System;
using UnityEngine;

[Serializable]
public class BaseMessage
{
    public string action;
    public string playerId;
}


[Serializable]
public class GameStartMessage : BaseMessage
{
    public int totalTurns;
    public int currentTurn;
}

[Serializable]
public class EndTurnMessage : BaseMessage
{

}

[Serializable]
public class CommitTurnMessage : BaseMessage
{
    public int[] cardIds;
}

[Serializable]
public class SyncBoardMessage : BaseMessage
{
    public int opponentCardCount;
}


[Serializable]
public class RevealCardMessage : BaseMessage
{
    public int cardId;
    public int orderIndex;
}

[Serializable]
public class ScoreUpdateMessage : BaseMessage
{
    public int hostScore;
    public int clientScore;
}

[Serializable]
public class GameStateSyncMessage : BaseMessage
{
    public int currentTurn;
    public int hostScore;
    public int clientScore;
    public GameState serverState;
    public int opponentFoldedCount;
}

[Serializable]
public class GameEndMessage : BaseMessage
{
    public string winnerId;
    public int finalHostScore;
    public int finalClientScore;
}

public static class JsonHandler
{
    public static string Serialize<T>(T data)
    {
        return JsonUtility.ToJson(data);
    }

    public static T Parse<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    public static string GetActionType(string json)
    {
        return JsonUtility.FromJson<BaseMessage>(json).action;
    }
}