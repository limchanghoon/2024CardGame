using DG.Tweening;
using System;
using UnityEngine;

public class HandMouseEvent : IMyMouseEvent
{
    CardMono cardMono;
    Transform transform;

    public HandMouseEvent(CardMono _cardMono)
    {
        cardMono = _cardMono;
        transform = _cardMono.transform;
    }

    private bool DraggingCardInMyHandArea()
    {
        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;
        Ray2D ray = new Ray2D(_pos, Vector2.zero);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);
        int layer = LayerMask.NameToLayer("MyHandArea");

        return Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    public void OnIsZoomingChanged()
    {
        if (cardMono.networkObject.HasInputAuthority) return;
        if (cardMono.isZooming || cardMono.isDragging) cardMono.GetBackFaceGlow().SetActive(true);
        else cardMono.GetBackFaceGlow().SetActive(false);
    }

    public void OnMyMouseDown()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        cardMono.isDragging = true;
    }

    public void OnMyMouseUp()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (!cardMono.isDragging) return;
        cardMono.isDragging = false;

        if (!cardMono.owner.IsMyTurn() || DraggingCardInMyHandArea() || cardMono.owner.field.Count == cardMono.owner.field.Capacity)
        {
            transform.DOMove(cardMono.originPos, 0.2f);
        }
        else
        {
            cardMono.owner.SpawnCardOfHand(cardMono);
        }
    }

    public void OnMyMouseDrag()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (!cardMono.isDragging) return;

        if (DraggingCardInMyHandArea())
        {
            cardMono.isZooming = true;
            cardMono.imageTr.DOScale(Vector3.one * 1.3f, 0.2f);
            cardMono.imageTr.DOLocalMove(new Vector3(0, 3f, -100f), 0f);
        }
        else
        {
            cardMono.isZooming = false;
            cardMono.imageTr.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.2f);
            cardMono.imageTr.DOLocalMove(Vector3.zero, 0f);
        }

        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;
        transform.position = _pos;
    }

    public void OnMyMouseEnter()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (cardMono.isDragging || cardMono.isZooming) return;
        cardMono.isZooming = true;
        cardMono.imageTr.DOScale(Vector3.one * 1.3f, 0.2f);
        cardMono.imageTr.DOLocalMove(new Vector3(0, 3f, -100f), 0f);

        transform.DORotateQuaternion(Quaternion.identity, 0.2f);
    }

    public void OnMyMouseExit()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (cardMono.isDragging) return;
        cardMono.isZooming = false;
        cardMono.imageTr.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.2f);
        cardMono.imageTr.DOLocalMove(Vector3.zero, 0f);


        transform.DORotateQuaternion(cardMono.originRot, 0.2f);
    }
}
