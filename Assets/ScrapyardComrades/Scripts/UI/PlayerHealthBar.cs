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
    public SCSpriteAnimator IconAnimator;
    public SCSpriteAnimator BloodAnimator1;
    public SCSpriteAnimator BloodAnimator2;
    public SCSpriteAnimation IconHurtAnim;
    public SCSpriteAnimation[] IconIdleAnims;
    public float IconIdleLerpMinPercent = 2.0f;
    public float IconIdleLerpMaxPercent = 50.0f;
    public int HurtAnimDuration = 30;

    public float MaxPercentForBeeps = 0.4f;
    public SoundData.Key HealthBeepSfxKey;

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

        _idleAnimCutoff = (this.IconIdleLerpMaxPercent - this.IconIdleLerpMinPercent) / Mathf.Max(this.IconIdleAnims.Length - 1, 1);
        _hurtTimer = new Timer(this.HurtAnimDuration, false, false, playCorrectIdleAnim);
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
            
            if (_strobingIn && _healthController != null && _prevHealth / (float)_healthController.Damagable.MaxHealth < this.MaxPercentForBeeps)
                SoundManager.Play(this.HealthBeepSfxKey);
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
        _hurtTimer.update();
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
    private float _idleAnimCutoff;
    private Timer _hurtTimer;
    private PlayerHealthController _healthController;

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        _healthController = (e as PlayerSpawnedEvent).PlayerObject.GetComponent<PlayerHealthController>();
        if (_healthController != null)
        {
            _prevHealth = _healthController.Damagable.Health;
            playerHealthChanged(_healthController.Damagable.Health, _healthController.Damagable.MaxHealth, false);
            _healthController.HealthChangedCallback += playerHealthChanged;
        }
    }

    private void playerHealthChanged(int currentHealth, int maxHealth, bool wasAttrition, bool animate = true, bool highlight = true)
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
                {
                    this.LostHealthChunk.TriggerHealthLostAnimation(currentHealth, _prevHealth, maxHealth, this.Bar.TargetLength);
                    this.IconAnimator.PlayAnimation(this.IconHurtAnim);
                    this.BloodAnimator1.gameObject.SetActive(true);
                    this.BloodAnimator2.gameObject.SetActive(true);
                    this.BloodAnimator1.Play();
                    this.BloodAnimator2.Play();
                    _hurtTimer.resetAndStart();
                }
                else
                {
                    playCorrectIdleAnim();
                }

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
            else
            {
                playCorrectIdleAnim();
            }

            _prevHealth = currentHealth;
        }

        this.LostHealthChunk.UpdateTransform(currentHealth, maxHealth, this.Bar.TargetLength);
    }

    private void playCorrectIdleAnim()
    {
        this.BloodAnimator1.gameObject.SetActive(false);
        this.BloodAnimator2.gameObject.SetActive(false);

        if (_healthController != null)
        {
            float current = (((float)_healthController.Damagable.Health) / _healthController.Damagable.MaxHealth) * 100.0f;
            int i = 0;
            bool found = false;
            for (float percent = this.IconIdleLerpMaxPercent; percent > this.IconIdleLerpMinPercent; percent -= _idleAnimCutoff)
            {
                if (current > percent)
                {
                    found = true;
                    this.IconAnimator.PlayAnimation(this.IconIdleAnims[i]);
                    break;
                }
                ++i;
            }

            if (!found)
                this.IconAnimator.PlayAnimation(this.IconIdleAnims[this.IconIdleAnims.Length - 1]);
        }
    }
}
