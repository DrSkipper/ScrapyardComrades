using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIBar))]
public class PlayerHealthBar : MonoBehaviour, IPausable
{
    public UIBar Bar;
    public Image CurrentHealthImage;
    public Color StrobeColor;
    public int MinStrobeDuration;
    public int MaxStrobeDuration;

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

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        PlayerHealthController healthController = (e as PlayerSpawnedEvent).PlayerObject.GetComponent<PlayerHealthController>();
        if (healthController != null)
        {
            playerHealthChanged(healthController.Damagable.Health, healthController.Damagable.MaxHealth);
            healthController.HealthChangedCallback += playerHealthChanged;
        }
    }

    private void playerHealthChanged(int currentHealth, int maxHealth)
    {
        //TODO: Extend max length of bar based on max health
        this.Bar.UpdateLength(currentHealth, maxHealth);

        _currentDuration = Mathf.RoundToInt(Mathf.Lerp(this.MinStrobeDuration, this.MaxStrobeDuration, (float)currentHealth / (float)maxHealth));
        _currentSpeed = (_endHSV - _startHSV) / _currentDuration;
    }
}
