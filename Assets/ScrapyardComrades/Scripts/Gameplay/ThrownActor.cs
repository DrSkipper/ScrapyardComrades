using UnityEngine;

public class ThrownActor : Actor2D
{
    public float Gravity;
    public float Friction;
    public float AirFriction;
    public float MaxFallSpeed;
    public float BounceMultiplier = 1.0f;
    public LayerMask MovingPlatformMask;
    public Pickup Pickup;
    public DamageOnCollision Damager;

    void Start()
    {
        if (_vMod == null)
            this.Throw(0.0f, 0);

        _nonHaltBounceCooldown = new Timer(NON_HALTING_BOUNCE_COOLDOWN, false, false);
        _nonHaltBounceCooldown.complete();
        this.Damager.ConfigureForPickup(this.Pickup.Data);
        this.localNotifier.Listen(CollisionEvent.NAME, this, onCollide);
    }

    public void Throw(float angle, int direction)
    {
        this.spriteRenderer.sprite = this.Pickup.Data.Sprite;
        _vMod = new VelocityModifier(getThrowDirection(angle, direction) * this.Pickup.Data.ThrowVelocity, VelocityModifier.CollisionBehavior.bounce, this.BounceMultiplier);
        _restingVMod = new VelocityModifier(Vector2.zero, VelocityModifier.CollisionBehavior.sustain);
        this.Damager.ConfigureForPickup(this.Pickup.Data);
        this.SetVelocityModifier(V_KEY, _vMod);
        this.SetVelocityModifier(RESTING_VELOCITY_KEY, _restingVMod);
    }

    public override void FixedUpdate()
    {
        _nonHaltBounceCooldown.update();
        Vector2 v = _vMod.Modifier;
        if (v.y > 0)
            v.y = v.y.Approach(-this.MaxFallSpeed, this.Gravity + this.AirFriction);
        else
            v.y = v.y.Approach(-this.MaxFallSpeed, this.Gravity);

        GameObject groundedAgainst = this.GroundedAgainst;
        v.x = v.x.Approach(0.0f, groundedAgainst != null ? this.Friction : this.AirFriction);
        _vMod.Modifier = v;
            
        if (groundedAgainst != null && ((1 << groundedAgainst.layer) & this.MovingPlatformMask) == (1 << groundedAgainst.layer))
        {
            _restingVMod.Modifier = groundedAgainst.GetComponent<IMovingPlatform>().Velocity;
        }
        else
        {
            _restingVMod.Modifier = Vector2.zero;
        }
        base.FixedUpdate();
    }

    void onCollide(LocalEventNotifier.Event e)
    {
        if (!_nonHaltBounceCooldown.Completed)
            return;
        CollisionEvent ce = e as CollisionEvent;

        if (!ce.MovementHalted && (ce.CollideX || ce.CollideY))
        {
            if (ce.CollideX)
                _vMod.CollideX();
            if (ce.CollideY)
                _vMod.CollideY();
            _nonHaltBounceCooldown.reset();
            _nonHaltBounceCooldown.start();
        }
    }

    void OnReturnToPool()
    {
        if (_vMod != null)
            _vMod.Modifier = Vector2.zero;
        if (_restingVMod != null)
            _restingVMod.Modifier = Vector2.zero;
        if (_nonHaltBounceCooldown != null)
            _nonHaltBounceCooldown.complete();
    }

    public GameObject GroundedAgainst
    {
        get { return this.integerCollider.CollideFirst(0, -1, this.HaltMovementMask); }
    }

    /**
     * Private
     */
    private const string V_KEY = "throw";
    private VelocityModifier _vMod;
    private VelocityModifier _restingVMod;
    private Timer _nonHaltBounceCooldown;
    private const string RESTING_VELOCITY_KEY = "rest";
    private const int NON_HALTING_BOUNCE_COOLDOWN = 8;

    private static Vector2 getThrowDirection(float angle, int dir)
    {
        Vector2 v = Vector2.right.VectorAtAngle(-angle);
        v.x *= dir;
        return v;
    }
}
