using System;
using TMPro;
using UnityEngine;

public class ButtonPullOutDeckMaker : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI countText;

    public int dataIndex { get; set; }

    public void UpdateContent(ValueTuple<CardSO, int> input, int _dataIndex)
    {
        costText.text = input.Item1.cost.ToString();
        nameText.text = input.Item1.cardName;
        countText.text = input.Item2.ToString();

        dataIndex = _dataIndex;
    }
}
