using DG.Tweening;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] MyObjectPool dieEffectPool;
    [SerializeField] MyObjectPool BattleCryEffectPool;
    [SerializeField] MyObjectPool DeathRattleEffectPool;

    public void DoDieEffect(Vector3 _pos)
    {
        _pos.z = -300;
        dieEffectPool.CreateOjbect().transform.DOMove(_pos, 0);
    }

    public void DoDeathRattleEffect(Vector3 _pos)
    {
        _pos.z = -300;
        DeathRattleEffectPool.CreateOjbect().transform.DOMove(_pos, 0);
    }
}
