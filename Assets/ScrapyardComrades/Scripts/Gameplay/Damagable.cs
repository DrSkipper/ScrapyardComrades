using UnityEngine;

public class Damagable : VoBehavior, IPausable
{
    public const int FREEZE_FRAMES = 6;
    public bool Invincible { get; private set; }

    void Awake()
    {
        _invincibilityTimer = new Timer(1, false, false);
        _freezeFrameEvent = new FreezeFrameEvent(FREEZE_FRAMES);
        _hitStunEvent = new HitStunEvent(1);
    }

    public bool Damage(SCAttack attack, IntegerVector origin, IntegerVector hitPoint)
    {
        if (this.Invincible)
            return false;

        //TODO - Normalize knockback effect to 16 directions
        Vector2 knockbackDirection = ((Vector2)(hitPoint - origin)).normalized;
        this.Actor.Velocity = knockbackDirection * attack.KnockbackPower;
        this.Invincible = true;
        _invincibilityTimer.reset(attack.HitInvincibilityDuration + FREEZE_FRAMES);
        _invincibilityTimer.start();
        this.localNotifier.SendEvent(_freezeFrameEvent);
        _hitStunEvent.NumFrames = attack.HitStunDuration;
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
