using UnityEngine;
using System;
using System.Collections.Generic;

public class OpponentBoardController : MonoBehaviourSingleton<OpponentBoardController>
{
    public event Action<int> OnSpawnFaceDownRequest; 
    public event Action<int, CardData> OnRevealCardRequest; 
    public event Action OnBoardClearRequest;


    int totalCardsOnBoard = 0;
    int currentTurnStartIndex = 0;

    public void SpawnFaceDownCards(int count)
    {
        currentTurnStartIndex = totalCardsOnBoard;
        totalCardsOnBoard += count;
        OnSpawnFaceDownRequest?.Invoke(count);
    }

    public void RevealCard(int turnIndex, int cardId)
    {
        int absoluteIndex = currentTurnStartIndex + turnIndex;

        if (absoluteIndex >= totalCardsOnBoard)
        {
            Debug.LogError($"Opponent trying to reveal index {absoluteIndex} but only has {totalCardsOnBoard} cards.");
            return;
        }

        CardData realData = CardDatabase.Instance.GetCard(cardId);

        OnRevealCardRequest?.Invoke(absoluteIndex, realData);
    }

    public void ClearBoard()
    {
        totalCardsOnBoard = 0;
        currentTurnStartIndex = 0;
        OnBoardClearRequest?.Invoke();
    }
}