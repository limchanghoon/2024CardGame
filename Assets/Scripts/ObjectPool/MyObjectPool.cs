using UnityEngine;
using UnityEngine.Pool;

public class MyObjectPool : MonoBehaviour
{
    //public ObjectPoolType objectPoolType;
    [SerializeField, Min(1)] int myMaxSize;
    [SerializeField] Transform objectParent;
    [SerializeField] GameObject objectPrefab;
    IObjectPool<GameObject> pool;

    protected virtual void Awake()
    {
        pool = new ObjectPool<GameObject>(OnCreateObject, OnGetObject, OnReleaseObject, OnDestoryObject, maxSize: myMaxSize);

        GameObject[] poolingObjects = new GameObject[myMaxSize];
        for (int i = 0; i < myMaxSize; i++)
        {
            poolingObjects[i] = CreateOjbect();
        }

        for (int i = 0; i < myMaxSize; i++)
        {
            poolingObjects[i].GetComponent<PoolingObject>().DestroyObject();
        }
    }

    public GameObject CreateOjbect()
    {
        return pool.Get();
    }

    GameObject OnCreateObject()
    {
        GameObject result = Instantiate(objectPrefab);
        PoolingObject myNewObject = result.GetComponent<PoolingObject>();
        myNewObject.SetManagedPool(pool);
        myNewObject.transform.SetParent(objectParent);
        return result;
    }

    void OnGetObject(GameObject myObject)
    {
        myObject.GetComponent<PoolingObject>().ReInit();
    }

    void OnReleaseObject(GameObject myObject)
    {
        myObject.SetActive(false);
    }

    void OnDestoryObject(GameObject myObject)
    {
        Destroy(myObject);
    }

    public GameObject GetObjectPrefab()
    {
        return objectPrefab;
    }
}
