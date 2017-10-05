using UnityEngine;

public class AreaSwitch : MonoBehaviour
{
    public IntegerCollider DetectionCollider;
    public Switch SwitchScript;
    public LayerMask DetectionLayers;
    public bool OneWay = false;
    public int TwoWayResetDuration = 0;

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
        GameObject collided = this.DetectionCollider.CollideFirst(0, 0, this.DetectionLayers, null);

        if (!this.OneWay)
            _twoWayTimer.update();

        if (!_beingPressed)
        {
            if (collided != null)
            {
                _beingPressed = true;
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
        }
        else
        {
            if (collided == null)
            {
                _beingPressed = false;
                if (canSwitch())
                    this.SwitchScript.ToggleSwitch();
            }
        }
    }

    /**
     * Private
     */
    private bool _beingPressed;
    private Timer _twoWayTimer;

    private bool canSwitch()
    {
        if (!this.OneWay)
            return _twoWayTimer.Completed;
        return this.SwitchScript.CurrentState == this.SwitchScript.DefaultState;
    }
}
