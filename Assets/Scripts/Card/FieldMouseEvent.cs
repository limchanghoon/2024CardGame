using UnityEngine;

public class FieldMouseEvent : IMyMouseEvent
{
    CardMono cardMono;
    Transform transform;

    public FieldMouseEvent(CardMono _cardMono)
    {
        cardMono = _cardMono;
        transform = _cardMono.transform;
    }

    public bool DraggingCardInMyHandArea()
    {
        throw new System.NotImplementedException();
    }

    public void OnIsZoomingChanged()
    {
        throw new System.NotImplementedException();
    }

    public void OnMyMouseDown()
    {
        throw new System.NotImplementedException();
    }

    public void OnMyMouseDrag()
    {
        throw new System.NotImplementedException();
    }

    public void OnMyMouseEnter()
    {
        throw new System.NotImplementedException();
    }

    public void OnMyMouseExit()
    {
        throw new System.NotImplementedException();
    }

    public void OnMyMouseUp()
    {
        throw new System.NotImplementedException();
    }
}
