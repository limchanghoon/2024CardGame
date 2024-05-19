using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class DeckMaker : MonoBehaviour
{
    [SerializeField] MyJsonManager myJsonManager;
    [SerializeField] RectTransform deckListPanelTr;
    [SerializeField] RectTransform deckMakerTr;
    [SerializeField] TMP_InputField dectNameField;
    [SerializeField] TextMeshProUGUI deckCardCountText;

    bool inAnimation = false;
    int currentDeck = -1;

    DeckData[] decks = new DeckData[20];
    [SerializeField] TextMeshProUGUI[] deckNameTexts;
    [SerializeField] ReUseScrollViewDeck reUseScrollViewDeck;

    List<CardSO> cardSOs = new List<CardSO>();
    Dictionary<int, CardSO> cardSO_Map = new Dictionary<int, CardSO>();

    readonly int cardMax = 8;
    int currentPage = 0;
    List<ValueTuple<CardSO, int>> currentDeckCardSOs = new List<ValueTuple<CardSO, int>>();
    [Header("덱 메이커")]
    [SerializeField] TextMeshProUGUI[] deckCardTexts;
    [SerializeField] Image[] deckCardImages;
    [SerializeField] Button preButton;
    [SerializeField] Button nextButton;

    [SerializeField] Sprite legendSprite;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite magicSprite;
    AsyncOperationHandle<IList<CardSO>> op;
    private void Awake()
    {
        op = Addressables.LoadAssetsAsync<CardSO>("CardData", null);
        var _data = op.WaitForCompletion();

        for (int i = 0; i < decks.Length; i++)
        {
            decks[i] = myJsonManager.LoadDeckData(i);
            deckNameTexts[i].text = decks[i].DeckName;
        }

        foreach(var _cardSO in _data)
        {
            cardSOs.Add(_cardSO);
            cardSO_Map.Add(_cardSO.cardID, _cardSO);
        }
    }

    private void OnDestroy()
    {
        Addressables.Release(op);
    }

    public void SelectDeckSlot(int selected)
    {
        if (inAnimation) return;
        inAnimation = true;
        currentDeck = selected;
        dectNameField.text = decks[currentDeck].DeckName;

        currentPage = 0;
        ShowPage(0);
        currentDeckCardSOs.Clear();
        // decks[currentDeck]  => currentDeckCardSOs
        for (int i = 0; i < decks[currentDeck].cardIDs.Length; i++)
        {
            if (decks[currentDeck].cardIDs[i] <= 0) continue;
            bool isInclude = false;
            int j = 0;
            for (; j < currentDeckCardSOs.Count; ++j)
            {
                if (currentDeckCardSOs[j].Item1.cardID == decks[currentDeck].cardIDs[i])
                {
                    isInclude = true;
                    break;
                }
            }
            if (isInclude) currentDeckCardSOs[j] = (currentDeckCardSOs[j].Item1, currentDeckCardSOs[j].Item2 + 1);
            else currentDeckCardSOs.Add(new ValueTuple<CardSO, int>(cardSO_Map[decks[currentDeck].cardIDs[i]], 1));
        }

        SortCurrentDeck();
        reUseScrollViewDeck.SetDatas(currentDeckCardSOs);
        UpdateDeckCountText();

        deckListPanelTr.DOAnchorPos(new Vector3(1920, 0, 0), 1f).SetEase(Ease.OutExpo).OnComplete(() => { inAnimation = false; });
        deckMakerTr.DOAnchorPos(new Vector3(0, 0, 0), 1f).SetEase(Ease.OutExpo);
    }

    public void DeckComplete()
    {
        if (inAnimation) return;
        inAnimation = true;

        decks[currentDeck].DeckName = dectNameField.text;
        deckNameTexts[currentDeck].text = decks[currentDeck].DeckName;

        // currentDeckCardSOs => decks[currentDeck]
        int idx = 0;
        for(int i = 0; i < currentDeckCardSOs.Count; i++)
        {
            for (int j = 0; j < currentDeckCardSOs[i].Item2; ++j)
            {
                decks[currentDeck].cardIDs[idx] = currentDeckCardSOs[i].Item1.cardID;
                idx++;
            }
        }
        for(;idx < decks[currentDeck].cardIDs.Length; ++idx)
        {
            decks[currentDeck].cardIDs[idx] = -1;
        }

        myJsonManager.SaveDeckData(currentDeck, decks[currentDeck]);

        deckListPanelTr.DOAnchorPos(new Vector3(0, 0, 0), 1f).SetEase(Ease.OutExpo);
        deckMakerTr.DOAnchorPos(new Vector3(-1920, 0, 0), 1f).SetEase(Ease.OutExpo).OnComplete(() => { inAnimation = false; });

        currentDeckCardSOs.Clear();
    }

    public void ShowPage(int num)
    {
        currentPage += num;
        for (int i = 0; i < cardMax; ++i)
        {
            if (currentPage * cardMax + i < cardSOs.Count)
            {
                deckCardTexts[i].text = cardSOs[currentPage * cardMax + i].cardName;
                if (cardSOs[currentPage * cardMax + i].grade == CardGrade.Lengend)
                    deckCardImages[i].sprite = legendSprite;
                else
                    deckCardImages[i].sprite = normalSprite;
                deckCardImages[i].gameObject.SetActive(true);
            }
            else
            {
                deckCardImages[i].gameObject.SetActive(false);
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
        int dataIndex = currentPage * cardMax + idx;
        if (dataIndex >= cardSOs.Count) return;
        if (GetCurrentDeckCount() >= decks[currentDeck].cardIDs.Length) 
        {
            Debug.Log("덱이 가득 찼습니다!");
            return;
        }
        int _target = -1;
        for (int i = 0; i < currentDeckCardSOs.Count; ++i)
        {
            if (currentDeckCardSOs[i].Item1.cardID != cardSOs[dataIndex].cardID) continue;
            if (currentDeckCardSOs[i].Item2 >= currentDeckCardSOs[i].Item1.GetLimitCount())
            {
                Debug.Log($"{currentDeckCardSOs[i].Item1.cardName} 는 덱에 넣을 수 없습니다! ({currentDeckCardSOs[i].Item1.grade} 카드 최대 {currentDeckCardSOs[i].Item1.GetLimitCount()}개까지)");
                return;
            }
            _target = i;
            break;
        }
        if (_target == -1)
            currentDeckCardSOs.Add(new ValueTuple<CardSO, int>(cardSOs[dataIndex], 1));
        else
            currentDeckCardSOs[_target] = (currentDeckCardSOs[_target].Item1, currentDeckCardSOs[_target].Item2 + 1);

        SortCurrentDeck();
        reUseScrollViewDeck.UpdateAllContent();
        UpdateDeckCountText();
    }

    public void PullOutCard(ButtonPullOutDeckMaker buttonPullOutDeckMaker)
    {

        if (currentDeckCardSOs[buttonPullOutDeckMaker.dataIndex].Item2 > 1)
            currentDeckCardSOs[buttonPullOutDeckMaker.dataIndex] = (currentDeckCardSOs[buttonPullOutDeckMaker.dataIndex].Item1, currentDeckCardSOs[buttonPullOutDeckMaker.dataIndex].Item2 - 1);
        else
            currentDeckCardSOs.RemoveAt(buttonPullOutDeckMaker.dataIndex);
        
        SortCurrentDeck();
        reUseScrollViewDeck.UpdateAllContent();
        UpdateDeckCountText();
    }

    public void SortCurrentDeck()
    {
        currentDeckCardSOs.Sort((a, b) => {
            if (a.Item1.cost == b.Item1.cost)
                return a.Item1.cardID < b.Item1.cardID ? -1 : 1;
            else
                return a.Item1.cost < b.Item1.cost ? -1 : 1;
        });
    }

    public void UpdateDeckCountText()
    {
        deckCardCountText.text = GetCurrentDeckCount().ToString() + "/" + decks[currentDeck].cardIDs.Length.ToString();
    }

    private int GetCurrentDeckCount()
    {
        int _count = 0;
        for (int i = 0; i < currentDeckCardSOs.Count; ++i)
        {
            _count += currentDeckCardSOs[i].Item2;
        }
        return _count;
    }

}
