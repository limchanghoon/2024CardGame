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
        if(current == null || current.currentHealth <= 0)
        {
            Disable();
            return;
        }
        if (current.cardSO.grade == CardGrade.Lengend)
            bgRender.sprite = legend;
        else
            bgRender.sprite = normal;
        nameText.text = current.cardSO.cardName;
        costText.text = current.cardSO.cost.ToString();
        powerText.text = current.currentPower.ToString();
        healthText.text = current.currentHealth.ToString();
        abilityText.text = current.cardSO.infomation.ToString();
    }

    private void OnMouseDown()
    {
        Disable();
    }
}
