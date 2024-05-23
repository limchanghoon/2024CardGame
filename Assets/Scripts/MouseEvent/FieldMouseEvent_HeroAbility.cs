using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldMouseEvent_HeroAbility : IMyMouseEvent
{
    HeroAbility heroAbility;
    Player player;

    public FieldMouseEvent_HeroAbility(HeroAbility _heroAbility, Player _player)
    {
        heroAbility = _heroAbility;
        player = _player;
    }


    private bool IsTargetOn(out RaycastHit2D hit)
    {
        Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pos.z = -100f;
        Ray2D ray = new Ray2D(_pos, Vector2.zero);
        hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        int layer = LayerMask.NameToLayer("Targetable");
        // юс╫ц
        if (hit.collider != null && hit.collider.gameObject.layer == layer && player.IsMyTurn())
        {
            var _ITargetable = hit.collider.GetComponent<ITargetable>();
            if ((_ITargetable.GetTargetType() & heroAbility.GetTargetType()) != 0)
            {
                return true;
            }
        }
        return false;
    }



    public void OnIsZoomingChanged()
    {
        return;
    }

    public void OnMyMouseDown()
    {
        if (!player.networkObject.HasStateAuthority) return;
        if (!player.IsMyTurn()) return;
        if (heroAbility.IsNeedTarget()) return;
        if (heroAbility.count <= 0) return;

        heroAbility.Execute(null, default, CommandType.HeroAbility);

        return;
    }

    public void OnMyMouseDrag()
    {
        if (!player.networkObject.HasStateAuthority) return;
        if (!player.IsMyTurn()) return;
        if (!heroAbility.IsNeedTarget()) return;
        if (heroAbility.count <= 0) return;

        RaycastHit2D hit;
        bool _isTargetOn = IsTargetOn(out hit);
        player.gameManager.SetLineTarget(heroAbility.gameObject.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), true, _isTargetOn);
        heroAbility.Predict(_isTargetOn ? hit.collider.gameObject : null);

        return;
    }

    public void OnMyMouseUp()
    {
        if (!player.networkObject.HasStateAuthority) return;
        if (!player.IsMyTurn()) return;
        if (!heroAbility.IsNeedTarget()) return;
        if (heroAbility.count <= 0) return;

        player.gameManager.SetLineTarget(Vector3.zero, Vector3.zero, false, false);
        heroAbility.Predict(null);

        RaycastHit2D hit;
        if (IsTargetOn(out hit))
        {
            player.gameManager.DisalbeFieldCardTooltip();
            var _networkObject = hit.collider.gameObject.GetComponent<ITargetable>();
            if (_networkObject == null) return;
            heroAbility.Execute(null, _networkObject.GetNetworkId(), CommandType.Magic);
        }

        return;
    }

    public void OnMyMouseEnter()
    {
        return;
    }

    public void OnMyMouseExit()
    {
        return;
    }
}
