using UnityEngine;
using System.Collections.Generic;

public class Boulder : VoBehavior, IPausable
{
    public float Gravity = 10;
    public float MaxFallSpeed = 10;
    public float Friction = 7;
    public float AirFriction = 5;
    public float MinSpeedToDamage = 5;
    public float MinSpeedToDamageDown = 2;
    public float BounceVelocity = 5;
    public LayerMask MovingPlatformMask;
    public LayerMask DamagableLayers;
    public SCAttack.HitData HitParameters;
    public PooledObject HitEffect;
    public SoundData.Key LandSfxKey;

    private const int HIT_EFFECT_BORDER = 8;

    void Awake()
    {
        _damageRange = this.integerCollider.Bounds;
        _damageRange.Size += new IntegerVector(_damageRange.Size.X / 2, 0);
        this.localNotifier.Listen(CollisionEvent.NAME, this, onCollide);
    }

    void OnSpawn()
    {
        _grounded = true;
        this.integerCollider.AddToCollisionPool();

        if (_restingVelocityModifier == null)
            _restingVelocityModifier = new VelocityModifier(Vector2.zero, VelocityModifier.CollisionBehavior.sustain);
        else
            _restingVelocityModifier.Modifier = Vector2.zero;
        this.Actor.SetVelocityModifier(SCCharacterController.RESTING_VELOCITY_KEY, _restingVelocityModifier);
    }

    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }

    void FixedUpdate()
    {
        Vector2 velocity = this.Actor.Velocity;
        GameObject groundedAgainst = this.GroundedAgainst;

        // Check if we're on a moving platform
        _restingVelocityModifier.Modifier = Vector2.zero;
        if (groundedAgainst != null)
        {
            if (!_grounded)
            {
                SoundManager.Play(this.LandSfxKey, this.transform);
                _grounded = true;
            }

            int groundedLayerMask = 1 << groundedAgainst.layer;
            if ((groundedLayerMask & this.MovingPlatformMask) == groundedLayerMask)
            {
                attemptMovingPlatformAlignment(groundedAgainst);
            }
        }
        else
        {
            _grounded = false;
        }

        velocity.x = velocity.x.Approach(0.0f, groundedAgainst != null ? this.Friction : this.AirFriction);
        velocity.y = velocity.y.Approach(-this.MaxFallSpeed, this.Gravity);
        this.Actor.Velocity = velocity;
        _velocity = this.Actor.TotalVelocity;
    }

    public GameObject GroundedAgainst
    {
        get { return this.integerCollider.CollideFirst(0, -1, this.Actor.HaltMovementMask, null); }
    }

    /**
     * Private
     */
    private IntegerRect _damageRange;
    private VelocityModifier _restingVelocityModifier;
    private Vector2 _velocity;
    private bool _grounded;

    private void onCollide(LocalEventNotifier.Event e)
    {
        bool movingDown = _velocity.y < -this.MinSpeedToDamageDown;
        bool movingUp = !movingDown && _velocity.y >= this.MinSpeedToDamage;
        bool movingLeft = _velocity.x <= -this.MinSpeedToDamage;
        bool movingRight = !movingLeft && _velocity.x >= this.MinSpeedToDamage;

        if (movingDown || movingUp || movingLeft || movingRight)
        {
            CollisionEvent collisionEvent = e as CollisionEvent;
            for (int i = 0; i < collisionEvent.Hits.Count; ++i)
            {
                GameObject collided = collisionEvent.Hits[i];
                if (this.DamagableLayers.ContainsLayer(collided.layer))
                {
                    Damagable damagable = collided.GetComponent<Damagable>();

                    if (damagable != null)
                    {
                        if (movingDown && collided.transform.position.y < this.transform.position.y)
                        {
                            hit(damagable, (Vector2)damagable.transform.position, new Vector2(this.transform.position.x, this.transform.position.y + this.integerCollider.Offset.Y - this.integerCollider.Bounds.Size.Y / 2 + HIT_EFFECT_BORDER));

                            if (this.Actor.HaltMovementMask.ContainsLayer(collided.layer))
                                bounce();
                        }
                        else if (movingUp && collided.transform.position.y > this.transform.position.y)
                        {
                            hit(damagable, (Vector2)damagable.transform.position, new Vector2(this.transform.position.x, this.transform.position.y + this.integerCollider.Offset.Y + this.integerCollider.Bounds.Size.Y / 2 - HIT_EFFECT_BORDER));
                        }
                        else if (movingLeft && collided.transform.position.x < this.transform.position.x)
                        {
                            hit(damagable, (Vector2)damagable.transform.position, new Vector2(this.transform.position.x + this.integerCollider.Offset.X - this.integerCollider.Bounds.Size.X / 2 + HIT_EFFECT_BORDER, this.transform.position.y));
                        }
                        else if (movingRight && collided.transform.position.x > this.transform.position.x)
                        {
                            hit(damagable, (Vector2)damagable.transform.position, new Vector2(this.transform.position.x + this.integerCollider.Offset.X + this.integerCollider.Bounds.Size.X / 2 - HIT_EFFECT_BORDER, this.transform.position.y));
                        }
                    }
                }
            }
        }
    }

    private void hit(Damagable damagable, IntegerVector otherPos, IntegerVector colliderPos)
    {
        IntegerVector between = (otherPos + colliderPos) / 2;
        SCCharacterController.Facing facing = otherPos.X > colliderPos.X ? SCCharacterController.Facing.Right : SCCharacterController.Facing.Left;
        bool landedHit = damagable.Damage(this.HitParameters, (Vector2)this.transform.position, between, facing);

        if (landedHit && this.HitParameters.HitAnimation != null)
        {
            AttackController.CreateHitEffect(this.HitEffect, this.HitParameters.HitAnimation, between, Damagable.FREEZE_FRAMES, facing);
        }
    }

    private void bounce()
    {
        this.Actor.Velocity = new Vector2(this.Actor.Velocity.x, this.BounceVelocity);
    }

    private bool attemptMovingPlatformAlignment(GameObject platform)
    {
        IMovingPlatform movingPlatform = platform.GetComponent<IMovingPlatform>();
        if (movingPlatform != null)
        {
            _restingVelocityModifier.Modifier = movingPlatform.Velocity;
            return true;
        }
        return false;
    }
}
