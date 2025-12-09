using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardAreaUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI manaText;

    [SerializeField] RectTransform boardArea;

    [SerializeField] GameObject cardPrefab;

    List<CardUI> spawnedCards = new List<CardUI>();
    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    public void UpdateMana(int currentMana, int maxMana)
    {
        manaText.text = $" {currentMana} / {maxMana}";
    }

    public void HandleSpawnRequest(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, boardArea);
            CardUI ui = newCard.GetComponent<CardUI>();

            CardData hiddenData = new CardData { name = "Hidden", cost = 0, power = 0 };
            ui.Setup(hiddenData, null);

            ui.PutCardFaceDown();
            spawnedCards.Add(ui);
        }
    }

    public void HandleRevealRequest(int absoluteIndex, CardData realData)
    {
        if (absoluteIndex >= 0 && absoluteIndex < spawnedCards.Count)
        {
            CardUI ui = spawnedCards[absoluteIndex];

            ui.Setup(realData, null);

            ui.FlipOpen();
        }
    }

    public void HandleClearRequest()
    {
        foreach (var card in spawnedCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        spawnedCards.Clear();
    }
    public void SetCardAsChild(CardUI ui)
    {
        ui.transform.SetParent(boardArea);
       
    }

    public CardUI DrawCard(CardData data)
    {
        GameObject newCardObj = Instantiate(cardPrefab, boardArea);
        CardUI cardUI = newCardObj.GetComponent<CardUI>();

        return cardUI;
    }


}
