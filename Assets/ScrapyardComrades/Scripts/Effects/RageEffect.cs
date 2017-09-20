using UnityEngine;

public class RageEffect : VoBehavior, IPausable
{
    public Color RageBlinkOnColor;
    public Color RageBlinkOffColor;
    public int BlinkDuration = 10;

    void Awake()
    {
        _defaultColor = this.spriteRenderer.color;
        _blinkTimer = new Timer(this.BlinkDuration, true, false, blink);
        this.localNotifier.Listen(RageEvent.NAME, this, onRage);
    }

    void FixedUpdate()
    {
        if (_raging)
            _blinkTimer.update();
    }

    /**
     * Private
     */
    private Color _defaultColor;
    private bool _raging;
    private Timer _blinkTimer;
    private bool _blinkedOn;

    private void onRage(LocalEventNotifier.Event e)
    {
        if ((e as RageEvent).Raging)
        {
            _raging = true;
            _blinkedOn = false;
            blink();
            _blinkTimer.reset();
            _blinkTimer.start();
        }
        else
        {
            _raging = false;
            this.spriteRenderer.color = _defaultColor;
        }
    }

    private void blink()
    {
        _blinkedOn = !_blinkedOn;
        this.spriteRenderer.color = _blinkedOn ? this.RageBlinkOnColor : this.RageBlinkOffColor;
    }
}
