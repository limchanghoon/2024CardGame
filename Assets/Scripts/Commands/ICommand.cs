using Fusion;

public interface ICommand
{
    // 현재 턴인 유저만 Execute 수행 => Execute 내부에서 RPC_Execute 실행
    void Execute(CardMono mine, NetworkId target);
    void RPC_Execute(NetworkObject target);
    bool IsNeedTarget();
}
