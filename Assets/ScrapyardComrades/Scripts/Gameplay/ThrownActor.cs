using UnityEngine;

public class ThrownActor : Actor2D
{
    public float Gravity;
    public float Friction;
    public float AirFriction;
    public float MaxFallSpeed;
    public LayerMask MovingPlatformMask;

    void Start()
    {
        if (_vMod == null)
            this.Throw(Vector2.zero);
    }

    public void Throw(Vector2 initialVelocity)
    {
        _vMod = new VelocityModifier(initialVelocity, VelocityModifier.CollisionBehavior.bounce);
        _restingVMod = new VelocityModifier(Vector2.zero, VelocityModifier.CollisionBehavior.sustain);
        this.SetVelocityModifier(V_KEY, _vMod);
        this.SetVelocityModifier(RESTING_VELOCITY_KEY, _restingVMod);
    }

    public override void FixedUpdate()
    {
        if (_vMod != null)
        {
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
    }

    void OnReturnToPool()
    {
        _vMod.Modifier = Vector2.zero;
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
    private const string RESTING_VELOCITY_KEY = "rest";
}
