using UnityEngine;

public class PunchSwitch : VoBehavior, IPausable, SwitchBehavior
{
    public Switch SwitchScript;
    public bool OneWay { get; set; }
    public int TwoWayResetDuration { get; set; }

    public SoundData.Key SwitchOnKey = SoundData.Key.NONE;
    public SoundData.Key SwitchOffKey = SoundData.Key.NONE;

    void Awake()
    {
        this.localNotifier.Listen(HitStunEvent.NAME, this, onHitStun);
        //this.SwitchScript.StateChangeCallback += onStateChange;
    }

    void OnSpawn()
    {
        if (!this.OneWay)
        {
            if (_twoWayTimer == null)
            {
                _twoWayTimer = new Timer(this.TwoWayResetDuration, false, false, onTwoWayComplete);
            }
            else
            {
                _twoWayTimer.reset(this.TwoWayResetDuration);
                _twoWayTimer.Paused = true;
            }
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
    private Timer _twoWayTimer;

    private void onHitStun(LocalEventNotifier.Event e)
    {
        //HitStunEvent hse = e as HitStunEvent;

        if (this.SwitchScript.CurrentState == Switch.SwitchState.OFF)
        {
            if (canSwitch())
            {
                SoundManager.Play(this.SwitchOnKey, this.transform);
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
            {
                SoundManager.Play(this.SwitchOffKey, this.transform);
                this.SwitchScript.ToggleSwitch();
            }
        }
    }

    private void onTwoWayComplete()
    {
        if (this.SwitchScript.CurrentState == Switch.SwitchState.ON)
            this.SwitchScript.ToggleSwitch();
    }

    private bool canSwitch()
    {
        if (this.SwitchScript.CurrentState == Switch.SwitchState.ON)
            return true;

        if (!this.OneWay)
            return false;

        return true;
    }

    /*private void onStateChange(Switch.SwitchState state)
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
    }*/
}
