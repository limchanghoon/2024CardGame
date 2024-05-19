using Fusion;

public interface ICommand
{
    // Execute => Execute 내부에서 ITargetable.RPC_Command 실행 => ITargetable.RPC_Command 내부에서 ICommand.ExecuteInRPC 실행
    void Execute(CardMono mine, NetworkId target);
    void RPC_Execute(NetworkObject target);
    bool IsNeedTarget();
}
