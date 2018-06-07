using UnityEngine;

public class Damagable : VoBehavior, IPausable
{
    public const int FREEZE_FRAMES = 4;
    public const int DEATH_FREEZE_FRAMES = 12;
    public const int RAGE_MULTIPLIER = 6;

    public int Health = 10;
    public int MaxHealth = 10;
    public bool Invincible { get; private set; }
    public bool Dead { get { return this.Health <= 0; } }
    public string DeathLayer = "Dying";
    public bool UseRageMode = false;
    public int RageLimit = 200;
    public int RageDuration = 50;
    public bool IsRaging { get { return !_rageTimer.Completed; } }
    public bool CardinalKnockbackOnly = false;
    public bool FreezeSelf = true;
    public int Level = 1;
    public bool InvincibleToLowerLevels = false;
    public bool Stationary = false;
    public PooledObject BlockEffectPrefab;
    public SCSpriteAnimation BlockEffect;
    public delegate void OnDeathDelegate();
    public OnDeathDelegate OnDeathCallback;

    void Awake()
    {
        _invincibilityTimer = new Timer(1, false, false);
        _freezeFrameEvent = new FreezeFrameEvent(FREEZE_FRAMES);
        _hitStunEvent = new HitStunEvent(1, 1.0f, 1.0f, false, false, false, Vector2.zero);
        _healEvent = new HealEvent(this.Health, this.Health, this.MaxHealth, this.MaxHealth);
        _prevLayer = this.gameObject.layer;

        if (this.UseRageMode)
        {
            _rageTimer = new Timer(this.RageDuration, false, true, onRageComplete);
            _rageTimer.complete(false);
            _rageEvent = new RageEvent(false);
        }
    }

    void OnSpawn()
    {
        this.Invincible = false;
        _invincibilityTimer.reset(1);
        _invincibilityTimer.Paused = true;

        if (this.UseRageMode)
        {
            _rageCounter = 0;
            _rageTimer.complete(false);
        }

        if (this.gameObject.layer == LayerMask.NameToLayer(this.DeathLayer))
        {
            Debug.LogWarning("Damagable found spawning with dying layer: " + this.gameObject.name);
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

    public void ResetDamagable()
    {
        if (_invincibilityTimer != null)
            _invincibilityTimer.complete();
        this.Invincible = false;
        this.Health = this.MaxHealth;
    }

    public bool Damage(SCAttack.HitData hitData, IntegerVector origin, IntegerVector hitPoint, SCCharacterController.Facing attackerFacing)
    {
        if (this.Invincible)
            return false;

        if (this.InvincibleToLowerLevels && hitData.Level < this.Level)
        {
            this.Block(FREEZE_FRAMES, hitData);
            AttackController.CreateHitEffect(this.BlockEffectPrefab, this.BlockEffect, hitPoint, FREEZE_FRAMES, (SCCharacterController.Facing)Mathf.RoundToInt(Mathf.Sign(this.transform.position.x - origin.X)));
            return false;
        }

        // Damage
        int damage = hitData.Damage;
        if (hitData.Level >= this.Level + LEVEL_DIFF_FOR_ADV)
            damage += damage * Mathf.CeilToInt(LEVEL_ADV * (hitData.Level - LEVEL_DIFF_FOR_ADV - this.Level + 1));
        this.Health = Mathf.Max(0, this.Health - damage);
        
        // Handle knockback
        if (!this.Stationary && this.Actor != null)
            this.Actor.Velocity = hitData.GetKnockback(origin, this.transform.position, hitPoint, attackerFacing, this.CardinalKnockbackOnly);

        // Handle invincibility and freeze frames
        this.Invincible = true;
        int invinciblityDuration = (!this.Dead ? hitData.HitInvincibilityDuration : DEATH_HITSTUN) + FREEZE_FRAMES;
        _invincibilityTimer.reset(invinciblityDuration);
        _invincibilityTimer.start();

        if (this.FreezeSelf)
        {
            _freezeFrameEvent.NumFrames = this.Dead ? DEATH_FREEZE_FRAMES : FREEZE_FRAMES;
            this.localNotifier.SendEvent(_freezeFrameEvent);
        }

        // Rage buildup
        bool raging = false;
        if (this.UseRageMode)
        {
            raging = this.IsRaging;
            if (!raging)
            {
                _rageCounter += hitData.HitStunDuration * RAGE_MULTIPLIER;
                if (_rageCounter >= this.RageLimit)
                {
                    raging = true;
                    enterRage();
                }
            }
        }

        // Handle hitstun
        _hitStunEvent.NumFrames = !this.Dead ? hitData.HitStunDuration + FREEZE_FRAMES : invinciblityDuration;
        _hitStunEvent.GravityMultiplier = hitData.HitStunGravityMultiplier;
        _hitStunEvent.AirFrictionMultiplier = hitData.HitStunAirFrictionMultiplier;
        _hitStunEvent.Blocked = false;
        _hitStunEvent.Raging = raging && !this.Dead;
        _hitStunEvent.Dead = this.Dead;
        _hitStunEvent.HitPos = hitPoint;

        // SFX
        SoundManager.Play(hitData.HitSfxKey);

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

        if (this.UseRageMode)
        {
            _rageTimer.update();
            if (_rageCounter > 0)
                --_rageCounter;
        }
    }

    public void SetInvincible(int numFrames)
    {
        if (numFrames > 0)
            this.Invincible = true;
        if (numFrames > _invincibilityTimer.FramesRemaining)
            _invincibilityTimer.reset(numFrames);
        _invincibilityTimer.start();
    }

    public void ResetLayer()
    {
        this.gameObject.layer = _prevLayer;
    }

    public void Block(int freezeFrames, SCAttack.HitData hitData)
    {
        _freezeFrameEvent.NumFrames = freezeFrames;
        this.localNotifier.SendEvent(_freezeFrameEvent);

        //TODO: Play block sfx

        int stunFrames = hitData.HitStunDuration + freezeFrames;
        _hitStunEvent.NumFrames = stunFrames;
        _hitStunEvent.GravityMultiplier = 1.0f;
        _hitStunEvent.AirFrictionMultiplier = 1.0f;
        _hitStunEvent.Blocked = true;
        this.localNotifier.SendEvent(_hitStunEvent);

        this.SetInvincible(stunFrames);
    }

    /**
     * Private
     */
    private Timer _invincibilityTimer;
    private FreezeFrameEvent _freezeFrameEvent;
    private HitStunEvent _hitStunEvent;
    private HealEvent _healEvent;
    private int _prevLayer;
    private int _rageCounter;
    private Timer _rageTimer;
    private RageEvent _rageEvent;

    private const int DEATH_HITSTUN = 1000;
    private const float DEATH_GRAV_MULT = 0.9f;
    private const float DEATH_AIRFRICT_MULT = 0.9f;
    private const float DEATH_KNOCKBACK_ADD = 5.0f;
    private const int LEVEL_DIFF_FOR_ADV = 2;
    private const float LEVEL_ADV = 0.15f;

    private void heal(int amount)
    {
        _healEvent.PrevHealth = this.Health;
        this.Health = Mathf.Min(this.Health + amount, this.MaxHealth);
        _healEvent.NewHealth = this.Health;
    }

    private void die()
    {
        if (!this.DeathLayer.IsEmpty())
        {
            this.integerCollider.RemoveFromCollisionPool();
            this.gameObject.layer = LayerMask.NameToLayer(this.DeathLayer);
            this.integerCollider.AddToCollisionPool();
        }

        _hitStunEvent.GravityMultiplier *= DEATH_GRAV_MULT;
        _hitStunEvent.GravityMultiplier *= DEATH_AIRFRICT_MULT;

        if (!this.Stationary && this.Actor != null)
            this.Actor.Velocity = this.Actor.Velocity.normalized * (this.Actor.Velocity.magnitude + DEATH_KNOCKBACK_ADD);

        if (this.OnDeathCallback != null)
            this.OnDeathCallback();
    }

    private void enterRage()
    {
        _rageCounter = 0;
        _rageTimer.reset();
        _rageTimer.start();
        _rageEvent.Raging = true;
        this.localNotifier.SendEvent(_rageEvent);
    }

    private void onRageComplete()
    {
        _rageEvent.Raging = false;
        this.localNotifier.SendEvent(_rageEvent);
    }
}
