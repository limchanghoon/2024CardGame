using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckMakerCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] TextMeshProUGUI infoText;

    [SerializeField] TextMeshProUGUI powerText;
    [SerializeField] TextMeshProUGUI hpText;

    [SerializeField] Image bgSprite;

    [SerializeField] Sprite legendSprite;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite magicSprite;

    [SerializeField] GameObject minion_BG;
    [SerializeField] GameObject magic_BG;

    public void UpdateUI(CardSO cardSO)
    {

        nameText.text = cardSO.cardName;
        costText.text = cardSO.cost.ToString();
        infoText.text = cardSO.infomation;

        if (cardSO.cardType == CardType.Magic)
        {
            minion_BG.SetActive(false);
            magic_BG.SetActive(true);

            bgSprite.sprite = magicSprite;

            powerText.text = string.Empty;
            hpText.text = string.Empty;
        }
        else
        {
            minion_BG.SetActive(true);
            magic_BG.SetActive(false);

            if (cardSO.grade == CardGrade.Lengend)
                bgSprite.sprite = legendSprite;
            else
                bgSprite.sprite = normalSprite;

            powerText.text = cardSO.power.ToString();
            hpText.text = cardSO.health.ToString();
        }
    }
}
