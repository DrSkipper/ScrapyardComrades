using UnityEngine;
using System.Collections.Generic;

public class Mine : VoBehavior, IPausable
{
    public LayerMask DamagableLayers;
    public SCAttack.HitData HitData;
    public PooledObject FlashEffect;
    public PooledObject ExplosionEffect;
    public Transform ExplosionLocation;
    public AudioClip ExplosionSound;

    void Awake()
    {
        _nearbyColliders = new List<IntegerCollider>();
    }

    void OnSpawn()
    {
        gatherNearbyColliders();
        _framesUntilColliderGet = FRAME_OFFSET % FRAMES_BETWEEN_COLLIDER_GET;
        FRAME_OFFSET = FRAME_OFFSET >= 10000 ? 0 : FRAME_OFFSET + 1;
    }

    void FixedUpdate()
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
        flash.transform.position = this.transform.position;
        flash.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);

        PooledObject explosion = this.ExplosionEffect.Retain();
        explosion.transform.position = this.ExplosionLocation.position;
        explosion.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);

        if (this.ExplosionSound != null)
            SoundManager.Play(this.ExplosionSound.name);

        this.GetComponent<WorldEntity>().TriggerConsumption();
    }

    private void gatherNearbyColliders()
    {
        this.integerCollider.GetPotentialCollisions(0, 0, 0, 0, this.DamagableLayers, _nearbyColliders, ENLARGE_AMT, ENLARGE_AMT);
        _framesUntilColliderGet = FRAMES_BETWEEN_COLLIDER_GET;
    }
}
