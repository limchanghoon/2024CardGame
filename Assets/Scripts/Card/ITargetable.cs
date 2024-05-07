using Fusion;
using UnityEngine;

public interface ITargetable
{
    void SetActivePrediction(bool _active);
    bool DieIfHit(int damage);
    void RPC_Command(NetworkObject _networkObject);
    void Hit(int damage);
    bool CheckIsFirstDie();
    int PredictHit(int damage);
    void UpdateHit(int damage);
    void Die();
    NetworkId GetNetworkId();
    TargetType GetTargetType();
    bool CanBeTarget();
    bool CanBeDirectAttackTarget();
    GameObject GetTargetGameObject();

    int currentPower { get; set; }
    int currentHealth { get; set; }
}
