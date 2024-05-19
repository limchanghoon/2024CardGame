using Fusion;

public interface ICommand
{
    // Execute => Execute ���ο��� ITargetable.RPC_Command ���� => ITargetable.RPC_Command ���ο��� ICommand.ExecuteInRPC ����
    void Execute(CardMono mine, NetworkId target);
    void RPC_Execute(NetworkObject target);
    bool IsNeedTarget();
}
