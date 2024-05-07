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

    public void ExecuteInRPC(ITargetable targetHit)
    {

    }

    public bool IsNeedTarget()
    {
        return false;
    }
}
