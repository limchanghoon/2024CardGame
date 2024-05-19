using Fusion;
using UnityEngine;


public class DrawCard : NetworkBehaviour, ICommand
{
    [SerializeField] int amount;

    public void Execute(CardMono mine, NetworkId target)
    {
        if (!mine.owner.IsMyTurn()) return;
        for (int i = 0; i < amount; i++)
        {
            mine.owner.DrawMyCard();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Execute(NetworkObject _target)
    {

    }

    public bool IsNeedTarget()
    {
        return false;
    }
}
