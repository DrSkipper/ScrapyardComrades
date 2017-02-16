using UnityEngine;

public class ThrownActor : Actor2D
{
    public float Gravity;
    public float Friction;
    public float AirFriction;
    public float MaxFallSpeed;

    public void Throw(Vector2 initialVelocity)
    {
        _vMod = new VelocityModifier(initialVelocity, VelocityModifier.CollisionBehavior.bounce);
        this.SetVelocityModifier(V_KEY, _vMod);
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
            v.x = v.x.Approach(0.0f, this.IsGrounded ? this.Friction : this.AirFriction);
            _vMod.Modifier = v;
            this.SetVelocityModifier(V_KEY, _vMod);
            base.FixedUpdate();
        }
    }

    void OnReturnToPool()
    {
        this.RemoveVelocityModifier(V_KEY);
        _vMod = null;
    }

    public bool IsGrounded
    {
        get { return this.integerCollider.CollideFirst(0, -1, this.HaltMovementMask) != null; }
    }

    /**
     * Private
     */
    private const string V_KEY = "throw";
    private VelocityModifier _vMod;
}
