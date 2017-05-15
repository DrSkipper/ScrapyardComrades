using UnityEngine;
using System.Collections.Generic;

public class ObjectPools : MonoBehaviour
{
    public PooledObject[] PrefabsToPool;
    public PrefabCollection[] PrefabCollections;

    void Awake()
    {
        _instance = this;
        int totalCount = getTotalPrefabsCount();
        _pools = new List<PooledObject>[totalCount];
        int i = 0;

        if (this.PrefabsToPool != null)
        {
            for (; i < this.PrefabsToPool.Length; ++i)
            {
                // Set pool ids, create pools
                PooledObject prefab = this.PrefabsToPool[i];
                prefab.PoolId = i;
                _pools[i] = new List<PooledObject>(prefab.MaxToStore);

                // Preload
                while (_pools[i].Count < prefab.MaxToStore)
                {
                    returnObject(_pools[i], instantiate(prefab), false);
                }
            }
        }

        if (this.PrefabCollections != null)
        {
            for (int j = 0; j < this.PrefabCollections.Length; ++j)
            {
                for (int p = 0; p < this.PrefabCollections[j].Prefabs.Count; ++p)
                {
                    // Set pool ids, create pools
                    PooledObject prefab = this.PrefabCollections[j].Prefabs[p];
                    prefab.PoolId = i;
                    _pools[i] = new List<PooledObject>(prefab.MaxToStore);

                    // Preload
                    while (_pools[i].Count < prefab.MaxToStore)
                    {
                        returnObject(_pools[i], instantiate(prefab), false);
                    }
                    ++i;
                }
            }
        }
    }

    public static PooledObject Retain(PooledObject prefab)
    {
        if (_instance != null)
            return _instance.retain(prefab);
        Debug.LogWarning("No ObjectPools instance exists, cannot retain " + prefab);
        return instantiate(prefab);
    }

    public static void Release(GameObject toRelease)
    {
        PooledObject pooledObject = toRelease.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            Debug.LogWarning("No PooledObject script found on " + toRelease);
            Destroy(toRelease.gameObject);
        }
        else
        {
            Release(pooledObject);
        }
    }

    public static void Release(PooledObject toRelease)
    {
        if (_instance != null)
            _instance.release(toRelease);
        else
            Debug.LogWarning("No ObjectPools instance exists, cannot release " + toRelease);
    }

    /**
     * Private
     */
    private static ObjectPools _instance;
    private List<PooledObject>[] _pools;

    private int getTotalPrefabsCount()
    {
        int c = 0;
        if (this.PrefabsToPool != null)
        {
            c += this.PrefabsToPool.Length;
        }
        if (this.PrefabCollections != null)
        {
            for (int i = 0; i < this.PrefabCollections.Length; ++i)
                c += this.PrefabCollections[i].Prefabs.Count;
        }
        return c;
    }

    private PooledObject retain(PooledObject prefab)
    {
        int poolId = prefab.PoolId;
        if (poolId < 0 || poolId >= _pools.Length)
        {
            Debug.LogWarning("No pool found with id " + poolId + ", specified on prefab " + prefab);
        }
        else
        {
            List<PooledObject> pool = _pools[poolId];
            if (pool.Count > 0)
            {
                PooledObject instance = pool.Pop();
                instance.gameObject.SetActive(true);
                return instance;
            }
            Debug.LogWarning("No pooled instances for " + prefab.gameObject.name + " when attempting to retain");
        }

        return instantiate(prefab);
    }

    private void release(PooledObject toRelease)
    {
        int poolId = toRelease.PoolId;
        if (poolId < 0 || poolId >= _pools.Length)
        {
            Debug.LogWarning("No pool found with id " + poolId + ", specified on object " + toRelease);
        }
        else
        {
            if (returnObject(_pools[poolId], toRelease))
                return;
        }

        Destroy(toRelease.gameObject);
    }

    private static PooledObject instantiate(PooledObject prefab)
    {
        return Instantiate<PooledObject>(prefab);
    }

    private bool returnObject(List<PooledObject> pool, PooledObject obj, bool broadcastMessage = true)
    {
        if (broadcastMessage)
            obj.BroadcastMessage(POOL_RETURN_METHOD, SendMessageOptions.DontRequireReceiver);

        if (pool.Count < pool.Capacity)
        {
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(false);
            pool.Add(obj);
            return true;
        }
        Debug.LogWarning("Over pool capacity for " + obj.gameObject.name + " when attempting to return to pool");
        return false;
    }

    private const string POOL_RETURN_METHOD = "OnReturnToPool";
}
