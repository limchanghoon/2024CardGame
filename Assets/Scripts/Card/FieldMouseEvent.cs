using Fusion;
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

    private bool IsTargetOn(out RaycastHit2D hit)
    {
        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;
        Ray2D ray = new Ray2D(_pos, Vector2.zero);
        hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        int layer = LayerMask.NameToLayer("Targetable");
        // юс╫ц
        if (hit.collider != null && hit.collider.gameObject.layer == layer && cardMono.owner.IsMyTurn())
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
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (!cardMono.owner.IsMyTurn()) return;
        cardMono.isDragging = true;
        cardMono.owner.gameManager.DisalbeFieldCardTooltip();
    }

    public void OnMyMouseUp()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (!cardMono.isDragging) return;
        if (!cardMono.owner.IsMyTurn()) return;
        cardMono.isDragging = false;
        cardMono.owner.gameManager.SetLineTarget(Vector3.zero, Vector3.zero, false, false);
        cardMono.Predict(null);

        RaycastHit2D hit;
        if (IsTargetOn(out hit))
        {
            cardMono.owner.gameManager.DisalbeFieldCardTooltip();
            var _networkObject = hit.collider.gameObject.GetComponent<ITargetable>();
            if (_networkObject == null) return;
            cardMono.RPC_Attack(_networkObject.GetNetworkId());
        }
    }

    public void OnMyMouseDrag()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (!cardMono.isDragging) return;
        if (!cardMono.owner.IsMyTurn()) return;
        RaycastHit2D hit;
        bool _isTargetOn = IsTargetOn(out hit);
        cardMono.owner.gameManager.SetLineTarget(cardMono.gameObject.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), true, _isTargetOn);
        cardMono.Predict(_isTargetOn ? hit.collider.gameObject : null);
    }

    public void OnMyMouseEnter()
    {
        if (cardMono.isDragging) return;
        cardMono.owner.ShowFieldCardTooltip(cardMono);
    }

    public void OnMyMouseExit()
    {
        if (cardMono.isDragging) return;
        cardMono.owner.gameManager.DisalbeFieldCardTooltip();
    }
}
