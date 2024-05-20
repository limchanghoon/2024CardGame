using Fusion;

public interface ICommand
{
    // ���� ���� ������ Execute ���� => Execute ���ο��� RPC_Execute ����
    void Execute(CardMono mine, NetworkId target);
    void RPC_Execute(NetworkObject target);
    bool IsNeedTarget();
}
