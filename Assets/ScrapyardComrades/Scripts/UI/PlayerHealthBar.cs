using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour, IPausable
{
    public UIBar Bar;
    public Image CurrentHealthImage;
    public LostHealthBarChunk LostHealthChunk;
    public Color StrobeColor;
    public int MinStrobeDuration;
    public int MaxStrobeDuration;
    public int PixelUnitsPerHealthUnit = 10;
    public Easing.Function ScaleFunction;
    public Easing.Flow ScaleFlow;
    public float TargetScale = 2.0f;
    public int ScaleDuration = 20;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);

        float h, s, v;
        Color.RGBToHSV(this.CurrentHealthImage.color, out h, out s, out v);
        _startHSV = new Vector3(h, s, v);
        Color.RGBToHSV(this.StrobeColor, out h, out s, out v);
        _endHSV = new Vector3(h, s, v);
        _currentHSV = _startHSV;
        _strobingIn = true;
        _currentDuration = this.MaxStrobeDuration;
        _currentSpeed = (_endHSV - _startHSV) / _currentDuration;
        _scaleDelegate = Easing.GetFunction(this.ScaleFunction, this.ScaleFlow);
    }

    void FixedUpdate()
    {
        Vector3 target = _strobingIn ? _endHSV : _startHSV;
        _currentHSV.x = _currentHSV.x.Approach(target.x, _currentSpeed.x);
        _currentHSV.y = _currentHSV.y.Approach(target.y, _currentSpeed.y);
        _currentHSV.z = _currentHSV.z.Approach(target.z, _currentSpeed.z);

        if (_t >= _currentDuration)
        {
            _strobingIn = !_strobingIn;
            _t = 0;
        }
        else
        {
            ++_t;
        }

        if (_scaleT >= 0)
        {
            float scale = 1;

            if (_scaleT > 0)
            {
                int oneThird = this.ScaleDuration / 3;
                int twoThirds = this.ScaleDuration * 2 / 3;

                if (_scaleT >= twoThirds)
                {
                    scale = _scaleDelegate(oneThird - (_scaleT - twoThirds), 1.0f, this.TargetScale - 1.0f, oneThird);
                }
                else if (_scaleT <= oneThird)
                {
                    scale = _scaleDelegate(oneThird - _scaleT, this.TargetScale, 1.0f - this.TargetScale, oneThird);
                }
                else
                {
                    scale = this.TargetScale;
                }
            }

            this.transform.localScale = new Vector3(scale, scale, scale);
            _scaleT -= 1;
        }

        this.CurrentHealthImage.color = Color.HSVToRGB(_currentHSV.x, _currentHSV.y, _currentHSV.z);
    }

    /**
     * Private
     */
    private Vector3 _currentSpeed;
    private int _t;
    private int _currentDuration;
    private bool _strobingIn;
    private Vector3 _currentHSV;
    private Vector3 _startHSV;
    private Vector3 _endHSV;
    private int _prevHealth;
    private Easing.EasingDelegate _scaleDelegate;
    private int _scaleT;

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        PlayerHealthController healthController = (e as PlayerSpawnedEvent).PlayerObject.GetComponent<PlayerHealthController>();
        if (healthController != null)
        {
            _prevHealth = healthController.Damagable.Health;
            playerHealthChanged(healthController.Damagable.Health, healthController.Damagable.MaxHealth, false);
            healthController.HealthChangedCallback += playerHealthChanged;
        }
    }

    private void playerHealthChanged(int currentHealth, int maxHealth, bool animate = true, bool highlight = true)
    {
        this.Bar.ChangeTargetLength(maxHealth * this.PixelUnitsPerHealthUnit);
        this.Bar.UpdateLength(currentHealth, maxHealth);

        _currentDuration = Mathf.RoundToInt(Mathf.Lerp(this.MinStrobeDuration, this.MaxStrobeDuration, (float)currentHealth / (float)maxHealth));
        _currentSpeed = (_endHSV - _startHSV) / _currentDuration;

        if (currentHealth != _prevHealth)
        {
            if (animate)
            {
                if (currentHealth < _prevHealth)
                    this.LostHealthChunk.TriggerHealthLostAnimation(currentHealth, _prevHealth, maxHealth, this.Bar.TargetLength);

                if (highlight)
                {
                    if (_scaleT <= 0)
                    {
                        _scaleT = this.ScaleDuration;
                    }
                    else if (_scaleT < this.ScaleDuration * 2 / 3)
                    {
                        if (_scaleT < this.ScaleDuration / 3)
                            _scaleT = this.ScaleDuration - _scaleT;
                        else
                            _scaleT = this.ScaleDuration * 2 / 3;
                    }
                }
            }
            _prevHealth = currentHealth;
        }
    }
}
