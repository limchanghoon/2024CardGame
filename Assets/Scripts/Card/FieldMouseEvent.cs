using DG.Tweening.Core.Easing;
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
        int layer = LayerMask.NameToLayer("FieldCard");
        // юс╫ц
        if (hit.collider != null && hit.collider.gameObject.layer == layer && cardMono.owner.gameManager.IsMyTurn())
        {
            CardMono _target = hit.collider.GetComponent<CardMono>();
            if (!_target.networkObject.HasInputAuthority && _target.currentHealth > 0)
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
        cardMono.isDragging = true;
        cardMono.owner.gameManager.DisalbeFieldCardTooltip();
    }

    public void OnMyMouseUp()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (!cardMono.isDragging) return;
        cardMono.isDragging = false;
        cardMono.owner.gameManager.SetLineTarget(Vector3.zero, Vector3.zero, false, false);

        RaycastHit2D hit;
        if (IsTargetOn(out hit))
        {
            cardMono.owner.gameManager.DisalbeFieldCardTooltip();
            CardMono targetCard = hit.collider.gameObject.GetComponent<CardMono>();
            cardMono.RPC_Attack(targetCard.uniqueID);
        }
    }

    public void OnMyMouseDrag()
    {
        if (!cardMono.networkObject.HasInputAuthority) return;
        if (!cardMono.isDragging) return;
        RaycastHit2D hit;
        cardMono.owner.gameManager.SetLineTarget(cardMono.gameObject.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), true, IsTargetOn(out hit));
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
