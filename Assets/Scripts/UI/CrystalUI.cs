using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrystalUI : MonoBehaviour
{
    [SerializeField] bool isMine;
    [SerializeField] TextMeshProUGUI crystalText;

    [Header("isMine이 true일 때만 사용")]
    [SerializeField] Image[] crystals;

    public void UpdateCryStal(int current, int max)
    {
        crystalText.text = $"{current}/{max}";
        if (isMine)
        {
            for(int i = 0;i < crystals.Length; i++)
            {
                if (i < current)
                    crystals[i].color = Color.white;
                else
                    crystals[i].color = new Color(0.2f, 0.2f, 0.2f);
                crystals[i].gameObject.SetActive(i < max);
            }

        }
    }
}
