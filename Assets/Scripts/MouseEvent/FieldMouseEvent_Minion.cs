using UnityEngine;

public class FieldMouseEvent_Minion : IMyMouseEvent
{
    CardMono_Minion cardMono_Minion;

    public FieldMouseEvent_Minion(CardMono_Minion _cardMono)
    {
        cardMono_Minion = _cardMono;
    }

    private bool IsTargetOn(out RaycastHit2D hit)
    {
        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;
        Ray2D ray = new Ray2D(_pos, Vector2.zero);
        hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        int layer = LayerMask.NameToLayer("Targetable");
        // юс╫ц
        if (hit.collider != null && hit.collider.gameObject.layer == layer && cardMono_Minion.owner.IsMyTurn())
        {
            var _ITargetable = hit.collider.GetComponent<ITargetable>();
            if (((_ITargetable.GetTargetType() & TargetType.Opponent) != 0) && _ITargetable.CanBeDirectAttackTarget())
            {
                return true;
            }
        }
        return false;
    }

    public void OnIsZoomingChanged()
    {

    }

    public void OnMyMouseDown()
    {
        if (!cardMono_Minion.networkObject.HasStateAuthority) return;
        if (!cardMono_Minion.owner.IsMyTurn()) return;
        if (!cardMono_Minion.canAttack) return;
        cardMono_Minion.isDragging = true;
        cardMono_Minion.owner.gameManager.DisalbeFieldCardTooltip();
    }

    public void OnMyMouseUp()
    {
        if (!cardMono_Minion.networkObject.HasStateAuthority) return;
        if (!cardMono_Minion.isDragging) return;
        if (!cardMono_Minion.owner.IsMyTurn()) return;
        if (!cardMono_Minion.canAttack) return;
        cardMono_Minion.isDragging = false;
        cardMono_Minion.owner.gameManager.SetLineTarget(Vector3.zero, Vector3.zero, false, false);
        cardMono_Minion.Predict(null);

        RaycastHit2D hit;
        if (IsTargetOn(out hit))
        {
            cardMono_Minion.owner.gameManager.DisalbeFieldCardTooltip();
            var _networkObject = hit.collider.gameObject.GetComponent<ITargetable>();
            if (_networkObject == null) return;
            cardMono_Minion.RPC_Attack(_networkObject.GetNetworkId());
        }
    }

    public void OnMyMouseDrag()
    {
        if (!cardMono_Minion.networkObject.HasStateAuthority) return;
        if (!cardMono_Minion.isDragging) return;
        if (!cardMono_Minion.owner.IsMyTurn()) return;
        if (!cardMono_Minion.canAttack) return;
        RaycastHit2D hit;
        bool _isTargetOn = IsTargetOn(out hit);
        cardMono_Minion.owner.gameManager.SetLineTarget(cardMono_Minion.gameObject.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), true, _isTargetOn);
        cardMono_Minion.Predict(_isTargetOn ? hit.collider.gameObject : null);
    }

    public void OnMyMouseEnter()
    {
        if (cardMono_Minion.isDragging) return;
        cardMono_Minion.owner.gameManager.ShowFieldCardTooltip(cardMono_Minion, cardMono_Minion.transform.position);
    }

    public void OnMyMouseExit()
    {
        if (cardMono_Minion.isDragging) return;
        cardMono_Minion.owner.gameManager.DisalbeFieldCardTooltip();
    }
}
