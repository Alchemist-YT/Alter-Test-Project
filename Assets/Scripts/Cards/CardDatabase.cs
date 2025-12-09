using UnityEngine;
using System.Collections.Generic;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance;
    public Dictionary<int, CardData> Library = new Dictionary<int, CardData>();

    void Awake()
    {
        Instance = this;
        LoadCards();
    }

    void LoadCards()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("cards");
        if (jsonFile == null)
        {
            Debug.LogError("cards.json not found in Resources!");
            return;
        }

        CardList wrapper = JsonUtility.FromJson<CardList>(jsonFile.text);
        foreach (var card in wrapper.cards)
        {
            Library[card.id] = card;
        }

        Debug.Log($"Loaded {Library.Count} cards.");
    }

    public CardData GetCard(int id)
    {
        if (Library.ContainsKey(id)) return Library[id];
        return null;
    }
}