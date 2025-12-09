using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System; // Required for Actions/Events

public class PlayerBoardController : MonoBehaviourSingleton<PlayerBoardController>
{
    public event Action<CardData> OnCardDrawn;
    public event Action<CardData> OnCardStaged;
    public event Action<CardData> OnCardUnstaged;
    public event Action<CardData, bool> OnCardSelectionChanged;
    public event Action<int> OnChangeInMana;

    public int MaxMana => GameManager.Instance.CurrentTurn;
    [SerializeField] int initialCardInHand = 3;

    List<CardData> myHandData = new List<CardData>();
    List<CardData> stagedCardData = new List<CardData>();
    CardData currentSelectedCardData;

    private void Start()
    {
        for (int i = 0; i < initialCardInHand; i++)
        {
            DrawCard();
        }
        OnChangeInMana?.Invoke(GetCurrentBalance());
    }


    public void SelectCard(CardData data)
    {
        if (GameManager.Instance.CurrentState != GameState.TurnPlanning) return;

        if (stagedCardData.Contains(data))
        {
            UnstageCard(data);
            return;
        }

        if (currentSelectedCardData != null)
        {
            OnCardSelectionChanged?.Invoke(currentSelectedCardData, false);
        }

        if (currentSelectedCardData == data)
        {
            currentSelectedCardData = null;
            return;
        }

        currentSelectedCardData = data;
        OnCardSelectionChanged?.Invoke(currentSelectedCardData, true);
    }

    public int GetUsedMana()
    {
        return stagedCardData.Sum(card => card.cost);
    }

    public int GetCurrentBalance()
    {
        return MaxMana - GetUsedMana();
    }

    public void TryPlaySelectedCard()
    {
        if (currentSelectedCardData == null) return;

        int currentCost = stagedCardData.Sum(c => c.cost);

        if (currentCost + currentSelectedCardData.cost <= MaxMana)
        {
            myHandData.Remove(currentSelectedCardData);
            stagedCardData.Add(currentSelectedCardData);

            OnCardStaged?.Invoke(currentSelectedCardData);

            OnCardSelectionChanged?.Invoke(currentSelectedCardData, false);
            currentSelectedCardData = null;

            OnChangeInMana?.Invoke(GetCurrentBalance());
        }
        else
        {
            Debug.Log("Not enough Mana!");
        }
    }

    public void UnstageCard(CardData data)
    {
        if (stagedCardData.Contains(data))
        {
            stagedCardData.Remove(data);
            myHandData.Add(data);
            OnCardUnstaged?.Invoke(data);
        }
    }

    public void DrawCard()
    {
        var allCards = CardDatabase.Instance.Library.Values.ToList();
        if (allCards.Count == 0) return;

        CardData newCard = CloneCard(allCards[UnityEngine.Random.Range(0, allCards.Count)]);

        myHandData.Add(newCard);

        OnCardDrawn?.Invoke(newCard);
    }

    public void CommitAndStartNewTurn()
    {
        stagedCardData.Clear();
    }
    public List<int> GetCommittedCards()
    {
        return stagedCardData.Select(c => c.id).ToList();
    }

    public bool HasSelection() => currentSelectedCardData != null;

    private CardData CloneCard(CardData original)
    {
        return new CardData
        {
            id = original.id,
            name = original.name,
            cost = original.cost,
            power = original.power,
            ability = new CardAbilityData { type = original.ability.type, value = original.ability.value }
        };
    }
}