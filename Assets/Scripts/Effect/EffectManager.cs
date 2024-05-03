using DG.Tweening;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] MyObjectPool pool;

    public void DoHitEffect(Vector3 _pos)
    {
        _pos.z = -300;
        pool.CreateOjbect().transform.DOMove(_pos, 0);
    }
}
