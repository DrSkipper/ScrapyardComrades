using UnityEngine;

public class PlayerHealthController : VoBehavior
{
    public Damagable Damagable;
    public int AttritionInterval = 3000;

    public HealthChangedDelegate HealthChangedCallback;
    public delegate void HealthChangedDelegate(int currentHealth, int maxHealth);

    void Awake()
    {
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHitStun);
    }

    void OnSpawn()
    {
        if (_attritionTimer == null)
            _attritionTimer = new Timer(this.AttritionInterval, true, true, attrition);
        else
            _attritionTimer.reset(this.AttritionInterval);

        _prevHealth = this.Damagable.Health;
        _prevMaxHealth = this.Damagable.MaxHealth;
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

    private void attrition()
    {
        this.Damagable.Health = Mathf.Max(0, this.Damagable.Health - 1);

        if (this.HealthChangedCallback != null)
            this.HealthChangedCallback(this.Damagable.Health, this.Damagable.MaxHealth);
    }

    private void onHitStun(LocalEventNotifier.Event e)
    {
        if (_prevHealth != this.Damagable.Health || _prevMaxHealth != this.Damagable.MaxHealth)
        {
            _prevHealth = this.Damagable.Health;
            _prevMaxHealth = this.Damagable.MaxHealth;

            if (this.HealthChangedCallback != null)
                this.HealthChangedCallback(this.Damagable.Health, this.Damagable.MaxHealth);
        }
    }
}
