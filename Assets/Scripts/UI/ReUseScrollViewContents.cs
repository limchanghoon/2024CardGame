using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ReUseScrollViewContents<T> : MonoBehaviour
{
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected RectTransform content;
    [SerializeField] protected ScrollView scrollView;
    protected List<T> datas = new List<T>();

    [SerializeField] protected float cell_Y;
    [SerializeField] protected float spaceing_Y;

    public int curIndex { get; protected set; }
    protected int itemSize;
    protected int lastIndex;

    protected RectTransform[] childs;

    protected virtual void Awake()
    {
        curIndex = 0;
        childs = new RectTransform[content.childCount];
        for (int i = 0; i < content.childCount; i++)
        {
            childs[i] = content.GetChild(i).GetComponent<RectTransform>();
            childs[i].anchoredPosition = new Vector2(0, -i * (cell_Y + spaceing_Y));
            childs[i].sizeDelta = new Vector2(childs[i].sizeDelta.x, cell_Y);
        }
        SetInitPosition();
    }

    public void OnValueChanged()
    {
        if (!canvas.enabled) return;
        if (itemSize > datas.Count) return;
        ScrollDown();
        ScrollUp();
    }

    public virtual void SetInitPosition()
    {
        curIndex = 0;
        for (int i = 0; i < content.childCount; i++)
        {
            childs[i].anchoredPosition = new Vector2(0, -i * (cell_Y + spaceing_Y));
            childs[i].SetAsLastSibling();
        }
    }

    public void SetDatas(List<T> inputData)
    {
        SetInitPosition();
        datas = inputData;
        itemSize = content.childCount;
        lastIndex = itemSize - 1;
        UpdateAllContent();
    }

    public void UpdateAllContent()
    {
        int i = 0;
        for (; i < itemSize && curIndex + i < datas.Count; i++)
        {
            UpdateContent(i, curIndex + i);
            content.GetChild(i).gameObject.SetActive(true);
        }
        for (; i < itemSize; i++)
        {
            content.GetChild(i).gameObject.SetActive(false);
        }
        float height = datas.Count * (cell_Y + spaceing_Y);
        content.sizeDelta = new Vector2(content.sizeDelta.x, height);
    }

    protected void ScrollDown()
    {
        if (curIndex + itemSize < datas.Count)
        {
            while (content.anchoredPosition.y >= (cell_Y + spaceing_Y) * (curIndex + 2 * itemSize + 1))
            {
                curIndex += itemSize;
            }
            if (content.anchoredPosition.y >= (cell_Y + spaceing_Y) * (curIndex+1))
            {
                content.GetChild(0).GetComponent<RectTransform>().anchoredPosition = -new Vector2(0, cell_Y + spaceing_Y) * (curIndex + itemSize);
                UpdateContent(0, curIndex + itemSize);
                content.GetChild(0).SetAsLastSibling();
                curIndex++;
                ScrollDown();
            }
        }
    }

    protected void ScrollUp()
    {
        if (curIndex > 0)
        {
            while (content.anchoredPosition.y < (cell_Y + spaceing_Y) * (curIndex - 2 * itemSize))
            {
                curIndex -= itemSize;
            }
            if (content.anchoredPosition.y < (cell_Y + spaceing_Y) * curIndex)
            {
                content.GetChild(lastIndex).GetComponent<RectTransform>().anchoredPosition = -new Vector2(0, cell_Y + spaceing_Y) * (curIndex - 1);
                UpdateContent(lastIndex, curIndex - 1);
                content.GetChild(lastIndex).SetAsFirstSibling();
                curIndex--;
                ScrollUp();
            }
        }
    }

    protected abstract void UpdateContent(int childIndex, int dataIndex);
}
