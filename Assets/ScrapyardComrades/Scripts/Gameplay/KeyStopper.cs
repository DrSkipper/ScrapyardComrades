using UnityEngine;

public class KeyStopper : VoBehavior
{
    public Door Door;

    void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();
    }

    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }

    public void HandleStop()
    {
        //TODO: Effect to indicate key is stopped
    }
}
