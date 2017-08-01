using UnityEngine;

public class ExplodeOnCollision : VoBehavior
{
    public PooledObject ExplosionPrefab;

    void Awake()
    {
        this.localNotifier.Listen(CollisionEvent.NAME, this, onCollide);
    }

    private void onCollide(LocalEventNotifier.Event e)
    {
        PooledObject explosion = this.ExplosionPrefab.Retain();
        explosion.transform.SetPosition2D(this.transform.position);
        explosion.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
        ObjectPools.Release(this.gameObject);
    }
}
