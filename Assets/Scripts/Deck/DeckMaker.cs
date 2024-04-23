using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckMaker : MonoBehaviour
{
    [SerializeField] MyJsonManager myJsonManager;
    [SerializeField] RectTransform deckListPanelTr;
    [SerializeField] RectTransform deckMakerTr;
    [SerializeField] TMP_InputField dectNameField;

    bool inAnimation = false;
    int currentDeck = -1;

    DeckData[] decks = new DeckData[20];
    [SerializeField] TextMeshProUGUI[] deckNameTexts;
    [SerializeField] ReUseScrollViewDeck reUseScrollViewDeck;

    [SerializeField] List<CardSO> cardSOs = new List<CardSO>();

    [Header("덱 메이커")]
    readonly int cardMax = 8;
    int currentPage = 0;
    [SerializeField] List<CardSO> currentDeckCardSOs = new List<CardSO>();
    [SerializeField] TextMeshProUGUI[] deckCardTexts;
    [SerializeField] Button preButton;
    [SerializeField] Button nextButton;

    private void Awake()
    {
        for(int i = 0; i < decks.Length; i++)
        {
            decks[i] = myJsonManager.LoadDeckData(i);
            deckNameTexts[i].text = decks[i].DeckName;
        }
    }

    public void SelectDeckSlot(int idx)
    {
        if (inAnimation) return;
        inAnimation = true;
        currentDeck = idx;
        dectNameField.text = decks[currentDeck].DeckName;

        currentPage = 0;
        ShowPage(0);
        currentDeckCardSOs.Clear();
        reUseScrollViewDeck.SetDatas(currentDeckCardSOs);

        deckListPanelTr.DOAnchorPos(new Vector3(1920, 0, 0), 1f).SetEase(Ease.OutExpo).OnComplete(() => { inAnimation = false; });
        deckMakerTr.DOAnchorPos(new Vector3(0, 0, 0), 1f).SetEase(Ease.OutExpo);
    }

    public void DeckComplete()
    {
        if (inAnimation) return;
        inAnimation = true;

        decks[currentDeck].DeckName = dectNameField.text;
        deckNameTexts[currentDeck].text = decks[currentDeck].DeckName;
        myJsonManager.SaveDeckData(currentDeck, decks[currentDeck]);

        deckListPanelTr.DOAnchorPos(new Vector3(0, 0, 0), 1f).SetEase(Ease.OutExpo);
        deckMakerTr.DOAnchorPos(new Vector3(-1920, 0, 0), 1f).SetEase(Ease.OutExpo).OnComplete(() => { inAnimation = false; });
    }

    public void ShowPage(int num)
    {
        currentPage += num;
        for (int i = 0; i < cardMax; ++i)
        {
            if (currentPage * cardMax + i < cardSOs.Count)
            {
                deckCardTexts[i].text = cardSOs[currentPage * cardMax + i].cardName;
            }
            else
            {
                deckCardTexts[i].text = "없음";
            }
        }
        UpdateShowPageButton();
    }

    public void UpdateShowPageButton()
    {
        if (currentPage == 0) preButton.interactable = false;
        else preButton.interactable = true;

        if (currentPage * cardMax + cardMax >= cardSOs.Count) nextButton.interactable = false;
        else nextButton.interactable = true;
    }

    public void SelectCard(int idx)
    {
        if (currentPage * cardMax + idx >= cardSOs.Count) return;
        currentDeckCardSOs.Add(cardSOs[currentPage * cardMax + idx]);
        reUseScrollViewDeck.SetDatas(currentDeckCardSOs);
    }
}
