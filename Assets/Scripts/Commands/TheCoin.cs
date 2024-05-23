using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheCoin : NetworkBehaviour, ICommand
{
    [SerializeField] int amount;

    public void Execute(CardMono mine, NetworkId target, CommandType _commandType)
    {
        if (!mine.owner.IsMyTurn()) return;
        RPC_Execute(mine.owner.networkObject);

        // ав╦ч!
        mine.owner.gameManager.DoDeathRattleOneLayer();
        mine.owner.gameManager.RPC_EnqueueChangeField();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Execute(NetworkObject _target)
    {
        _target.GetComponent<Player>().UpCurrentCrystal(amount);
    }

    public bool IsNeedTarget()
    {
        return false;
    }
}
