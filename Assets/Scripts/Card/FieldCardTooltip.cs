using DG.Tweening;
using TMPro;
using UnityEngine;

public class FieldCardTooltip : MonoBehaviour
{
    [SerializeField] SpriteRenderer cardRender;
    [SerializeField] SpriteRenderer bgRender;
    [SerializeField] Sprite normal;
    [SerializeField] Sprite legend;

    [SerializeField] TextMeshPro nameText;
    [SerializeField] TextMeshPro costText;
    [SerializeField] TextMeshPro powerText;
    [SerializeField] TextMeshPro healthText;
    [SerializeField] TextMeshPro abilityText;

    [SerializeField] GameObject card_minion;
    [SerializeField] GameObject card_magic;

    CardMono current;
    [SerializeField] float zDepth;
    [SerializeField] bool isAutoClose;
    float timer = 0f;

    private void Update()
    {
        if (isAutoClose)
        {
            timer += Time.deltaTime;
            if(timer >= 2f)
            {
                Disable();
            }
        }
    }

    public void Show(CardMono cardMono, Vector3 _pos)
    {
        timer = 0f;
        _pos.z = zDepth;
        if (Camera.main.WorldToViewportPoint(_pos).x < 0.5f)
        {
            transform.DOMove(_pos + Vector3.right * 3f, 0);
        }
        else
        {
            transform.DOMove(_pos + Vector3.left * 3f, 0);
        }

        current = cardMono;
        UpdateUI();

        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        current = null;
    }

    public void UpdateUI()
    {
        if(current == null)
        {
            Disable();
            return;
        }

        if(current is CardMono_Minion)
        {
            CardMono_Minion cardMono_Minion = current as CardMono_Minion;
            if(cardMono_Minion.currentHealth <= 0)
            {
                Disable();
                return;
            }
            if (card_minion != null && card_magic != null)
            {
                card_minion.SetActive(true);
                card_magic.SetActive(false);
            }
            if (cardMono_Minion.cardSO.grade == CardGrade.Lengend)
                bgRender.sprite = legend;
            else
                bgRender.sprite = normal;

            nameText.rectTransform.anchoredPosition.Set(0, -0.3f);

            nameText.text = cardMono_Minion.cardSO.cardName;
            costText.text = cardMono_Minion.cardSO.cost.ToString();
            powerText.text = cardMono_Minion.currentPower.ToString();
            healthText.text = cardMono_Minion.currentHealth.ToString();
            abilityText.text = cardMono_Minion.cardSO.infomation.ToString();
        }
        else
        {
            CardMono_Magic cardMono_Magic = current as CardMono_Magic;
            if (cardMono_Magic == null)
            {
                Disable();
                return;
            }
            if (card_minion != null && card_magic != null)
            {
                card_minion.SetActive(false);
                card_magic.SetActive(true);
            }
            nameText.rectTransform.anchoredPosition.Set(0, -0.15f);

            nameText.text = cardMono_Magic.cardSO.cardName;
            costText.text = cardMono_Magic.cardSO.cost.ToString();
            abilityText.text = cardMono_Magic.cardSO.infomation.ToString();
        }
    }


    private void OnMouseDown()
    {
        Disable();
    }
}
