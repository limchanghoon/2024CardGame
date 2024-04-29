using DG.Tweening;
using TMPro;
using UnityEngine;

public class DeckMono : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Player player = null;
    [SerializeField] bool ismine;

    [SerializeField] Transform textBoxTr;
    [SerializeField] TextMeshPro deckCountText;

    private void OnMouseEnter()
    {
        if (ismine)
        {
            if (player == null) player = gameManager.GetMyPlayer();
            if (player == null) return;

            deckCountText.text = $"���� ���� �� {player.deck.Count}��";
            textBoxTr.DOMove(new Vector3(6f, -2.5f, -200f), 0);
        }
        else
        {
            if (player == null) player = gameManager.GetOppenetPlayer();
            if (player == null) return;

            deckCountText.text = $"���� ����� �� {player.deck.Count}��";
            textBoxTr.DOMove(new Vector3(6f, 2.5f, -200f), 0);
        }
    }

    private void OnMouseExit()
    {
        textBoxTr.DOMove(new Vector3(9999f, 9999f, 9999f), 0);
    }
}
