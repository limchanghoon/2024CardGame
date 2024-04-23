using TMPro;

public class ReUseScrollViewDeck : ReUseScrollViewContents<CardSO>
{
    protected override void UpdateContent(int childIndex, int dataIndex)
    {
        content.GetChild(childIndex).GetComponentInChildren<TextMeshProUGUI>().text = datas[dataIndex].cardName + " : " +datas[dataIndex].cost.ToString();
    }
}
