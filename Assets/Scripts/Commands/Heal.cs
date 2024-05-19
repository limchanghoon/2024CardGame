using Fusion;
using UnityEngine;


public class Heal : NetworkBehaviour, ICommand
{
    int amount;

    public Heal(int _amount)
    {
        amount = _amount;
    }

    public int CountOfRandomTarget()
    {
        return 0;
    }

    public void Execute(CardMono mine, NetworkId target)
    {

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Execute(NetworkObject _target)
    {

    }

    public bool IsNeedTarget()
    {
        return true;
    }
}
