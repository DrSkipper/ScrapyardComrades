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

    private const int HIT_EFFECT_BORDER = 8;

    void Awake()
    {
        _nearbyDamagables = new List<IntegerCollider>();
        _damageRange = this.integerCollider.Bounds;
        _damageRange.Size += new IntegerVector(_damageRange.Size.X / 2, 0);
        this.localNotifier.Listen(CollisionEvent.NAME, this, onCollide);
    }

    void OnSpawn()
    {
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
            int groundedLayerMask = 1 << groundedAgainst.layer;
            if ((groundedLayerMask & this.MovingPlatformMask) == groundedLayerMask)
            {
                attemptMovingPlatformAlignment(groundedAgainst);
            }
        }

        velocity.x = velocity.x.Approach(0.0f, groundedAgainst != null ? this.Friction : this.AirFriction);
        velocity.y = velocity.y.Approach(-this.MaxFallSpeed, this.Gravity);
        this.Actor.Velocity = velocity;
        _velocity = this.Actor.TotalVelocity;

        /*velocity = this.Actor.TotalVelocity;
        bool movingDown = velocity.y < -this.MinSpeedToDamageDown;
        bool movingUp = !movingDown && velocity.y >= this.MinSpeedToDamage;
        bool movingLeft = velocity.x <= -this.MinSpeedToDamage;
        bool movingRight = !movingLeft && velocity.x >= this.MinSpeedToDamage;

        if (movingDown || movingUp || movingLeft || movingRight)
        {
            _damageRange.Center = (IntegerVector)(Vector2)this.transform.position;
            CollisionManager.Instance.GetCollidersInRange(_damageRange, this.DamagableLayers, null, _nearbyDamagables);

            for (int i = _nearbyDamagables.Count - 1; i >= 0; --i)
            {
                IntegerCollider collider = _nearbyDamagables[i];
                if (collider == this.integerCollider)
                    _nearbyDamagables.RemoveAt(i);
            }

            if (movingDown)
                damage(this.DownDamageBox, 0, -1);
            if (movingUp)
                damage(this.UpDamageBox, 0, 1);
            if (movingLeft)
                damage(this.LeftDamageBox, -1, 0);
            if (movingRight)
                damage(this.RightDamageBox, 1, 0);
        }*/
    }

    public GameObject GroundedAgainst
    {
        get { return this.integerCollider.CollideFirst(0, -1, this.Actor.HaltMovementMask, null); }
    }

    /**
     * Private
     */
    private IntegerRect _damageRange;
    private List<IntegerCollider> _nearbyDamagables;
    private VelocityModifier _restingVelocityModifier;
    private Vector2 _velocity;

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
                            hit(damagable, (Vector2)damagable.transform.position, new Vector2(this.transform.position.x, this.transform.position.y + this.integerCollider.Offset.Y - this.integerCollider.Bounds.Size.Y + HIT_EFFECT_BORDER));

                            if (this.Actor.HaltMovementMask.ContainsLayer(collided.layer))
                                bounce();
                        }
                        else if (movingUp && collided.transform.position.y > this.transform.position.y)
                        {
                            hit(damagable, (Vector2)damagable.transform.position, new Vector2(this.transform.position.x, this.transform.position.y + this.integerCollider.Offset.Y + this.integerCollider.Bounds.Size.Y - HIT_EFFECT_BORDER));
                        }
                        else if (movingLeft && collided.transform.position.x < this.transform.position.x)
                        {
                            hit(damagable, (Vector2)damagable.transform.position, new Vector2(this.transform.position.x + this.integerCollider.Offset.X - this.integerCollider.Bounds.Size.X + HIT_EFFECT_BORDER, this.transform.position.y));
                        }
                        else if (movingRight && collided.transform.position.x > this.transform.position.x)
                        {
                            hit(damagable, (Vector2)damagable.transform.position, new Vector2(this.transform.position.x + this.integerCollider.Offset.X + this.integerCollider.Bounds.Size.X - HIT_EFFECT_BORDER, this.transform.position.y));
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

    /*private void damage(IntegerCollider collider, int targetDirX, int targetDirY)
    {
        GameObject collided = collider.CollideFirst(0, 0, this.DamagableLayers, null, _nearbyDamagables);
        if (collided != null)
        {
            IntegerCollider otherCollider = collided.GetComponent<IntegerCollider>();
            IntegerVector colliderPos = new IntegerVector(Mathf.RoundToInt(collider.transform.position.x) + collider.Offset.X, Mathf.RoundToInt(collider.transform.position.y) + collider.Offset.Y);
            IntegerVector otherPos = new IntegerVector(Mathf.RoundToInt(otherCollider.transform.position.x) + otherCollider.Offset.X, Mathf.RoundToInt(otherCollider.transform.position.y) + otherCollider.Offset.Y);

            int dirX = Mathf.RoundToInt(Mathf.Sign(otherPos.X - colliderPos.X));
            int dirY = Mathf.RoundToInt(Mathf.Sign(otherPos.Y - colliderPos.Y));
            if ((targetDirX == 0 || targetDirX == dirX) && (targetDirY == 0 || targetDirY == dirY))
            {
                Damagable damagable = collided.GetComponent<Damagable>();
                if (damagable != null)
                {
                    IntegerVector between = (otherPos + colliderPos) / 2; 
                    SCCharacterController.Facing facing = dirX == 1 ? SCCharacterController.Facing.Right : SCCharacterController.Facing.Left;
                    bool landedHit = damagable.Damage(this.HitParameters, (Vector2)this.transform.position, between, facing);

                    if (landedHit && this.HitParameters.HitAnimation != null)
                    {
                        AttackController.CreateHitEffect(this.HitEffect, this.HitParameters.HitAnimation, between, Damagable.FREEZE_FRAMES, facing);
                    }
                }
            }
        }
    }*/

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
