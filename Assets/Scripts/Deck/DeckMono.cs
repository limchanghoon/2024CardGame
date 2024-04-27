using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckMono : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Player player = null;
    [SerializeField] bool ismine;

    private void OnMouseEnter()
    {
        if (ismine)
        {
            if (player == null) player = gameManager.GetMyPlayer();
            if (player == null) return;

            Debug.Log(player.deck.Count);
        }
        else
        {
            if (player == null) player = gameManager.GetOppenetPlayer();
            if (player == null) return;

            Debug.Log(player.deck.Count);
        }
    }
}
