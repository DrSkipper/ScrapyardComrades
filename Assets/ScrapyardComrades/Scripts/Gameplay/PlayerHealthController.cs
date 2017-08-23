using UnityEngine;

public class PlayerHealthController : VoBehavior, IPausable
{
    public const string MUTATE_EVENT = "MUTATE";
    public const int MUTATE_HEAL_AMT = 1;

    public Damagable Damagable;
    public int AttritionInterval = 3000;
    public HeroProgressionData ProgressionData;
    public int HeroLevel = 0;
    public SCAttack.HitData AttritionDeathHitData;

    public HealthChangedDelegate HealthChangedCallback;
    public delegate void HealthChangedDelegate(int currentHealth, int maxHealth, bool animate = true);

    void Awake()
    {
        _healEffect = this.GetComponent<HealEffect>();
        this.localNotifier.Listen(HealEvent.NAME, this, onHeal);
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHitStun);
    }

    void OnSpawn()
    {
        int level = SaveData.DataLoaded ? SaveData.PlayerStats.Level : -1;

        if (level > 0)
        {
            if (level != this.HeroLevel)
            {
                loadLevel(level, SaveData.PlayerStats.CurrentHealth, SaveData.PlayerStats.MaxHealth);
                return;
            }

            this.Damagable.MaxHealth = SaveData.PlayerStats.MaxHealth;
            this.Damagable.Health = SaveData.PlayerStats.CurrentHealth;
        }

        if (_attritionTimer == null)
            _attritionTimer = new Timer(this.AttritionInterval, true, true, attrition);
        else
            _attritionTimer.reset(this.AttritionInterval);

        _prevHealth = this.Damagable.Health;
        _prevMaxHealth = this.Damagable.MaxHealth;
        notifyHealthChange(false);
    }

    private void FixedUpdate()
    {
        _attritionTimer.update();
    }

    void OnReturnToPool()
    {
        this.HealthChangedCallback = null;
    }

    /**
     * Private
     */
    private Timer _attritionTimer;
    private int _prevHealth;
    private int _prevMaxHealth;
    private HealEffect _healEffect;

    private void attrition()
    {
        int health = this.Damagable.Health - 1;
        if (health > 0)
        {
            this.Damagable.Health = health;
            notifyHealthChange();

            if (_healEffect != null)
                _healEffect.BeginEffect();
        }
        else
        {
            this.Damagable.Damage(this.AttritionDeathHitData, (Vector2)this.transform.position, (Vector2)this.transform.position, SCCharacterController.Facing.Right);
        }
    }

    private void onHeal(LocalEventNotifier.Event e)
    {
        if (this.HeroLevel < this.ProgressionData.MaxHeroLevel && this.Damagable.MaxHealth >= this.ProgressionData.MaxHealthThresholds[this.HeroLevel + 1])
            levelUp();
        else
            notifyHealthChange();
    }

    private void onHitStun(LocalEventNotifier.Event e)
    {
        if (_prevHealth != this.Damagable.Health || _prevMaxHealth != this.Damagable.MaxHealth)
        {
            _prevHealth = this.Damagable.Health;
            _prevMaxHealth = this.Damagable.MaxHealth;
            notifyHealthChange();
        }
    }

    private void notifyHealthChange(bool animate = true)
    {
        if (this.HealthChangedCallback != null)
            this.HealthChangedCallback(this.Damagable.Health, this.Damagable.MaxHealth, animate);
    }

    private void levelUp()
    {
        GlobalEvents.Notifier.SendEvent(new LocalEventNotifier.Event(MUTATE_EVENT));
        SaveData.PlayerStats.Level = this.HeroLevel + 1;
        loadLevel(this.HeroLevel + 1, this.Damagable.Health, this.ProgressionData.MaxHealthThresholds[this.HeroLevel + 1]);
    }

    private void loadLevel(int level, int health, int maxHealth)
    {
        PooledObject nextLevel = this.ProgressionData.HeroPrefabs[level].Retain();

        int offsetY = 0;
        int halfHeight = 0;
        if (this.integerCollider != null)
        {
            offsetY = this.integerCollider.Offset.Y;
            halfHeight = this.integerCollider.Bounds.Size.Y / 2;
        }

        IntegerCollider otherCollider = nextLevel.GetComponent<IntegerCollider>();
        int otherOffsetY = 0;
        int otherHalfHeight = 0;
        if (otherCollider != null)
        {
            otherOffsetY = otherCollider.Offset.Y;
            otherHalfHeight = otherCollider.Bounds.Size.Y / 2;
        }

        WorldEntity entity = this.GetComponent<WorldEntity>();
        WorldEntity otherEntity = nextLevel.GetComponent<WorldEntity>();
        otherEntity.EntityName = entity.EntityName;
        otherEntity.QuadName = entity.QuadName;

        nextLevel.transform.SetPosition2D(this.transform.position.x, this.transform.position.y + offsetY - halfHeight + otherHalfHeight - otherOffsetY);

        // Make sure we're not spawning into ground, since leveled up hitbox will be wider (note that we shouldn't need to check Y here, because players will spawn ducking if they aren't able to stand up, and all duck heights should be less or equal to the standing height of the previous player level
        SCCharacterController otherActor = otherEntity.GetComponent<SCCharacterController>();
        int dir = -1;
        int mag = 0;
        while (otherCollider.CollideFirst(mag * dir, 0, otherActor.HaltMovementMask) != null)
        {
            dir *= -1;
            if (dir == 1)
                mag += 1;
        }

        if (mag > 0)
            nextLevel.transform.SetX(nextLevel.transform.position.x + (mag * dir));

        otherActor.SetFacingDirectly(this.GetComponent<SCCharacterController>().CurrentFacing);

        SaveData.PlayerStats.MaxHealth = maxHealth;
        SaveData.PlayerStats.CurrentHealth = Mathf.Min(health + MUTATE_HEAL_AMT, otherActor.Damagable.MaxHealth);
        nextLevel.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);

        GlobalEvents.Notifier.SendEvent(new EntityReplacementEvent(otherEntity));
        GlobalEvents.Notifier.SendEvent(new InteractionTargetChangeEvent(null));
        ObjectPools.Release(this.gameObject);
    }
}
