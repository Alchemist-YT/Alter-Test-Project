using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameKeys
{
    public static class ActionKeys
    {
        public const string GameStart = "gameStart";
        public const string CommitTurn = "commitTurn";
        public const string SyncBoard = "syncBoard";
        public const string GameStateSync = "gameStateSync";
        public const string RevealSingleCard = "revealSingleCard";
        public const string UpdateScore = "updateScore";
        public const string GameEnd = "gameEnd";
    }

    public static class PlayerKeys
    {
        public const string Host = "Host";
        public const string Client = "Client";
    }

    public static class AbilityKeys
    {
        public const string GainPoints = "GainPoints";
        public const string StealPoints = "StealPoints";
    }
}