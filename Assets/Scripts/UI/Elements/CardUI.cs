using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] TextMeshProUGUI powerText;
    [SerializeField] GameObject cardBackObject;

    [SerializeField] Image cardImage;
    [SerializeField] private Color[] typeColors;

    CardData Data;

    PlayerBoardController ownerController;
    bool isInteractable = true;
    int colorIndex;

    bool isStaged;

    public void Setup(CardData data, PlayerBoardController controller)
    {
        this.Data = data;
        this.ownerController = controller;

        nameText.text = data.name;
        costText.text = data.cost.ToString();
        powerText.text = data.power.ToString();

        cardBackObject.SetActive(false);

        PlayerBoardController.Instance.OnChangeInMana += HandleManaChanged;
        HandleManaChanged(PlayerBoardController.Instance.GetCurrentBalance());

        colorIndex = Mathf.Max(0, (data.cost - 1)) % typeColors.Length;
        cardImage.color = typeColors[colorIndex];
    }

    public void SetStaged(bool staged)
    {
        isStaged = staged;

    }

    private void OnDestroy()
    {
        PlayerBoardController.Instance.OnChangeInMana -= HandleManaChanged;
    }

    private void HandleManaChanged(int balanceMana)
    {
        if (Data.cost <= balanceMana)
        {
            SetInteractable(true);
        }
        else
        {
            SetInteractable(false);
        }
    }

    public void PutCardFaceDown()
    {
        cardBackObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable || ownerController == null) return;
        ownerController.SelectCard(Data);
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected) transform.DOScale(1.1f, 0.1f);
        else transform.DOScale(1.0f, 0.1f);
    }

    public void FlipOpen()
    {
        if (cardBackObject == null) return;
        cardBackObject.SetActive(true);
        Sequence s = DOTween.Sequence();
        s.Append(transform.DOScaleX(0, 0.2f));
        s.AppendCallback(() => cardBackObject.SetActive(false));
        s.Append(transform.DOScaleX(1, 0.2f));
    }

    public void SetInteractable(bool state)
    {
        if (isStaged) return;
        isInteractable = state;
        Color color = typeColors[colorIndex];

        if (!isInteractable)
            color.a = .5f;

        cardImage.color = color;
    }
}