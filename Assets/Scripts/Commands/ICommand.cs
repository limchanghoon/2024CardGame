using Fusion;
using UnityEngine;

public interface ICommand
{
    void Execute(CardMono mine, NetworkId target);
    bool IsNeedTarget();
}
