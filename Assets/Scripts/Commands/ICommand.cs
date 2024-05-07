using Fusion;

public interface ICommand
{
    void Execute(CardMono mine, NetworkId target);
    void ExecuteInRPC(ITargetable targetHit);
    bool IsNeedTarget();
}
