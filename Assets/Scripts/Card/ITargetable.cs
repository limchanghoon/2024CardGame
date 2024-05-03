
using Fusion;

public interface ITargetable
{
    int Hit(int damage);
    void UpdateHit(int damage);
    void Die();
    NetworkId GetNetworkId();
    TargetType GetTargetType();
    bool CanBeTarget();

    int currentPower { get; set; }
    int currentHealth { get; set; }
}
