using UnityEngine;

public class PunchSwitch : VoBehavior, IPausable
{
    public Switch SwitchScript;
    public bool OneWay = false;
    public int TwoWayResetDuration = 0;

    void Awake()
    {
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHitStun);
        this.SwitchScript.StateChangeCallback += onStateChange;
    }

    void OnSpawn()
    {
        if (!this.OneWay)
        {
            if (_twoWayTimer == null)
                _twoWayTimer = new Timer(this.TwoWayResetDuration, false, false);
            else
                _twoWayTimer.reset(this.TwoWayResetDuration);
            _twoWayTimer.complete();
        }
    }

    void FixedUpdate()
    {
        if (!this.OneWay)
            _twoWayTimer.update();
    }

    /**
     * Private
     */
    private bool _pressed;
    private Timer _twoWayTimer;

    private void onHitStun(LocalEventNotifier.Event e)
    {
        //HitStunEvent hse = e as HitStunEvent;

        if (!_pressed)
        {
            if (canSwitch())
            {
                this.SwitchScript.ToggleSwitch();

                if (!this.OneWay)
                {
                    _twoWayTimer.reset();
                    _twoWayTimer.start();
                }
            }
        }
        else
        {
            if (canSwitch())
                this.SwitchScript.ToggleSwitch();
        }
    }

    private bool canSwitch()
    {
        if (!this.OneWay)
            return _twoWayTimer.Completed;

        return this.SwitchScript.CurrentState == Switch.SwitchState.OFF && _pressed;
    }

    private void onStateChange(Switch.SwitchState state)
    {
        switch (state)
        {
            default:
            case Switch.SwitchState.OFF:
                _pressed = false;
                break;
            case Switch.SwitchState.ON:
                _pressed = true;
                break;
        }
    }
}
