using UnityEngine;

public class Damagable : VoBehavior, IPausable
{
    public const int FREEZE_FRAMES = 4;
    public const int DEATH_FREEZE_FRAMES = 12;

    public int Health = 10;
    public int MaxHealth = 10;
    public bool Invincible { get; private set; }
    public bool Dead { get { return this.Health <= 0; } }
    public string DeathLayer = "Dying";

    void Awake()
    {
        _invincibilityTimer = new Timer(1, false, false);
        _freezeFrameEvent = new FreezeFrameEvent(FREEZE_FRAMES);
        _hitStunEvent = new HitStunEvent(1, 1.0f, 1.0f);
        _prevLayer = this.gameObject.layer;
    }

    public void Heal(int amount)
    {
        this.Health = Mathf.Min(this.Health + amount, this.MaxHealth);
    }

    public bool Damage(SCAttack.HitData hitData, IntegerVector origin, IntegerVector hitPoint, SCCharacterController.Facing attackerFacing)
    {
        if (this.Invincible)
            return false;

        // Damage
        this.Health = Mathf.Max(0, this.Health - hitData.Damage);
        
        // Handle knockback
        this.Actor.Velocity = hitData.GetKnockback(origin, this.transform.position, hitPoint, attackerFacing);

        // Handle invincibility and freeze frames
        this.Invincible = true;
        int invinciblityDuration = (!this.Dead ? hitData.HitInvincibilityDuration : DEATH_HITSTUN) + FREEZE_FRAMES;
        _invincibilityTimer.reset(invinciblityDuration);
        _invincibilityTimer.start();

        _freezeFrameEvent.NumFrames = this.Dead ? DEATH_FREEZE_FRAMES : FREEZE_FRAMES;
        this.localNotifier.SendEvent(_freezeFrameEvent);

        // Handle hitstun
        _hitStunEvent.NumFrames = !this.Dead ? hitData.HitStunDuration + FREEZE_FRAMES : invinciblityDuration;
        _hitStunEvent.GravityMultiplier = hitData.HitStunGravityMultiplier;
        _hitStunEvent.AirFrictionMultiplier = hitData.HitStunAirFrictionMultiplier;

        // SFX
        if (hitData.HitSfx != null && hitData.HitSfx != StringExtensions.EMPTY)
            SoundManager.Play(hitData.HitSfx);

        if (this.Dead)
        {
            this.gameObject.layer = LayerMask.NameToLayer(this.DeathLayer);
            _hitStunEvent.GravityMultiplier *= DEATH_GRAV_MULT;
            _hitStunEvent.GravityMultiplier *= DEATH_AIRFRICT_MULT;
            this.Actor.Velocity = this.Actor.Velocity.normalized * (this.Actor.Velocity.magnitude + DEATH_KNOCKBACK_ADD);
        }
        this.localNotifier.SendEvent(_hitStunEvent);

        return true;
    }

    //TODO - This should consistently be called either before or after updates of things that can damage
    void FixedUpdate()
    {
        if (_invincibilityTimer.Completed)
        {
            _invincibilityTimer.invalidate();
            this.Invincible = false;
        }
        _invincibilityTimer.update();
    }

    public void SetInvincible(int numFrames)
    {
        if (numFrames > _invincibilityTimer.FramesRemaining)
            _invincibilityTimer.reset(numFrames);
        _invincibilityTimer.start();
    }

    void OnReturnToPool()
    {
        this.gameObject.layer = _prevLayer;
    }

    /**
     * Private
     */
    private Timer _invincibilityTimer;
    private FreezeFrameEvent _freezeFrameEvent;
    private HitStunEvent _hitStunEvent;
    private int _prevLayer;

    private const int DEATH_HITSTUN = 250;
    private const float DEATH_GRAV_MULT = 0.9f;
    private const float DEATH_AIRFRICT_MULT = 0.9f;
    private const float DEATH_KNOCKBACK_ADD = 5.0f;
}
