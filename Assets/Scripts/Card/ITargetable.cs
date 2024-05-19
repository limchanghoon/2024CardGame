using Fusion;
using UnityEngine;

public interface ITargetable
{
    void SetActivePrediction(bool _active);
    bool DieIfHit(int damage);
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

    public int currentPower { get; set; }
    public int currentHealth { get; set; }
}
