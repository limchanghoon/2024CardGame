using System.Collections;
using System.Collections.Generic;
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

            textBoxTr.position = new Vector3(6f, -2.5f, -200f);
        }
        else
        {
            if (player == null) player = gameManager.GetOppenetPlayer();
            if (player == null) return;

            textBoxTr.position = new Vector3(6f, 2.5f, -200f);
        }

        deckCountText.text = $"남은 상대의 덱 {player.deck.Count}장";
    }

    private void OnMouseExit()
    {
        textBoxTr.position = new Vector3(9999f, 9999f, 9999f);
    }
}
