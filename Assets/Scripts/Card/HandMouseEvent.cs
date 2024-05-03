using DG.Tweening;
using Fusion;
using System;
using System.Collections;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class HandMouseEvent : IMyMouseEvent
{
    CardMono cardMono;
    Transform transform;

    private int expectedLocation = -2;

    public HandMouseEvent(CardMono _cardMono)
    {
        cardMono = _cardMono;
        transform = _cardMono.transform;
    }

    private int GetDraggingCardLocationFirst(float _posX)
    {
        int CNT = cardMono.owner.field.Count;
        if (CNT == cardMono.owner.field.Capacity) return -1;
        if (CNT == 0) return 0;
        CNT++;
        for (int i = 0; i < CNT; ++i)
        {
            if (_posX <= cardMono.owner.GetFieldPos(i, CNT).x) return i;
        }
        return CNT;
    }

    private int GetDraggingCardLocation(float _posX)
    {
        int CNT = cardMono.owner.field.Count;
        if (CNT == cardMono.owner.field.Capacity) return -1;
        if (CNT == 0) return 0;
        for (int i = 0; i < CNT; ++i)
        {
            if (_posX <= cardMono.owner.gameManager.GetCard(cardMono.owner.field[i]).transform.position.x)
            {
                return i;
            }
        }
        return CNT;
    }

    private void MoveAsExpected(int _location)
    {
        expectedLocation = _location;
        int cnt = cardMono.owner.field.Count + 1;
        for (int i = 0; i < cardMono.owner.field.Count; ++i)
        {
            Vector3 _des;
            if (i < _location) _des = cardMono.owner.GetFieldPos(i, cnt);
            else _des = cardMono.owner.GetFieldPos(i + 1, cnt);

            cardMono.owner.gameManager.GetCard(cardMono.owner.field[i]).SetPR(_des, Quaternion.identity, 1f);
        }
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

    private void GoBack()
    {
        transform.DOMove(cardMono.originPos, 0.2f);
        expectedLocation = -2;
        cardMono.owner.OnFieldChanged();
        cardMono.owner.OnHandChanged();
    }

    private bool IsTargetOn(out RaycastHit2D hit)
    {
        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;
        Ray2D ray = new Ray2D(_pos, Vector2.zero);
        hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        int layer = LayerMask.NameToLayer("Targetable");
        // 임시
        if (hit.collider != null && hit.collider.gameObject.layer == layer && cardMono.owner.IsMyTurn())
        {
            var _ITargetable = hit.collider.GetComponent<ITargetable>();
            if (((_ITargetable.GetTargetType() & cardMono.cardSO.battleCryTarget) != 0) && _ITargetable.CanBeTarget())
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator SelectTargetCoroutine()
    {
        cardMono.SetPR(cardMono.owner.GetFieldPos(expectedLocation, cardMono.owner.field.Count + 1), Quaternion.identity, 1f);
        while (true)
        {
            yield return null;
            RaycastHit2D hit;
            bool _isTargetOn = IsTargetOn(out hit);
            cardMono.owner.gameManager.SetLineTarget(cardMono.gameObject.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), true, _isTargetOn);
            if (!cardMono.owner.IsMyTurn())
            {
                GoBack();
                break;
            }
            if(Input.GetMouseButtonDown(0))
            {
                if (_isTargetOn)
                    cardMono.owner.SpawnCardOfHand(cardMono.uniqueID, expectedLocation, hit.collider.GetComponent<ITargetable>().GetNetworkId());
                else
                    GoBack();
                break;
            }
        }
        cardMono.isDragging = false;
        cardMono.owner.gameManager.SetLineTarget(Vector3.zero, Vector3.zero, false, false);
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

        if (!cardMono.owner.IsMyTurn() || DraggingCardInMyHandArea() || cardMono.owner.field.Count == cardMono.owner.field.Capacity)
        {
            GoBack();
            cardMono.isDragging = false;
        }
        else
        {
            if (cardMono.battleCry != null && cardMono.battleCry.IsNeedTarget())
            {
                if (cardMono.cardSO.IsTargetExist(cardMono.owner.gameManager))
                    cardMono.StartCoroutine(SelectTargetCoroutine());
                else
                {
                    cardMono.owner.SpawnCardOfHand(cardMono.uniqueID, expectedLocation);
                    cardMono.isDragging = false;
                }
            }
            else
            {
                cardMono.owner.SpawnCardOfHand(cardMono.uniqueID, expectedLocation);
                cardMono.isDragging = false;
            }
        }
    }

    public void OnMyMouseDrag()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (!cardMono.isDragging) return;

        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;

        transform.DOMove(_pos, 0);

        if (DraggingCardInMyHandArea())
        {
            if (!cardMono.isZooming)
            {
                cardMono.owner.OnFieldChanged();
                cardMono.isZooming = true;
                expectedLocation = -2;
            }
            cardMono.imageTr.DOScale(Vector3.one * 1.3f, 0.2f);
            cardMono.imageTr.DOLocalMove(new Vector3(0, 3f, -100f), 0f);
        }
        else
        {
            // 맨 처음에 실행
            if (expectedLocation < 0)
            {
                int _location = GetDraggingCardLocationFirst(_pos.x);
                if (_location != -1)
                {
                    MoveAsExpected(_location);
                }
            }
            else
            {
                int _location = GetDraggingCardLocation(_pos.x);
                if (_location != expectedLocation)
                {
                    MoveAsExpected(_location);
                }
            }

            cardMono.isZooming = false;
            cardMono.imageTr.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.2f);
            cardMono.imageTr.DOLocalMove(Vector3.zero, 0f);
        }
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
