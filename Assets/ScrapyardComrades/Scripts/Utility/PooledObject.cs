using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector]
    public int PoolId = 0; // Set automatically
    public int MaxToStore = 128;

    public PooledObject Retain()
    {
        return ObjectPools.Retain(this);
    }

    //NOTE: Can instead directly call "ObjectPools.Release(GameObject)" if you don't have a reference to the PooledObject component.
    public void Release()
    {
        ObjectPools.Release(this);
    }
    
    /**
     * If we've been added as a child to another object that is now being released, separate from that object and release ourselves as well.
     */
    void OnReturnToPool()
    {
        if (this.transform.parent != null)
        {
            this.transform.SetParent(null);
            ObjectPools.Release(this);
        }
    }
}
