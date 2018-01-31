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
    public TurretController.AttachDir AttachedAt = TurretController.AttachDir.Down;

    void Awake()
    {
        _ourCollider = this.GetComponent<IntegerRectCollider>();
        _nearbyColliders = new List<IntegerCollider>();
        _defaultColliderOffset = _ourCollider.Offset;
        _defaultColliderSize = _ourCollider.Size;
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHit);
    }

    void OnSpawn()
    {
        switch (this.AttachedAt)
        {
            default:
            case TurretController.AttachDir.Down:
                _normal = Vector2.up;
                this.transform.rotation = Quaternion.Euler(0, 0, 0);
                _ourCollider.Offset = _defaultColliderOffset;
                _ourCollider.Size = _defaultColliderSize;
                break;
            case TurretController.AttachDir.Up:
                _normal = Vector2.down;
                this.transform.rotation = Quaternion.Euler(0, 0, 180);
                _ourCollider.Offset = new IntegerVector(_defaultColliderOffset.X, -_defaultColliderOffset.Y - 1);
                _ourCollider.Size = _defaultColliderSize;
                break;
            case TurretController.AttachDir.Left:
                _normal = Vector2.right;
                this.transform.rotation = Quaternion.Euler(0, 0, -90);
                _ourCollider.Offset = new IntegerVector(_defaultColliderOffset.Y, -_defaultColliderOffset.X);
                _ourCollider.Size = new IntegerVector(_defaultColliderSize.Y, _defaultColliderSize.X);
                break;
            case TurretController.AttachDir.Right:
                _normal = Vector2.left;
                this.transform.rotation = Quaternion.Euler(0, 0, 90);
                _ourCollider.Offset = new IntegerVector(-_defaultColliderOffset.Y, _defaultColliderOffset.X);
                _ourCollider.Size = new IntegerVector(_defaultColliderSize.Y, _defaultColliderSize.X);
                break;
        }

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
    private IntegerRectCollider _ourCollider;
    private List<IntegerCollider> _nearbyColliders;
    private int _framesUntilColliderGet;
    private Vector2 _normal;
    private IntegerVector _defaultColliderOffset;
    private IntegerVector _defaultColliderSize;

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
