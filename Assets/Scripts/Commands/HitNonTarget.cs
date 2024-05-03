using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class HitNonTarget : ICommand
{
    int damage;
    int randomCount;

    public HitNonTarget(int _damage, int _randomCount)
    {
        damage = _damage;
        randomCount = _randomCount;
    }

    public int CountOfRandomTarget()
    {
        return randomCount;
    }

    //public NetworkData[] DoAndGetRandomTarget(CardMono mine, NetworkId _target)
    //{
    //}

    public void Execute(CardMono mine, NetworkId target)
    {

    }

    public bool IsNeedTarget()
    {
        return false;
    }
}
