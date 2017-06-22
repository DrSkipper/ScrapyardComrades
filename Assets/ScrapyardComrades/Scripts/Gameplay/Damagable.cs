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
        _healEvent = new HealEvent(this.Health, this.Health, this.MaxHealth, this.MaxHealth);
        _prevLayer = this.gameObject.layer;
    }

    void OnSpawn()
    {
        this.Invincible = false;
        _invincibilityTimer.reset();
        _invincibilityTimer.Paused = true;
        if (this.gameObject.layer == LayerMask.NameToLayer(this.DeathLayer))
        {
            Debug.LogWarning("Damagable found spawning with dying layer");
            this.ResetLayer();
        }
    }

    public void Heal(int amount)
    {
        heal(amount);
        this.localNotifier.SendEvent(_healEvent);
    }

    public void IncreaseMaxHealth(int increaseAmount, int healAmount)
    {
        _healEvent.PrevMaxHealth = this.MaxHealth;
        this.MaxHealth += increaseAmount;
        _healEvent.NewMaxHealth = this.MaxHealth;
        heal(healAmount);
        this.localNotifier.SendEvent(_healEvent);
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
            die();

        this.localNotifier.SendEvent(_hitStunEvent);

        return true;
    }

    //TODO - This should consistently be called either before or after updates of things that can damage
    void FixedUpdate()
    {
        if (_invincibilityTimer.Completed)
            this.Invincible = false;
        else
            _invincibilityTimer.update();
    }

    public void SetInvincible(int numFrames)
    {
        if (numFrames > _invincibilityTimer.FramesRemaining)
            _invincibilityTimer.reset(numFrames);
        _invincibilityTimer.start();
    }

    public void ResetLayer()
    {
        this.gameObject.layer = _prevLayer;
    }

    /**
     * Private
     */
    private Timer _invincibilityTimer;
    private FreezeFrameEvent _freezeFrameEvent;
    private HitStunEvent _hitStunEvent;
    private HealEvent _healEvent;
    private int _prevLayer;

    private const int DEATH_HITSTUN = 250;
    private const float DEATH_GRAV_MULT = 0.9f;
    private const float DEATH_AIRFRICT_MULT = 0.9f;
    private const float DEATH_KNOCKBACK_ADD = 5.0f;

    private void heal(int amount)
    {
        _healEvent.PrevHealth = this.Health;
        this.Health = Mathf.Min(this.Health + amount, this.MaxHealth);
        _healEvent.NewHealth = this.Health;
    }

    private void die()
    {
        this.integerCollider.RemoveFromCollisionPool();
        this.gameObject.layer = LayerMask.NameToLayer(this.DeathLayer);
        this.integerCollider.AddToCollisionPool();

        _hitStunEvent.GravityMultiplier *= DEATH_GRAV_MULT;
        _hitStunEvent.GravityMultiplier *= DEATH_AIRFRICT_MULT;
        this.Actor.Velocity = this.Actor.Velocity.normalized * (this.Actor.Velocity.magnitude + DEATH_KNOCKBACK_ADD);
    }
}
