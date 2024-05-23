using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarlockAbility : HeroAbility
{
    [SerializeField] int damage;
    [SerializeField] int amount;

    public override void Spawned()
    {
        base.Spawned();
        currentMouseEvent = new FieldMouseEvent_HeroAbility(this, OwnerPlayer.GetComponent<Player>());
    }

    public override void Execute(CardMono mine, NetworkId target, CommandType _commandType)
    {
        if (!myPlayer.IsMyTurn()) return;
        if (!myPlayer.IsCrystalEnough(cost)) return;
        if (!DecreaseCount()) return;
        myPlayer.RPC_UseCrystal(cost);
        for (int i = 0; i < amount; i++)
        {
            myPlayer.DrawMyCard();
        }
        RPC_Execute(myPlayer.networkObject);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public override void RPC_Execute(NetworkObject _target)
    {
        ITargetable targetHit = _target.GetComponent<ITargetable>();
        int _damage = targetHit.PredictHit(damage);
        GameManager.actionQueue.Enqueue(() => { targetHit.UpdateHit(_damage); GameManager.isAction = false; });
        targetHit.Hit(damage);
        targetHit.CheckIsFirstDie();
    }

    public override bool IsNeedTarget()
    {
        return false;
    }
}
