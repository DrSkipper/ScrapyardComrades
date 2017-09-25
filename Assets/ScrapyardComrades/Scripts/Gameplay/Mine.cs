using UnityEngine;
using System.Collections.Generic;

public class Mine : VoBehavior, IPausable
{
    public LayerMask DamagableLayers;
    public SCAttack.HitData HitData;
    public Damagable Damagable;
    public PooledObject FlashEffect;
    public PooledObject ExplosionEffect;
    public Transform ExplosionLocation;
    public AudioClip ExplosionSound;
    public bool TriggerOnCollision = true;

    void Awake()
    {
        _nearbyColliders = new List<IntegerCollider>();
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHit);
    }

    void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();

        if (this.TriggerOnCollision)
        {
            gatherNearbyColliders();
            _framesUntilColliderGet = FRAME_OFFSET % FRAMES_BETWEEN_COLLIDER_GET;
            FRAME_OFFSET = FRAME_OFFSET >= 10000 ? 0 : FRAME_OFFSET + 1;
        }
    }

    void FixedUpdate()
    {
        if (this.TriggerOnCollision)
        {
            --_framesUntilColliderGet;
            if (_framesUntilColliderGet < 0)
                gatherNearbyColliders();

            GameObject collided = this.integerCollider.CollideFirst(0, 0, this.DamagableLayers, null, _nearbyColliders);
            if (collided != null)
            {
                Damagable damagable = collided.GetComponent<Damagable>();
                if (damagable != null)
                {
                    Actor2D actor = damagable.GetComponent<Actor2D>();
                    float vx = actor == null ? 0.0f : actor.Velocity.x;

                    damagable.Damage(this.HitData, (Vector2)this.transform.position, (Vector2)this.transform.position, vx > 0.0f ? SCCharacterController.Facing.Left : SCCharacterController.Facing.Right);

                    explode();
                }
            }
        }
    }

    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }

    /**
     * Private
     */
    private List<IntegerCollider> _nearbyColliders;
    private int _framesUntilColliderGet;

    private static int FRAME_OFFSET = 0;
    private const int ENLARGE_AMT = 40;
    private const int FRAMES_BETWEEN_COLLIDER_GET = 3;

    private void explode()
    {
        PooledObject flash = this.FlashEffect.Retain();
        Explosion exp = flash.GetComponent<Explosion>();
        if (exp != null)
            exp.HitData = this.HitData;
        flash.transform.position = this.transform.position;
        flash.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);


        PooledObject explosion = this.ExplosionEffect.Retain();
        explosion.transform.position = this.ExplosionLocation.position;
        explosion.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);

        if (this.ExplosionSound != null)
            SoundManager.Play(this.ExplosionSound.name);

        this.GetComponent<WorldEntity>().TriggerConsumption();
    }

    private void onHit(LocalEventNotifier.Event e)
    {
        if (this.Damagable.Dead)
        {
            explode();
        }
    }

    private void gatherNearbyColliders()
    {
        this.integerCollider.GetPotentialCollisions(0, 0, 0, 0, this.DamagableLayers, _nearbyColliders, ENLARGE_AMT, ENLARGE_AMT);
        for (int i = 0; i < _nearbyColliders.Count; ++i)
        {
            if (_nearbyColliders[i] == this.integerCollider)
            {
                _nearbyColliders.RemoveAt(i);
                break;
            }
        }
        _framesUntilColliderGet = FRAMES_BETWEEN_COLLIDER_GET;
    }
}
