using UnityEngine;
using System.Collections.Generic;

public class Mine : VoBehavior, IPausable
{
    public LayerMask DamagableLayers;
    public LayerMask BlockingLayers;
    public SCAttack.HitData HitData;
    public Damagable Damagable;
    public PooledObject FlashEffect;
    public PooledObject ExplosionEffect;
    public Transform ExplosionLocation;
    public AudioClip ExplosionSound;
    public SoundData.Key ExplosionSfxKey;
    public bool TriggerOnCollision = true;
    public TurretController.AttachDir AttachedAt = TurretController.AttachDir.Down;
    public DestructionStyle DestructionType = DestructionStyle.Destroy;
    public int CooldownDuration = 50;
    public Color CooldownColor;
    public bool AttachToSurfaces = true;
    public LayerMask SurfaceLayers;

    public enum DestructionStyle
    {
        Destroy,
        Cooldown
    }

    void Awake()
    {
        _animator = this.GetComponent<SCSpriteAnimator>();
        _ourCollider = this.GetComponent<IntegerRectCollider>();
        _nearbyColliders = new List<IntegerCollider>();
        _defaultColliderOffset = _ourCollider.Offset;
        _defaultColliderSize = _ourCollider.Size;
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHit);
        _cooldownTimer = new Timer(this.CooldownDuration, false, true, onCooldownComplete);
        _cooldownTimer.complete();
    }

    void OnSpawn()
    {
        _hasAttached = !this.AttachToSurfaces;
        if (!_cooldownTimer.Completed)
            _cooldownTimer.complete();

        switch (this.AttachedAt)
        {
            default:
            case TurretController.AttachDir.Down:
                this.HitData.KnockbackDirection = VectorExtensions.Direction16.UpRight;
                this.transform.rotation = Quaternion.Euler(0, 0, 0);
                _ourCollider.Offset = _defaultColliderOffset;
                _ourCollider.Size = _defaultColliderSize;
                break;
            case TurretController.AttachDir.Up:
                this.HitData.KnockbackDirection = VectorExtensions.Direction16.DownRight;
                this.transform.rotation = Quaternion.Euler(0, 0, 180);
                _ourCollider.Offset = new IntegerVector(_defaultColliderOffset.X, -_defaultColliderOffset.Y - 1);
                _ourCollider.Size = _defaultColliderSize;
                break;
            case TurretController.AttachDir.Left:
                this.HitData.KnockbackDirection = VectorExtensions.Direction16.Right;
                this.transform.rotation = Quaternion.Euler(0, 0, -90);
                _ourCollider.Offset = new IntegerVector(_defaultColliderOffset.Y, -_defaultColliderOffset.X);
                _ourCollider.Size = new IntegerVector(_defaultColliderSize.Y, _defaultColliderSize.X);
                break;
            case TurretController.AttachDir.Right:
                this.HitData.KnockbackDirection = VectorExtensions.Direction16.Left;
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
        if (!_hasAttached)
            attachToSurface();

        if (!_cooldownTimer.Completed)
            _cooldownTimer.update();

        if (this.TriggerOnCollision && _cooldownTimer.Completed)
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
                    IntegerVector pos = this.integerPosition;
                    Vector2 diff = damagable.integerPosition - pos;

                    // Make sure we don't hurt though walls
                    if (!CollisionManager.RaycastFirst(pos, diff.normalized, diff.magnitude, this.BlockingLayers).Collided)
                    {
                        Actor2D actor = damagable.GetComponent<Actor2D>();
                        float vx = actor == null ? 0.0f : actor.Velocity.x;

                        damagable.Damage(this.HitData, (Vector2)this.transform.position, (Vector2)this.transform.position, vx > 0.0f ? SCCharacterController.Facing.Left : SCCharacterController.Facing.Right);
                        explode();
                    }
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
    private SCSpriteAnimator _animator;
    private IntegerRectCollider _ourCollider;
    private List<IntegerCollider> _nearbyColliders;
    private int _framesUntilColliderGet;
    private IntegerVector _defaultColliderOffset;
    private IntegerVector _defaultColliderSize;
    private Timer _cooldownTimer;
    private bool _hasAttached;

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
        
        SoundManager.Play(this.ExplosionSfxKey);
        
        switch (this.DestructionType)
        {
            default:
            case DestructionStyle.Destroy:
                WorldEntity entity = this.GetComponent<WorldEntity>();
                if (entity != null)
                    entity.TriggerConsumption();
                else
                    ObjectPools.Release(this.gameObject);
                break;
            case DestructionStyle.Cooldown:
                this.spriteRenderer.color = this.CooldownColor;
                _animator.GoToFrame(0);
                _animator.Stop();
                _cooldownTimer.resetAndStart();
                break;
        }
    }

    private void onHit(LocalEventNotifier.Event e)
    {
        if (this.Damagable.Dead && _cooldownTimer.Completed)
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

    private void onCooldownComplete()
    {
        this.spriteRenderer.color = Color.white;

        if (_animator != null)
            _animator.Play();

        if (this.Damagable != null)
            this.Damagable.ResetDamagable();
    }

    private void attachToSurface()
    {
        _hasAttached = true;
        int offsetX = 0;
        int offsetY = 0;

        switch (this.AttachedAt)
        {
            default:
            case TurretController.AttachDir.Down:
                offsetY = -1;
                break;
            case TurretController.AttachDir.Left:
                offsetX = -1;
                break;
            case TurretController.AttachDir.Up:
                offsetY = 2;
                break;
            case TurretController.AttachDir.Right:
                offsetX = 1;
                break;
        }

        GameObject surface = _ourCollider.CollideFirst(offsetX, offsetY, this.SurfaceLayers);
        if (surface != null)
        {
            this.transform.SetParent(surface.transform);
        }
    }
}
