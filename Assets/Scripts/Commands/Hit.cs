using Fusion;
using UnityEngine;

public class Hit : ICommand
{
    int damage;

    public Hit(int _damage)
    {
        damage = _damage;
    }

    public int CountOfRandomTarget()
    {
        return 0;
    }

    public NetworkData[] DoAndGetRandomTarget(CardMono mine, NetworkId _target)
    {
        int _damage = -1;
        if (_target != default)
        {
            ITargetable targetHit = mine.owner.gameManager.GetNetworkObject(_target).GetComponent<ITargetable>();
            if (targetHit != null)
            {
                _damage = targetHit.Hit(damage);
                //targetHit.RPC_Hit(damage);
            }
        }

        return new NetworkData[1] { new NetworkData(_target, _damage) };
    }

    public void Execute(CardMono mine, NetworkId target)
    {
        if (target == default) return;
        ITargetable targetHit = mine.owner.gameManager.GetNetworkObject(target).GetComponent<ITargetable>();
        if (targetHit == null) return;
        int _damage = targetHit.Hit(damage);
        targetHit.UpdateHit(_damage);
    }

    public bool IsNeedTarget()
    {
        return true;
    }
}
