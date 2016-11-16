using UnityEngine;

public class Damagable : VoBehavior, IPausable
{
    public const int FREEZE_FRAMES = 6;
    public bool Invincible { get; private set; }

    void Awake()
    {
        _invincibilityTimer = new Timer(1, false, false);
        _freezeFrameEvent = new FreezeFrameEvent(FREEZE_FRAMES);
        _hitStunEvent = new HitStunEvent(1, 1.0f, 1.0f);
    }

    public bool Damage(SCAttack attack, IntegerVector origin, IntegerVector hitPoint, SCCharacterController.Facing attackerFacing)
    {
        if (this.Invincible)
            return false;
        
        // Handle knockback
        this.Actor.Velocity = attack.HitParameters.GetKnockback(origin, this.transform.position, hitPoint, attackerFacing);

        // Handle invincibility and freeze frames
        this.Invincible = true;
        _invincibilityTimer.reset(attack.HitParameters.HitInvincibilityDuration + FREEZE_FRAMES);
        _invincibilityTimer.start();
        this.localNotifier.SendEvent(_freezeFrameEvent);

        // Handle hitstun
        _hitStunEvent.NumFrames = attack.HitParameters.HitStunDuration;
        _hitStunEvent.GravityMultiplier = attack.HitParameters.HitStunGravityMultiplier;
        _hitStunEvent.AirFrictionMultiplier = attack.HitParameters.HitStunAirFrictionMultiplier;
        this.localNotifier.SendEvent(_hitStunEvent);
        return true;
    }

    //TODO - This should consistently be called either before or after updates of things that can damage
    void FixedUpdate()
    {
        _invincibilityTimer.update();
        if (_invincibilityTimer.Completed)
        {
            _invincibilityTimer.invalidate();
            this.Invincible = false;
        }
    }

    public void SetInvincible(int numFrames)
    {
        if (numFrames > _invincibilityTimer.FramesRemaining)
            _invincibilityTimer.reset(numFrames);
        _invincibilityTimer.start();
    }

    /**
     * Private
     */
    private Timer _invincibilityTimer;
    private FreezeFrameEvent _freezeFrameEvent;
    private HitStunEvent _hitStunEvent;
}
