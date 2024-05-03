using DG.Tweening.Core.Easing;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCard : ICommand
{
    int amount;

    public DrawCard(int _amount)
    {
        amount = _amount;
    }

    public void Execute(CardMono mine, NetworkId target)
    {
        if (!mine.HasInputAuthority) return;
        for (int i = 0; i < amount; i++)
        {
            mine.owner.DrawMyCard();
        }
    }

    public bool IsNeedTarget()
    {
        throw new System.NotImplementedException();
    }
}
