using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;


public class HandMouseEvent_Magic : IMyMouseEvent
{
    CardMono_Magic cardMono_Magic;
    Transform transform;

    public HandMouseEvent_Magic(CardMono_Magic _cardMono)
    {
        cardMono_Magic = _cardMono;
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

    private void GoBack()
    {
        Vector3 nearZ = transform.position;
        nearZ.z = 0;
        cardMono_Magic.SetPR(nearZ, Quaternion.identity, 0f);
        //cardMono.owner.ChangeShowField();
        cardMono_Magic.owner.OnHandChanged();
    }

    private bool IsTargetOn(out RaycastHit2D hit)
    {
        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;
        Ray2D ray = new Ray2D(_pos, Vector2.zero);
        hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        int layer = LayerMask.NameToLayer("Targetable");
        // 임시
        if (hit.collider != null && hit.collider.gameObject.layer == layer && cardMono_Magic.owner.IsMyTurn())
        {
            var _ITargetable = hit.collider.GetComponent<ITargetable>();
            if (((_ITargetable.GetTargetType() & cardMono_Magic.cardSO.magicTarget) != 0) && _ITargetable.CanBeTarget())
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator SelectTargetCoroutine()
    {
        Vector3 farZ = transform.position;
        farZ.z = 9999;
        cardMono_Magic.SetPR(farZ, Quaternion.identity, 0f);
        while (true)
        {
            yield return null;
            RaycastHit2D hit;
            bool _isTargetOn = IsTargetOn(out hit);
            cardMono_Magic.owner.gameManager.SetLineTarget(cardMono_Magic.gameObject.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), true, _isTargetOn);
            cardMono_Magic.Predict(_isTargetOn ? hit.collider.gameObject : null);
            if (!cardMono_Magic.owner.IsMyTurn())
            {
                GoBack();
                break;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (_isTargetOn)
                    cardMono_Magic.owner.RPC_UseMagicOfHand(cardMono_Magic.uniqueID, hit.collider.GetComponent<ITargetable>().GetNetworkId());
                else
                    GoBack();
                break;
            }
        }
        cardMono_Magic.isDragging = false;
        cardMono_Magic.Predict(null);
        cardMono_Magic.owner.gameManager.SetLineTarget(Vector3.zero, Vector3.zero, false, false);
    }

    // ~~인터페이스~~

    public void OnIsZoomingChanged()
    {
        if (cardMono_Magic.networkObject.HasInputAuthority) return;
        if (cardMono_Magic.isZooming || cardMono_Magic.isDragging) cardMono_Magic.GetBackFaceGlow().SetActive(true);
        else cardMono_Magic.GetBackFaceGlow().SetActive(false);
    }

    public void OnMyMouseDown()
    {
        if (!cardMono_Magic.networkObject.HasInputAuthority) return;
        cardMono_Magic.isDragging = true;
    }

    public void OnMyMouseUp()
    {
        if (!cardMono_Magic.networkObject.HasInputAuthority) return;
        if (!cardMono_Magic.isDragging) return;

        if (!cardMono_Magic.owner.IsMyTurn() || DraggingCardInMyHandArea())
        {
            GoBack();
            cardMono_Magic.isDragging = false;
        }
        else
        {
            if (cardMono_Magic.magic != null && cardMono_Magic.magic.IsNeedTarget())
            {
                if (cardMono_Magic.cardSO.IsTargetExist(CommandType.Magic, cardMono_Magic.owner.gameManager))
                    cardMono_Magic.StartCoroutine(SelectTargetCoroutine());
                else
                {
                    cardMono_Magic.owner.RPC_UseMagicOfHand(cardMono_Magic.uniqueID);
                    cardMono_Magic.isDragging = false;
                }
            }
            else
            {
                cardMono_Magic.owner.RPC_UseMagicOfHand(cardMono_Magic.uniqueID);
                cardMono_Magic.isDragging = false;
            }
        }
    }

    public void OnMyMouseDrag()
    {
        if (!cardMono_Magic.networkObject.HasInputAuthority) return;
        if (!cardMono_Magic.isDragging) return;

        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;

        transform.DOMove(_pos, 0);

        if (DraggingCardInMyHandArea())
        {
            cardMono_Magic.isZooming = true;
            cardMono_Magic.imageTr.DOScale(Vector3.one * 1.3f, 0.2f);
            cardMono_Magic.imageTr.DOLocalMove(new Vector3(0, 3f, -100f), 0f);
        }
        else
        {
            cardMono_Magic.isZooming = false;
            cardMono_Magic.imageTr.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.2f);
            cardMono_Magic.imageTr.DOLocalMove(Vector3.zero, 0f);
        }
    }

    public void OnMyMouseEnter()
    {
        if (!cardMono_Magic.networkObject.HasInputAuthority) return;
        if (cardMono_Magic.isDragging || cardMono_Magic.isZooming) return;
        cardMono_Magic.isZooming = true;
        cardMono_Magic.imageTr.DOScale(Vector3.one * 1.3f, 0.2f);
        cardMono_Magic.imageTr.DOLocalMove(new Vector3(0, 3f, -100f), 0f);

        transform.DORotateQuaternion(Quaternion.identity, 0.2f);
    }

    public void OnMyMouseExit()
    {
        if (!cardMono_Magic.networkObject.HasInputAuthority) return;
        if (cardMono_Magic.isDragging) return;
        cardMono_Magic.isZooming = false;
        cardMono_Magic.imageTr.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.2f);
        cardMono_Magic.imageTr.DOLocalMove(Vector3.zero, 0f);

        transform.DORotateQuaternion(cardMono_Magic.originRot, 0.2f);
    }
}
