using System;
using TMPro;

public class ReUseScrollViewDeck : ReUseScrollViewContents<ValueTuple<CardSO, int>>
{
    protected override void UpdateContent(int childIndex, int dataIndex)
    {
        content.GetChild(childIndex).GetComponent<ButtonPullOutDeckMaker>().UpdateContent(datas[dataIndex], dataIndex);
    }
}
