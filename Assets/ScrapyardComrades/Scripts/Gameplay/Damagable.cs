using UnityEngine;

public class Damagable : VoBehavior
{
    public const int FreezeFrames = 6;
    public bool Invincible { get; private set; }

    void Awake()
    {
        _invincibilityTimer = new Timer(1, false, false);
    }

    public bool Damage(SCAttack attack, IntegerVector origin, IntegerVector hitPoint)
    {
        if (this.Invincible)
            return false;

        //TODO - Normalize knockback effect to 16 directions
        Vector2 knockbackDirection = ((Vector2)(hitPoint - origin)).normalized;
        this.Actor.Velocity = knockbackDirection * attack.KnockbackPower;
        this.Invincible = true;
        _invincibilityTimer.reset(attack.HitInvincibilityDuration + FreezeFrames);
        _invincibilityTimer.start();
        this.localNotifier.SendEvent(new FreezeFrameEvent(FreezeFrames));
        this.localNotifier.SendEvent(new HitStunEvent(attack.HitStunDuration));
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
}
