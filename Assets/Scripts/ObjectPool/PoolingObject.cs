using UnityEngine;
using UnityEngine.Pool;

public class PoolingObject : MonoBehaviour
{
    IObjectPool<GameObject> _ManagedPool;

    public void SetManagedPool(IObjectPool<GameObject> pool)
    {
        _ManagedPool = pool;
    }

    public virtual void ReInit()
    {
        gameObject.SetActive(true);
    }

    public void DestroyObject()
    {
        _ManagedPool.Release(gameObject);
    }
}
