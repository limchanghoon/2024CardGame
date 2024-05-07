using Fusion;
using UnityEngine;


public class Heal : ICommand
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

    public void ExecuteInRPC(ITargetable targetHit)
    {
        throw new System.NotImplementedException();
    }

    public bool IsNeedTarget()
    {
        return true;
    }
}
