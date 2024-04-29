using DG.Tweening;
using TMPro;
using UnityEngine;

public class FieldCardTooltip : MonoBehaviour
{
    [SerializeField] TextMeshPro nameText;
    [SerializeField] TextMeshPro costText;
    [SerializeField] TextMeshPro powerText;
    [SerializeField] TextMeshPro healthText;

    CardMono current;

    public void Show(CardMono cardMono, Vector3 _pos)
    {
        current = cardMono;
        _pos.z = -100f;
        if (Camera.main.WorldToViewportPoint(_pos).x < 0.5f)
        {
            transform.DOMove(_pos + Vector3.right * 3f, 0);
        }
        else
        {
            transform.DOMove(_pos + Vector3.left * 3f, 0);
        }

        UpdateText();

        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        current = null;
    }

    public void UpdateText()
    {
        if(current ==null || current.currentHealth <= 0)
        {
            Disable();
            return;
        }
        nameText.text = current.cardSO.cardName;
        costText.text = current.cardSO.cost.ToString();
        powerText.text = current.currentPower.ToString();
        healthText.text = current.currentHealth.ToString();
    }
}
