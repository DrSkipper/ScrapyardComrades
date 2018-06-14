using UnityEngine;

public class ExplodeOnCollision : VoBehavior
{
    public PooledObject ExplosionPrefab;
    public PooledObject AdditionalPrefab;
    public SCAttack.HitData HitData;
    public SoundData.Key ExplosionSfxKey;

    void Awake()
    {
        this.localNotifier.Listen(CollisionEvent.NAME, this, onCollide);
    }

    void OnReturnToPool()
    {
        _origin = null;
    }

    public void SetOriginObject(GameObject origin)
    {
        _origin = origin;
    }

    /**
     * Private
     */
    private GameObject _origin;

    private void onCollide(LocalEventNotifier.Event e)
    {
        if (_origin != null)
        {
            CollisionEvent ce = e as CollisionEvent;
            if (ce.Hits.Count == 1 && ce.Hits[0] == _origin)
                return;
        }

        PooledObject explosion = this.ExplosionPrefab.Retain();
        Explosion explosionComponent = explosion.GetComponent<Explosion>();
        if (explosionComponent != null)
            explosionComponent.HitData = this.HitData;
        explosion.transform.SetPosition2D(this.transform.position);
        explosion.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);

        if (this.AdditionalPrefab != null)
        {
            explosion = this.AdditionalPrefab.Retain();
            explosion.transform.SetPosition2D(this.transform.position);
            explosion.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
        }

        SoundManager.Play(this.ExplosionSfxKey, explosion.transform);
        ObjectPools.Release(this.gameObject);
    }
}
