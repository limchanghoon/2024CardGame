using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class HandMouseEvent_Minion : IMyMouseEvent
{
    CardMono_Minion cardMono_Minion;
    Transform transform;

    private int expectedLocation = -2;

    public HandMouseEvent_Minion(CardMono_Minion _cardMono)
    {
        cardMono_Minion = _cardMono;
        transform = _cardMono.transform;
    }

    private int GetDraggingCardLocationFirst(float _posX)
    {
        int CNT = cardMono_Minion.owner.field.Count;
        if (CNT == cardMono_Minion.owner.field.Capacity) return -1;
        if (CNT == 0) return 0;
        CNT++;
        for (int i = 0; i < CNT; ++i)
        {
            if (_posX <= cardMono_Minion.owner.GetFieldPos(i, CNT).x) return i;
        }
        return CNT;
    }

    private int GetDraggingCardLocation(float _posX)
    {
        int CNT = cardMono_Minion.owner.field.Count;
        if (CNT == cardMono_Minion.owner.field.Capacity) return -1;
        if (CNT == 0) return 0;
        for (int i = 0; i < CNT; ++i)
        {
            if (_posX <= cardMono_Minion.owner.gameManager.GetCard(cardMono_Minion.owner.field[i]).transform.position.x)
            {
                return i;
            }
        }
        return CNT;
    }

    private void MoveAsExpected(int _location)
    {
        expectedLocation = _location;
        int cnt = cardMono_Minion.owner.field.Count + 1;
        for (int i = 0; i < cardMono_Minion.owner.field.Count; ++i)
        {
            Vector3 _des;
            if (i < _location) _des = cardMono_Minion.owner.GetFieldPos(i, cnt);
            else _des = cardMono_Minion.owner.GetFieldPos(i + 1, cnt);

            cardMono_Minion.owner.gameManager.GetCard(cardMono_Minion.owner.field[i]).SetPR(_des, Quaternion.identity, 1f);
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
        //transform.DOMove(cardMono.originPos, 0.2f);
        expectedLocation = -2;
        cardMono_Minion.owner.ChangeShowField();
        cardMono_Minion.owner.OnHandChanged();
    }

    private bool IsTargetOn(out RaycastHit2D hit)
    {
        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;
        Ray2D ray = new Ray2D(_pos, Vector2.zero);
        hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        int layer = LayerMask.NameToLayer("Targetable");
        // 임시
        if (hit.collider != null && hit.collider.gameObject.layer == layer && cardMono_Minion.owner.IsMyTurn())
        {
            var _ITargetable = hit.collider.GetComponent<ITargetable>();
            if (((_ITargetable.GetTargetType() & cardMono_Minion.cardSO.battleCryTarget) != 0) && _ITargetable.CanBeTarget())
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator SelectTargetCoroutine()
    {
        cardMono_Minion.SetPR(cardMono_Minion.owner.GetFieldPos(expectedLocation, cardMono_Minion.owner.field.Count + 1), Quaternion.identity, 1f);
        IPredict iPredict = cardMono_Minion.battleCryObj.GetComponent<IPredict>();
        while (true)
        {
            yield return null;
            RaycastHit2D hit;
            bool _isTargetOn = IsTargetOn(out hit);
            cardMono_Minion.owner.gameManager.SetLineTarget(cardMono_Minion.gameObject.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), true, _isTargetOn);
            iPredict?.Predict(_isTargetOn ? hit.collider.gameObject : null);
            if (!cardMono_Minion.owner.IsMyTurn())
            {
                GoBack();
                break;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (_isTargetOn)
                    cardMono_Minion.owner.RPC_SpawnMinionOfHand(cardMono_Minion.uniqueID, expectedLocation, hit.collider.GetComponent<ITargetable>().GetNetworkId());
                else
                    GoBack();
                break;
            }
        }
        cardMono_Minion.isDragging = false;
        iPredict?.Predict(null);
        cardMono_Minion.owner.gameManager.SetLineTarget(Vector3.zero, Vector3.zero, false, false);
    }




    // ~~인터페이스~~
    public void OnIsZoomingChanged()
    {
        if (cardMono_Minion.networkObject.HasInputAuthority) return;
        if (cardMono_Minion.isZooming || cardMono_Minion.isDragging) cardMono_Minion.GetBackFaceGlow().SetActive(true);
        else cardMono_Minion.GetBackFaceGlow().SetActive(false);
    }

    public void OnMyMouseDown()
    {
        if (!cardMono_Minion.networkObject.HasInputAuthority) return;
        cardMono_Minion.isDragging = true;
    }

    public void OnMyMouseUp()
    {
        if (!cardMono_Minion.networkObject.HasInputAuthority) return;
        if (!cardMono_Minion.isDragging) return;

        if (!cardMono_Minion.owner.IsMyTurn() || DraggingCardInMyHandArea() || cardMono_Minion.owner.field.Count == cardMono_Minion.owner.field.Capacity)
        {
            GoBack();
            cardMono_Minion.isDragging = false;
        }
        else
        {
            if (cardMono_Minion.battleCry != null &&cardMono_Minion.battleCry.IsNeedTarget())
            {
                if (cardMono_Minion.cardSO.IsTargetExist(CommandType.BattleCry, cardMono_Minion.owner.gameManager))
                    cardMono_Minion.StartCoroutine(SelectTargetCoroutine());
                else
                {
                    cardMono_Minion.owner.RPC_SpawnMinionOfHand(cardMono_Minion.uniqueID, expectedLocation);
                    cardMono_Minion.isDragging = false;
                }
            }
            else
            {
                cardMono_Minion.owner.RPC_SpawnMinionOfHand(cardMono_Minion.uniqueID, expectedLocation);
                cardMono_Minion.isDragging = false;
            }
        }
    }

    public void OnMyMouseDrag()
    {
        if (!cardMono_Minion.networkObject.HasInputAuthority) return;
        if (!cardMono_Minion.isDragging) return;

        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;

        transform.DOMove(_pos, 0);

        if (DraggingCardInMyHandArea())
        {
            if (!cardMono_Minion.isZooming)
            {
                cardMono_Minion.owner.ChangeShowField();
                cardMono_Minion.isZooming = true;
                expectedLocation = -2;
            }
            cardMono_Minion.imageTr.DOScale(Vector3.one * 1.3f, 0.2f);
            cardMono_Minion.imageTr.DOLocalMove(new Vector3(0, 3f, -100f), 0f);
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

            cardMono_Minion.isZooming = false;
            cardMono_Minion.imageTr.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.2f);
            cardMono_Minion.imageTr.DOLocalMove(Vector3.zero, 0f);
        }
    }

    public void OnMyMouseEnter()
    {
        if (!cardMono_Minion.networkObject.HasInputAuthority) return;
        if (cardMono_Minion.isDragging || cardMono_Minion.isZooming) return;
        cardMono_Minion.isZooming = true;
        cardMono_Minion.imageTr.DOScale(Vector3.one * 1.3f, 0.2f);
        cardMono_Minion.imageTr.DOLocalMove(new Vector3(0, 3f, -100f), 0f);

        transform.DORotateQuaternion(Quaternion.identity, 0.2f);
    }

    public void OnMyMouseExit()
    {
        if (!cardMono_Minion.networkObject.HasInputAuthority) return;
        if (cardMono_Minion.isDragging) return;
        cardMono_Minion.isZooming = false;
        cardMono_Minion.imageTr.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.2f);
        cardMono_Minion.imageTr.DOLocalMove(Vector3.zero, 0f);


        transform.DORotateQuaternion(cardMono_Minion.originRot, 0.2f);
    }
}
